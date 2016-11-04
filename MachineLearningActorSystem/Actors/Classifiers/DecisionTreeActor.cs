using System;
using System.Collections.Generic;
using System.Linq;
using Accord.IO;
using Accord.MachineLearning;
using Accord.MachineLearning.DecisionTrees;
using Accord.MachineLearning.DecisionTrees.Learning;
using Accord.Math;
using Accord.Math.Optimization.Losses;
using LiteDB;
using MachineLearningActorSystem.Core;
using MachineLearningActorSystem.Events;
using MachineLearningActorSystem.Models;

namespace MachineLearningActorSystem.Actors.Classifiers
{
    public class DecisionTreeActor : BaseActor
    {

        private GridSearchParameterCollection _bestParameters = null;

        public DecisionTreeActor()
        {
            Receive<CrossValidationEvent>(crossValidationEvent =>
            {
                logger.Info($"{crossValidationEvent.GetType().Name} Received");
                var model = CrossValidationTraining(crossValidationEvent.Data);
                Sender.Tell(model, Self);
            });

            Receive<BootstrapEvent>(bootstrapEvent=>
            {
                logger.Info($"{bootstrapEvent.GetType().Name} Received");
                var model = BootstrapTraining(bootstrapEvent.Data);
                Sender.Tell(model, Self);
            });
        }

        private ClassifierResultEvent CrossValidationTraining(SingleClassDataModel singleClassDataModel)
        {
            var batchId = Guid.NewGuid();
            var folds = Config.ClassifierCrossValidationFolds;
            var samples = singleClassDataModel.Inputs.Length;
            var crossvalidation = new CrossValidation(samples, folds);
            var classifierModels = new List<ClassifierModel>();
            var attributes = DecisionVariable.FromData(singleClassDataModel.Inputs);

            crossvalidation.Fitting = delegate(int k, int[] indicesTrain, int[] indicesValidation)
            {
                var trainingInputs = singleClassDataModel.Inputs.Get(indicesTrain);
                var trainingOutputs = singleClassDataModel.Outputs.Get(indicesTrain);

                var validationInputs = singleClassDataModel.Inputs.Get(indicesValidation);
                var validationOutputs = singleClassDataModel.Outputs.Get(indicesValidation);

                var tree = TrainParameters(
                    attributes, 
                    singleClassDataModel.Classes.Count, 
                    trainingInputs,
                    trainingOutputs);

                var trainingError = new ZeroOneLoss(trainingOutputs).Loss(tree.Decide(trainingInputs));
                var validationError = new ZeroOneLoss(validationOutputs).Loss(tree.Decide(validationInputs));

                classifierModels.Add(new ClassifierModel
                {
                    BatchId = batchId.ToString(),
                    DataSetName = singleClassDataModel.Name,
                    Type = "DecisionTree",
                    Teacher = new TeacherModel
                    {
                        Type = "C45"
                    },
                    Validator = new ValidatorModel
                    {
                        Type = "CrossValidation",
                        Samples = samples,
                        Folds = folds
                    },
                    HyperParameters = new HyperParametersModel
                    {
                        SplitStep = (int)_bestParameters.Single(x => x.Name == "splitstep").Value,
                        Join = (int)_bestParameters.Single(x => x.Name == "join").Value
                    },
                    TrainingError = trainingError,
                    ValidationError = validationError,
                    TrainedModelBinary = Config.CoreSaveModelBinaries ? tree.Save() : null
                });

                return new CrossValidationValues(tree, trainingError, validationError);
            };

            var cvResult = crossvalidation.Compute();
            var cvMinError = cvResult.Models.Min(x => x.ValidationValue);
            var bestCvResult = cvResult.Models.Single(x => x.ValidationValue == cvMinError);
            var bestModel = (DecisionTree)bestCvResult.Model;

            logger.Info($"\n{StringHelper.PrintValidationResultString(classifierModels, singleClassDataModel.Name, cvResult)}");

            using (var db = new LiteDatabase(@".\Resources\MlAsData.db"))
            {
                var classifierModelCollection = db.GetCollection<ClassifierModel>("ClassifierModel");
                classifierModelCollection.Insert(classifierModels);
            }

            return new ClassifierResultEvent(bestModel, cvMinError, "CrossValidation", batchId.ToString());
        }

