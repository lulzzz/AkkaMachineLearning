//using System;
//using System.Collections.Generic;
//using System.Linq;
//using Accord.IO;
//using Accord.MachineLearning;
//using Accord.Math;
//using Accord.Math.Optimization.Losses;
//using Accord.Neuro;
//using Accord.Neuro.Learning;
//using LiteDB;
//using MachineLearningActorSystem.Core;
//using MachineLearningActorSystem.Events;
//using MachineLearningActorSystem.Models;

//namespace MachineLearningActorSystem.Actors.Classifiers
//{
//    /// <summary>
//    /// This actor is not computing the correct output values.
//    /// </summary>
//    public class NeuralNetworkActor : BaseActor
//    {
//        private GridSearchParameterCollection _bestParameters = null;

//        public NeuralNetworkActor()
//        {
//            throw new NotImplementedException("This Actor is invalid and does not compute correct results.");

//            Receive<CrossValidationEvent>(crossValidationEvent =>
//            {
//                logger.Info($"{crossValidationEvent.GetType().Name} Received");
//                var model = CrossValidationTraining(crossValidationEvent.Data);
//                Sender.Tell(model, Self);
//            });

//            Receive<BootstrapEvent>(bootstrapEvent =>
//            {
//                logger.Info($"{bootstrapEvent.GetType().Name} Received");
//                var model = BootstrapTraining(bootstrapEvent.Data);
//                Sender.Tell(model, Self);
//            });
//        }

//        private ClassifierResultEvent CrossValidationTraining(SingleClassDataModel singleClassDataModel)
//        {
//            var batchId = Guid.NewGuid();
//            var folds = Config.ClassifierCrossValidationFolds;
//            var samples = singleClassDataModel.Inputs.Length;
//            var crossvalidation = new CrossValidation(samples, folds);
//            var classifierModels = new List<ClassifierModel>();

//            crossvalidation.Fitting = delegate (int k, int[] indicesTrain, int[] indicesValidation)
//            {
//                var trainingInputs = singleClassDataModel.Inputs.Get(indicesTrain);
//                var trainingOutputs = singleClassDataModel.Outputs.Get(indicesTrain);

//                var validationInputs = singleClassDataModel.Inputs.Get(indicesValidation);
//                var validationOutputs = singleClassDataModel.Outputs.Get(indicesValidation);

//                var ann = TrainParameters(
//                    trainingInputs,
//                    trainingOutputs,
//                    singleClassDataModel.AttributeNames.Length);

//                var trainingPredicted = trainingInputs.Apply(ann.Compute).GetColumn(0).Apply(System.Math.Sign);
//                var validationPredicted = validationInputs.Apply(ann.Compute).GetColumn(0).Apply(System.Math.Sign);

//                var trainingError = new ZeroOneLoss(trainingOutputs).Loss(trainingPredicted);
//                var validationError = new ZeroOneLoss(validationOutputs).Loss(validationPredicted);

//                classifierModels.Add(new ClassifierModel
//                {
//                    BatchId = batchId.ToString(),
//                    DataSetName = singleClassDataModel.Name,
//                    Type = "NeuralNetwork",
//                    Teacher = new TeacherModel
//                    {
//                        Type = "BackPropogation"
//                    },
//                    Validator = new ValidatorModel
//                    {
//                        Type = "CrossValidation",
//                        Samples = samples,
//                        Folds = folds
//                    },
//                    HyperParameters = new HyperParametersModel
//                    {
//                        LearningRate = _bestParameters.Single(x => x.Name == "learningRate").Value,
//                        SigmoidAlphaValue = _bestParameters.Single(x => x.Name == "sigmoidAlphaValue").Value,
//                        NeuronsInFirstLayer = (int)_bestParameters.Single(x => x.Name == "neuronsInFirstLayer").Value,
//                        Iterations = (int)_bestParameters.Single(x => x.Name == "iterations").Value,
//                        UseNguyenWidrow = _bestParameters.Single(x => x.Name == "useNguyenWidrow").Value > 0,
//                        UseSameWeights = _bestParameters.Single(x => x.Name == "useSameWeights").Value > 0,
//                        Momentum = _bestParameters.Single(x => x.Name == "momentum").Value
//                    },
//                    TrainingError = trainingError,
//                    ValidationError = validationError,
//                    TrainedModelBinary = Config.CoreSaveModelBinaries ? ann.Save() : null
//                });

//                return new CrossValidationValues(ann, trainingError, validationError);
//            };

