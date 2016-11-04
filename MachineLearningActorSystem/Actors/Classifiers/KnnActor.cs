using System;
using System.Collections.Generic;
using System.Linq;
using Accord.IO;
using Accord.MachineLearning;
using Accord.Math;
using Accord.Math.Optimization.Losses;
using LiteDB;
using MachineLearningActorSystem.Core;
using MachineLearningActorSystem.Events;
using MachineLearningActorSystem.Models;

namespace MachineLearningActorSystem.Actors.Classifiers
{
    public class KnnActor : BaseActor
    {

        private GridSearchParameterCollection _bestParameters = null;

        public KnnActor()
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

                var knn = TrainParameters(
                    trainingInputs,
                    trainingOutputs,
                    singleClassDataModel.Classes.Count);

                var trainingPredicted = new int[trainingInputs.Count()];
                for (var i = 0; i < validationInputs.Count(); i++)
                    trainingPredicted[i] = knn.Compute(trainingInputs[i]);

                var validationPredicted = new int[validationInputs.Count()];
                for (var i = 0; i < validationInputs.Count(); i++)
                    validationPredicted[i] = knn.Compute(validationInputs[i]);

                var trainingError = new ZeroOneLoss(trainingOutputs).Loss(trainingPredicted);
                var validationError = new ZeroOneLoss(validationOutputs).Loss(validationPredicted);

                classifierModels.Add(new ClassifierModel
                {
                    BatchId = batchId.ToString(),
                    DataSetName = singleClassDataModel.Name,
                    Type = "KNearestNeighbors",
                    Teacher = new TeacherModel
                    {
                        Type = "KNN"
                    },
                    Validator = new ValidatorModel
                    {
                        Type = "CrossValidation",
                        Samples = samples,
                        Folds = folds
                    },
                    HyperParameters = new HyperParametersModel
                    {
                        K = (int)_bestParameters.Single(x => x.Name == "k").Value
                    },
                    TrainingError = trainingError,
                    ValidationError = validationError,
                    TrainedModelBinary = Config.CoreSaveModelBinaries ? knn.Save() : null
                });

                return new CrossValidationValues(knn, trainingError, validationError);
            };

            var cvResult = crossvalidation.Compute();
            var cvMinError = cvResult.Models.Min(x => x.ValidationValue);
            var bestCvResult = cvResult.Models.Single(x => x.ValidationValue == cvMinError);
            var bestModel = (KNearestNeighbors)bestCvResult.Model;

            logger.Info($"\n{StringHelper.PrintValidationResultString(classifierModels, singleClassDataModel.Name, cvResult)}");

            using (var db = new LiteDatabase(@".\Resources\MlAsData.db"))
            {
                var classifierModelCollection = db.GetCollection<ClassifierModel>("ClassifierModel");
                classifierModelCollection.Insert(classifierModels);
            }

            return new ClassifierResultEvent(bestModel,cvMinError, "CrossValidation", batchId.ToString());
        }

        private ClassifierResultEvent BootstrapTraining(SingleClassDataModel singleClassDataModel)
        {
            var batchId = Guid.NewGuid();
            int subSamples = Config.ClassifierBootstrapSubSamples;
            var samples = singleClassDataModel.Inputs.Length;
            var bootstrapValidation = new Bootstrap(samples, subSamples);
            var classifierModels = new List<ClassifierModel>();

            KeyValuePair<double, KNearestNeighbors> bestModel = new KeyValuePair<double, KNearestNeighbors>(10, null);

            bootstrapValidation.Fitting = delegate (int[] indicesTrain, int[] indicesValidation)
            {
                var trainingInputs = singleClassDataModel.Inputs.Get(indicesTrain);
                var trainingOutputs = singleClassDataModel.Outputs.Get(indicesTrain);

                var validationInputs = singleClassDataModel.Inputs.Get(indicesValidation);
                var validationOutputs = singleClassDataModel.Outputs.Get(indicesValidation);

                var knn = TrainParameters(
                    trainingInputs,
                    trainingOutputs,
                    singleClassDataModel.Classes.Count);

                var trainingPredicted = new int[trainingInputs.Count()];
                for (var i = 0; i < validationInputs.Count(); i++)
                    trainingPredicted[i] = knn.Compute(trainingInputs[i]);

                var validationPredicted = new int[validationInputs.Count()];
                for (var i = 0; i < validationInputs.Count(); i++)
                    validationPredicted[i] = knn.Compute(validationInputs[i]);

                var trainingError = new ZeroOneLoss(trainingOutputs).Loss(trainingPredicted);
                var validationError = new ZeroOneLoss(validationOutputs).Loss(validationPredicted);

                classifierModels.Add(new ClassifierModel
                {
                    BatchId = batchId.ToString(),
                    DataSetName = singleClassDataModel.Name,
                    Type = "KNearestNeighbors",
                    Teacher = new TeacherModel
                    {
                        Type = "KNN"
                    },
                    Validator = new ValidatorModel
                    {
                        Type = "Bootstrap",
                        Samples = samples,
                        SubSamples = subSamples
                    },
                    HyperParameters = new HyperParametersModel
                    {
                        K = (int)_bestParameters.Single(x => x.Name == "k").Value
                    },
                    TrainingError = trainingError,
                    ValidationError = validationError,
                    TrainedModelBinary = Config.CoreSaveModelBinaries ? knn.Save() : null
                });

                if (bestModel.Key > validationError)
                {
                    bestModel = new KeyValuePair<double, KNearestNeighbors>(validationError, knn);
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

        KNearestNeighbors TrainParameters(double[][] trainingInputs, int[] trainingOutputs, int numberOfClasses)
        {
            KNearestNeighbors knn = null;

            if (_bestParameters == null)
            {
                GridSearchRange[] ranges =
                {
                        new GridSearchRange("k", new double[] { 1, 2, 3, 5, 8, 13, 21, 34, 55, 89, 144 } ),
                };

                var gridsearch = new GridSearch<KNearestNeighbors>(ranges)
                {
                    Fitting = delegate (GridSearchParameterCollection parameters, out double error)
                    {
                        int k = (int)parameters["k"].Value;
                        var gridsearchKnn =  new KNearestNeighbors(k, numberOfClasses, trainingInputs, trainingOutputs);

                        var trainingPredicted = new int[trainingInputs.Count()];
                        for (var i = 0; i < trainingInputs.Count(); i++)
                            trainingPredicted[i] = gridsearchKnn.Compute(trainingInputs[i]);

                        error = new ZeroOneLoss(trainingOutputs).Loss(trainingPredicted);

                        return gridsearchKnn;
                    }
                };

                double gsMinError;
                knn = gridsearch.Compute(out _bestParameters, out gsMinError);
            }
            else
            {
                int k = (int)_bestParameters.Single(x => x.Name == "k").Value;
                knn = new KNearestNeighbors(k, numberOfClasses, trainingInputs, trainingOutputs);
            }

            return knn;
        }
    }
}