using System;
using System.Collections.Generic;
using System.Linq;
using Accord.IO;
using Accord.MachineLearning;
using Accord.MachineLearning.Bayes;
using Accord.Math;
using Accord.Math.Optimization.Losses;
using Accord.Statistics.Distributions.Fitting;
using Accord.Statistics.Distributions.Univariate;
using LiteDB;
using MachineLearningActorSystem.Core;
using MachineLearningActorSystem.Events;
using MachineLearningActorSystem.Models;

namespace MachineLearningActorSystem.Actors.Classifiers
{
    public class NaiveBayesActor : BaseActor
    {

        private GridSearchParameterCollection _bestParameters = null;

        public NaiveBayesActor()
        {
            Receive<CrossValidationEvent>(crossValidationEvent =>
            {
                logger.Info($"{crossValidationEvent.GetType().Name} Received");
                var model = CrossValidationTraining(crossValidationEvent.Data);
                Sender.Tell(model, Self);
            });

            Receive<BootstrapEvent>(bootstrapEvent =>
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

            crossvalidation.Fitting = delegate (int k, int[] indicesTrain, int[] indicesValidation)
            {
                var trainingInputs = singleClassDataModel.Inputs.Get(indicesTrain);
                var trainingOutputs = singleClassDataModel.Outputs.Get(indicesTrain);

                var validationInputs = singleClassDataModel.Inputs.Get(indicesValidation);
                var validationOutputs = singleClassDataModel.Outputs.Get(indicesValidation);

                var bayes = TrainParameters(
                    trainingInputs,
                    trainingOutputs);

                var trainingError = new ZeroOneLoss(trainingOutputs).Loss(bayes.Decide(trainingInputs));
                var validationError = new ZeroOneLoss(validationOutputs).Loss(bayes.Decide(validationInputs));

                classifierModels.Add(new ClassifierModel
                {
                    BatchId = batchId.ToString(),
                    DataSetName = singleClassDataModel.Name,
                    Type = "NaiveBayes",
                    Teacher = new TeacherModel
                    {
                        Type = "NormalDistribution"
                    },
                    Validator = new ValidatorModel
                    {
                        Type = "CrossValidation",
                        Samples = samples,
                        Folds = folds
                    },
                    HyperParameters = new HyperParametersModel
                    {
                        Diagonal = _bestParameters.Single(x => x.Name == "normal-diagonal").Value > 0,
                        Robust = _bestParameters.Single(x => x.Name == "normal-robust").Value > 0
                    },
                    TrainingError = trainingError,
                    ValidationError = validationError,
                    TrainedModelBinary = Config.CoreSaveModelBinaries ? bayes.Save() : null
                });

                return new CrossValidationValues(bayes, trainingError, validationError);
            };

            var cvResult = crossvalidation.Compute();
            var cvMinError = cvResult.Models.Min(x => x.ValidationValue);
            var bestCvResult = cvResult.Models.Single(x => x.ValidationValue == cvMinError);
            var bestModel = (NaiveBayes<NormalDistribution>)bestCvResult.Model;

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

            KeyValuePair<double, NaiveBayes<NormalDistribution>> bestModel = new KeyValuePair<double, NaiveBayes<NormalDistribution>>(10, null);

            bootstrapValidation.Fitting = delegate (int[] indicesTrain, int[] indicesValidation)
            {
                var trainingInputs = singleClassDataModel.Inputs.Get(indicesTrain);
                var trainingOutputs = singleClassDataModel.Outputs.Get(indicesTrain);

                var validationInputs = singleClassDataModel.Inputs.Get(indicesValidation);
                var validationOutputs = singleClassDataModel.Outputs.Get(indicesValidation);

                var bayes = TrainParameters(
                    trainingInputs,
                    trainingOutputs);

                var trainingError = new ZeroOneLoss(trainingOutputs).Loss(bayes.Decide(trainingInputs));
                var validationError = new ZeroOneLoss(validationOutputs).Loss(bayes.Decide(validationInputs));

                classifierModels.Add(new ClassifierModel
                {
                    BatchId = batchId.ToString(),
                    DataSetName = singleClassDataModel.Name,
                    Type = "NaiveBayes",
                    Teacher = new TeacherModel
                    {
                        Type = "NormalDistribution"
                    },
                    Validator = new ValidatorModel
                    {
                        Type = "Bootstrap",
                        Samples = samples,
                        SubSamples = subSamples
                    },
                    HyperParameters = new HyperParametersModel
                    {
                        Diagonal = _bestParameters.Single(x => x.Name == "normal-diagonal").Value > 0,
                        Robust = _bestParameters.Single(x => x.Name == "normal-robust").Value > 0
                    },
                    TrainingError = trainingError,
                    ValidationError = validationError,
                    TrainedModelBinary = Config.CoreSaveModelBinaries ? bayes.Save() : null
                });

                if (bestModel.Key > validationError)
                {
                    bestModel = new KeyValuePair<double, NaiveBayes<NormalDistribution>>(validationError, bayes);
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

        NaiveBayes<NormalDistribution> TrainParameters(double[][] trainingInputs, int[] trainingOutputs)
        {
            NaiveBayes<NormalDistribution> bayes = null;

            if (_bestParameters == null)
            {
                GridSearchRange[] ranges =
                {
                        new GridSearchRange("normal-diagonal", new double[] { 0, 1 } ),
                        new GridSearchRange("normal-robust", new double[] { 0, 1 } )
                };

                var gridsearch = new GridSearch<NaiveBayes<NormalDistribution>>(ranges)
                {
                    Fitting = delegate (GridSearchParameterCollection parameters, out double error)
                    {
                        var teacher = new NaiveBayesLearning<NormalDistribution>
                        {
                            Options =
                            {
                                InnerOption = new NormalOptions
                                {
                                    Regularization = 1e-5, // to avoid zero variances
                                    Diagonal = parameters["normal-diagonal"].Value > 0,
                                    Robust = parameters["normal-robust"].Value > 0
                                },
                            }
                        };
                        var gridsearchBayes = teacher.Learn(trainingInputs, trainingOutputs);

                        teacher.Learn(trainingInputs, trainingOutputs);

                        error = new ZeroOneLoss(trainingOutputs).Loss(gridsearchBayes.Decide(trainingInputs));

                        return gridsearchBayes;
                    }
                };

                double gsMinError;
                bayes = gridsearch.Compute(out _bestParameters, out gsMinError);
            }
            else
            {
                var teacher = new NaiveBayesLearning<NormalDistribution>
                {
                    Options =
                    {
                        InnerOption = new NormalOptions
                        {
                            Regularization = 1e-5, // to avoid zero variances
                            Diagonal = _bestParameters.Single(x => x.Name == "normal-diagonal").Value > 0,
                            Robust = _bestParameters.Single(x => x.Name == "normal-robust").Value > 0,
                        }
                    }
                };
                bayes = teacher.Learn(trainingInputs, trainingOutputs);
            }

            return bayes;
        }
    }
}