//            var cvResult = crossvalidation.Compute();
//            var cvMinError = cvResult.Models.Min(x => x.ValidationValue);
//            var bestCvResult = cvResult.Models.Single(x => x.ValidationValue == cvMinError);
//            var bestModel = (ActivationNetwork)bestCvResult.Model;

//            logger.Info($"\n{StringHelper.PrintValidationResultString(classifierModels, singleClassDataModel.Name, cvResult)}");

//            using (var db = new LiteDatabase(@".\Resources\MlAsData.db"))
//            {
//                var classifierModelCollection = db.GetCollection<ClassifierModel>("ClassifierModel");
//                classifierModelCollection.Insert(classifierModels);
//            }

//            return new ClassifierResultEvent(bestModel, cvMinError, "CrossValidation", batchId.ToString());
//        }

//        private ClassifierResultEvent BootstrapTraining(SingleClassDataModel singleClassDataModel)
//        {
//            var batchId = Guid.NewGuid();
//            int subSamples = Config.ClassifierBootstrapSubSamples;
//            var samples = singleClassDataModel.Inputs.Length;
//            var bootstrapValidation = new Bootstrap(samples, subSamples);
//            var classifierModels = new List<ClassifierModel>();

//            KeyValuePair<double, ActivationNetwork> bestModel = new KeyValuePair<double, ActivationNetwork>(10, null);

//            bootstrapValidation.Fitting = delegate (int[] indicesTrain, int[] indicesValidation)
//            {
//                var trainingInputs = singleClassDataModel.Inputs.Get(indicesTrain);
//                var trainingOutputs = singleClassDataModel.Outputs.Get(indicesTrain);

//                var validationInputs = singleClassDataModel.Inputs.Get(indicesValidation);
//                var validationOutputs = singleClassDataModel.Outputs.Get(indicesValidation);

//                var ann = TrainParameters(
//                    trainingInputs,
//                    trainingOutputs,
//                    singleClassDataModel.AttributeNames.Length);

//                var trainingPredicted = trainingInputs.Apply(ann.Compute).GetColumn(0).Apply(System.Math.Sign);
//                var validationPredicted = validationInputs.Apply(ann.Compute).GetColumn(0).Apply(System.Math.Sign);

//                var trainingError = new ZeroOneLoss(trainingOutputs).Loss(trainingPredicted);
//                var validationError = new ZeroOneLoss(validationOutputs).Loss(validationPredicted);

//                classifierModels.Add(new ClassifierModel
//                {
//                    BatchId = batchId.ToString(),
//                    DataSetName = singleClassDataModel.Name,
//                    Type = "NeuralNetwork",
//                    Teacher = new TeacherModel
//                    {
//                        Type = "BackPropogation"
//                    },
//                    Validator = new ValidatorModel
//                    {
//                        Type = "Bootstrap",
//                        Samples = samples,
//                        SubSamples = subSamples
//                    },
//                    HyperParameters = new HyperParametersModel
//                    {
//                        LearningRate = _bestParameters.Single(x => x.Name == "learningRate").Value,
//                        SigmoidAlphaValue = _bestParameters.Single(x => x.Name == "sigmoidAlphaValue").Value,
//                        NeuronsInFirstLayer = (int)_bestParameters.Single(x => x.Name == "neuronsInFirstLayer").Value,
//                        Iterations = (int)_bestParameters.Single(x => x.Name == "iterations").Value,
//                        UseNguyenWidrow = _bestParameters.Single(x => x.Name == "useNguyenWidrow").Value > 0,
//                        UseSameWeights = _bestParameters.Single(x => x.Name == "useSameWeights").Value > 0,
//                        Momentum = _bestParameters.Single(x => x.Name == "momentum").Value
//                    },
//                    TrainingError = trainingError,
//                    ValidationError = validationError,
//                    TrainedModelBinary = Config.CoreSaveModelBinaries ? ann.Save() : null
//                });

//                if (bestModel.Key > validationError)
//                {
//                    bestModel = new KeyValuePair<double, ActivationNetwork>(validationError, ann);
//                }

//                return new BootstrapValues(trainingError, validationError);
//            };

//            var bsResult = bootstrapValidation.Compute();

//            logger.Info($"\n{StringHelper.PrintValidationResultString(classifierModels, singleClassDataModel.Name, bsResult)}");

//            using (var db = new LiteDatabase(@".\Resources\MlAsData.db"))
//            {
//                var classifierModelCollection = db.GetCollection<ClassifierModel>("ClassifierModel");
//                classifierModelCollection.Insert(classifierModels);
//            }

//            return new ClassifierResultEvent(bestModel.Value, bestModel.Key, "BootStrap", batchId.ToString());
//        }