        private ClassifierResultEvent BootstrapTraining(SingleClassDataModel singleClassDataModel)
        {
            var batchId = Guid.NewGuid();
            int subSamples = Config.ClassifierBootstrapSubSamples;
            var samples = singleClassDataModel.Inputs.Length;
            var bootstrapValidation = new Bootstrap(samples, subSamples);
            var classifierModels = new List<ClassifierModel>();
            var attributes = DecisionVariable.FromData(singleClassDataModel.Inputs);

            KeyValuePair<double, DecisionTree> bestModel = new KeyValuePair<double, DecisionTree>(10, null);

            bootstrapValidation.Fitting = delegate (int[] indicesTrain, int[] indicesValidation)
            {
                var trainingInputs = singleClassDataModel.Inputs.Get(indicesTrain);
                var trainingOutputs = singleClassDataModel.Outputs.Get(indicesTrain);

                var validationInputs = singleClassDataModel.Inputs.Get(indicesValidation);
                var validationOutputs = singleClassDataModel.Outputs.Get(indicesValidation);

                var tree = TrainParameters(
                    attributes,
                    singleClassDataModel.Classes.Count,
                    trainingInputs,
                    trainingOutputs);

                var trainingError = new ZeroOneLoss(trainingOutputs).Loss(tree.Decide(trainingInputs));
                var validationError = new ZeroOneLoss(validationOutputs).Loss(tree.Decide(validationInputs));

                classifierModels.Add(new ClassifierModel
                {
                    BatchId = batchId.ToString(),
                    DataSetName = singleClassDataModel.Name,
                    Type = "DecisionTree",
                    Teacher = new TeacherModel
                    {
                        Type = "C45"
                    },
                    Validator = new ValidatorModel
                    {
                        Type = "BootStrap",
                        Samples = samples,
                        SubSamples = subSamples
                    },
                    HyperParameters = new HyperParametersModel
                    {
                        SplitStep = (int)_bestParameters.Single(x => x.Name == "splitstep").Value,
                        Join = (int)_bestParameters.Single(x => x.Name == "join").Value
                    },
                    TrainingError = trainingError,
                    ValidationError = validationError,
                    TrainedModelBinary = Config.CoreSaveModelBinaries ? tree.Save() : null
                });

                if (bestModel.Key > validationError)
                {
                    bestModel = new KeyValuePair<double, DecisionTree>(validationError, tree);
                }

                return new BootstrapValues(trainingError, validationError);
            };

            var bsResult = bootstrapValidation.Compute();

            logger.Info($"\n{StringHelper.PrintValidationResultString(classifierModels, singleClassDataModel.Name, bsResult)}");

            using (var db = new LiteDatabase(@".\Resources\MlAsData.db"))
            {
                var classifierModelCollection = db.GetCollection<ClassifierModel>("ClassifierModel");
                classifierModelCollection.Insert(classifierModels);
            }

            return new ClassifierResultEvent(bestModel.Value, bestModel.Key, "BootStrap", batchId.ToString());
        }

        DecisionTree TrainParameters(DecisionVariable[] attributes, int numberOfClasses, double[][] trainingInputs, int[] trainingOutputs)
        {
            DecisionTree tree = null;

            if (_bestParameters == null)
            {
                GridSearchRange[] ranges =
                {
                        new GridSearchRange("join", new double[] { 1, 3, 8 } ),
                        new GridSearchRange("splitstep", new double[] { 1, 3, 8 } )
                    };

                var gridsearch = new GridSearch<DecisionTree>(ranges)
                {
                    Fitting = delegate (GridSearchParameterCollection parameters, out double error)
                    {
                        var gridsearchTree = new DecisionTree(attributes, numberOfClasses);

                        var teacher = new C45Learning(gridsearchTree)
                        {
                            Join = (int)parameters["join"].Value,
                            SplitStep = (int)parameters["splitstep"].Value
                        };

                        teacher.Learn(trainingInputs, trainingOutputs);

                        error = new ZeroOneLoss(trainingOutputs).Loss(gridsearchTree.Decide(trainingInputs));

                        return gridsearchTree;
                    }
                };

                double gsMinError;
                tree = gridsearch.Compute(out _bestParameters, out gsMinError);
            }
            else
            {
                tree = new DecisionTree(attributes, numberOfClasses);
                var teacher = new C45Learning(tree)
                {
                    SplitStep = (int)_bestParameters.Single(x => x.Name == "splitstep").Value,
                    Join = (int)_bestParameters.Single(x => x.Name == "join").Value
                };

                teacher.Learn(trainingInputs, trainingOutputs);
            }

            return tree;
        }
    }
}