//        ActivationNetwork TrainParameters(double[][] trainingInputs, int[] trainingOutputs, int numberOfAttributes)
//        {
//            ActivationNetwork ann = null;

//            if (_bestParameters == null)
//            {
//                GridSearchRange[] ranges =
//                {
//                        new GridSearchRange("learningRate", new double[] { 0.1, 0.5, 1 } ),
//                        new GridSearchRange("sigmoidAlphaValue", new double[] { 0.5, 1, 2 } ),
//                        new GridSearchRange("neuronsInFirstLayer", new double[] { 5, 10, 15 } ),
//                        new GridSearchRange("iterations", new double[] { 10, 25, 50 } ),
//                        new GridSearchRange("useNguyenWidrow", new double[] { 0, 1 } ),
//                        new GridSearchRange("useSameWeights", new double[] { 0, 1 } ),
//                        new GridSearchRange("momentum", new double[] { 0, 0.5, 1 } )
//                };

//                var gridsearch = new GridSearch<ActivationNetwork>(ranges)
//                {
//                    Fitting = delegate (GridSearchParameterCollection parameters, out double error)
//                    {
//                        double learningRate = parameters["learningRate"].Value;
//                        double sigmoidAlphaValue = parameters["sigmoidAlphaValue"].Value;
//                        int neuronsInFirstLayer = (int)parameters["neuronsInFirstLayer"].Value;
//                        int iterations = (int)parameters["iterations"].Value;
//                        bool useNguyenWidrow = parameters["useNguyenWidrow"].Value > 0;
//                        bool useSameWeights = parameters["useSameWeights"].Value > 0;
//                        double momentum = parameters["momentum"].Value;

//                        var gridsearchAnn = new ActivationNetwork(
//                            new BipolarSigmoidFunction(sigmoidAlphaValue),
//                            numberOfAttributes,
//                            neuronsInFirstLayer,
//                            1);

//                        if (useNguyenWidrow)
//                        {
//                            if (useSameWeights)
//                                Accord.Math.Random.Generator.Seed = 0;

//                            NguyenWidrow initializer = new NguyenWidrow(gridsearchAnn);
//                            initializer.Randomize();
//                        }

//                        var teacher = new BackPropagationLearning(gridsearchAnn);
//                        teacher.LearningRate = learningRate;
//                        teacher.Momentum = momentum;

//                        var trainingOutputs2 = trainingOutputs.ToDouble().ToJagged();

//                        error = 0;

//                        while (iterations-- > 0)
//                        {
//                            error = teacher.RunEpoch(trainingInputs, trainingOutputs2)/trainingInputs.Length;
//                        }

//                        //error = new ZeroOneLoss(trainingOutputs).Loss(trainingInputs.Apply(gridsearchAnn.Compute).GetColumn(0).Apply(System.Math.Sign));

//                        return gridsearchAnn;
//                    }
//                };

//                double gsMinError;
//                ann = gridsearch.Compute(out _bestParameters, out gsMinError);
//            }
//            else
//            {
//                double learningRate = _bestParameters.Single(x => x.Name == "learningRate").Value;
//                double sigmoidAlphaValue = _bestParameters.Single(x => x.Name == "sigmoidAlphaValue").Value;
//                int neuronsInFirstLayer = (int)_bestParameters.Single(x => x.Name == "neuronsInFirstLayer").Value;
//                int iterations = (int)_bestParameters.Single(x => x.Name == "iterations").Value;
//                bool useNguyenWidrow = _bestParameters.Single(x => x.Name == "useNguyenWidrow").Value > 0;
//                bool useSameWeights =  _bestParameters.Single(x => x.Name == "useSameWeights").Value > 0;
//                double momentum = _bestParameters.Single(x => x.Name == "momentum").Value;

//                ann = new ActivationNetwork(
//                    new BipolarSigmoidFunction(sigmoidAlphaValue),
//                    numberOfAttributes,
//                    neuronsInFirstLayer,
//                    1);

//                if (useNguyenWidrow)
//                {
//                    if (useSameWeights)
//                        Accord.Math.Random.Generator.Seed = 0;

//                    NguyenWidrow initializer = new NguyenWidrow(ann);
//                    initializer.Randomize();
//                }

//                var teacher = new BackPropagationLearning(ann);
//                teacher.LearningRate = learningRate;
//                teacher.Momentum = momentum;

//                var trainingOutputs2 = trainingOutputs.ToDouble().ToJagged();

//                var error = 0.0;

//                while (iterations-- > 0)
//                {
//                    error = teacher.RunEpoch(trainingInputs, trainingOutputs2) / trainingInputs.Length;
//                }
//            }

//            return ann;
//        }
//    }
//}