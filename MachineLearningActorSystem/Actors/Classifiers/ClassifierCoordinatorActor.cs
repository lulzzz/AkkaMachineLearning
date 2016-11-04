using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using Accord.MachineLearning;
using Accord.MachineLearning.Bayes;
using Accord.MachineLearning.DecisionTrees;
using Accord.Math;
using Accord.Neuro;
using Accord.Statistics.Analysis;
using Accord.Statistics.Distributions.Univariate;
using Accord.Statistics.Filters;
using Akka.Actor;
using MachineLearningActorSystem.Core;
using MachineLearningActorSystem.Events;
using MachineLearningActorSystem.Models;
using Newtonsoft.Json;

namespace MachineLearningActorSystem.Actors.Classifiers
{
    public class ClassifierCoordinatorActor : BaseActor
    {
        private readonly IActorRef _decisionTreeActor = Context.ActorOf(Props.Create<DecisionTreeActor>(),
            typeof(DecisionTreeActor).Name);

        private readonly IActorRef _knnActor = Context.ActorOf(Props.Create<KnnActor>(), typeof(KnnActor).Name);

        private readonly IActorRef _naiveBayesActor = Context.ActorOf(Props.Create<NaiveBayesActor>(),
            typeof(NaiveBayesActor).Name);

        public ClassifierCoordinatorActor()
        {
            Receive<FileClassifierDataSourceEvent>(fileDataSourceEvent =>
            {
                logger.Info($"{fileDataSourceEvent.GetType().Name} Received");
                var classifiers = ReadFile(fileDataSourceEvent);
                StartClassifiers(classifiers, fileDataSourceEvent.Classifier);
            });
        }

        private Models.Classifiers ReadFile(FileClassifierDataSourceEvent dataSource)
        {
            var classifiers = XmlHelper.LoadClassifiers();
            if ( /*dataSource.PrintLastRun && */(classifiers != null)
                                                && (classifiers.SingleClassDataModels != null) &&
                                                (classifiers.ClassifierModels != null)
                                                && !classifiers.SingleClassDataModels.Any() &&
                                                !classifiers.ClassifierModels.Any())
            {
                logger.Info("Classifier data file already exists.. using xml");
                return classifiers;
            }

            try
            {
                logger.Info("Reading classifier data file started");

                var data = File.ReadAllText(dataSource.DataSourceFilePath);

                var lines = data.TrimEnd('\n').Split('\n');
                var inputColumnNames = lines[0].Replace("attribute-names:", "").Split(',');
                var outputColumn = new KeyValuePair<int, string>(int.Parse(lines[2].Replace("class-index:", "")),
                    lines[1].Replace("class-name:", ""));

                var allColumns = new List<string>();
                allColumns.Add(outputColumn.Value);
                allColumns.AddRange(inputColumnNames);

                var dataSetName = Path.GetFileNameWithoutExtension(dataSource.DataSourceFilePath);

                var table = new DataTable(dataSetName);
                table.Columns.Add(outputColumn.Value);
                foreach (var inputColumnName in inputColumnNames)
                    table.Columns.Add(inputColumnName);

                foreach (var line in lines.Get(3, lines.Length - 1))
                    table.Rows.Add(line.Split(','));

                var codebook = new Codification(table, allColumns.ToArray());
                var classes = new List<KeyValuePair<int, string>>();
                foreach (var c in codebook.Columns[0].Mapping)
                    classes.Add(new KeyValuePair<int, string>(c.Value, c.Key));

                var symbols = codebook.Apply(table);
                var inputs = symbols.ToArray(inputColumnNames);
                var outputs = symbols.ToArray<int>(outputColumn.Value);

                logger.Info("Reading classifier data file completed");

                classifiers = new Models.Classifiers();
                classifiers.SingleClassDataModels = new List<SingleClassDataModel>
                {
                    new SingleClassDataModel
                    {
                        Inputs = inputs,
                        Outputs = outputs,
                        AttributeNames = inputColumnNames,
                        ClassAttributeName = outputColumn.Value,
                        Name = dataSetName,
                        Classes = classes
                    }
                };

                XmlHelper.SaveClassifiers(classifiers);
            }
            catch (Exception ex)
            {
                logger.Error($"Failed to load data from file: {dataSource.DataSourceFilePath}. Error: {ex.Message}");
            }

            return classifiers;
        }

        private void StartClassifiers(Models.Classifiers classifiers,
            FileClassifierDataSourceEvent.ClassifierMethod classifierMethod)
        {
            logger.Info($"Starting Classifiers with config: {classifierMethod}");
            var singleClassDataModel = classifiers.SingleClassDataModels.First();

            var classifierResultEvents = new List<ClassifierResultEvent>();

            if ((classifierMethod == FileClassifierDataSourceEvent.ClassifierMethod.DECISIONTREEBS) ||
                (classifierMethod == FileClassifierDataSourceEvent.ClassifierMethod.FINDBEST))
                classifierResultEvents.Add(
                    (ClassifierResultEvent) _decisionTreeActor.Ask(new BootstrapEvent(classifiers)).Result);
            if ((classifierMethod == FileClassifierDataSourceEvent.ClassifierMethod.DECISIONTREECV) ||
                (classifierMethod == FileClassifierDataSourceEvent.ClassifierMethod.FINDBEST))
                classifierResultEvents.Add(
                    (ClassifierResultEvent) _decisionTreeActor.Ask(new CrossValidationEvent(classifiers)).Result);
            if ((classifierMethod == FileClassifierDataSourceEvent.ClassifierMethod.KNNBS) ||
                (classifierMethod == FileClassifierDataSourceEvent.ClassifierMethod.FINDBEST))
                classifierResultEvents.Add((ClassifierResultEvent) _knnActor.Ask(new BootstrapEvent(classifiers)).Result);
            if ((classifierMethod == FileClassifierDataSourceEvent.ClassifierMethod.KNNCV) ||
                (classifierMethod == FileClassifierDataSourceEvent.ClassifierMethod.FINDBEST))
                classifierResultEvents.Add(
                    (ClassifierResultEvent) _knnActor.Ask(new CrossValidationEvent(classifiers)).Result);
            if ((classifierMethod == FileClassifierDataSourceEvent.ClassifierMethod.NAIVEBAYESBS) ||
                (classifierMethod == FileClassifierDataSourceEvent.ClassifierMethod.FINDBEST))
                classifierResultEvents.Add(
                    (ClassifierResultEvent) _naiveBayesActor.Ask(new CrossValidationEvent(classifiers)).Result);
            if ((classifierMethod == FileClassifierDataSourceEvent.ClassifierMethod.NAIVEBAYESCV) ||
                (classifierMethod == FileClassifierDataSourceEvent.ClassifierMethod.FINDBEST))
                classifierResultEvents.Add(
                    (ClassifierResultEvent) _naiveBayesActor.Ask(new BootstrapEvent(classifiers)).Result);

            var minError = classifierResultEvents.Min(x => x.Error);
            var bestClassifier = classifierResultEvents.First(x => x.Error == minError);

            var tree = bestClassifier.Classifier as DecisionTree;
            if (tree != null)
                ComputeConfusionMatrix(singleClassDataModel, tree, bestClassifier.SplittingMethod);

            var knn = bestClassifier.Classifier as KNearestNeighbors;
            if (knn != null)
                ComputeConfusionMatrix(singleClassDataModel, knn, bestClassifier.SplittingMethod);

            var bayes = bestClassifier.Classifier as NaiveBayes<NormalDistribution>;
            if (bayes != null)
                ComputeConfusionMatrix(singleClassDataModel, bayes, bestClassifier.SplittingMethod);

            var ann = bestClassifier.Classifier as ActivationNetwork;
            if (ann != null)
                ComputeConfusionMatrix(singleClassDataModel, ann, bestClassifier.SplittingMethod);

            var classifierModels =
                XmlHelper.LoadClassifiers().ClassifierModels.FirstOrDefault(x => x.BatchId == bestClassifier.BatchId);
            Console.Write(
                $"Best Classifier Details:\n {JsonConvert.SerializeObject(classifierModels, Formatting.Indented)}\n");
            logger.Info($"Classifier for completed with config: {classifierMethod}");
        }

        protected void ComputeConfusionMatrix(SingleClassDataModel singleClassDataModel, DecisionTree tree,
            string splittingMethod)
        {
            try
            {
                logger.Info($"Printing ConfusionMatrix");
                var compute = tree.ToExpression().Compile();

                var inputs = singleClassDataModel.Inputs;
                var actual = new int[inputs.Count()];
                for (var i = 0; i < inputs.Count(); i++)
                    actual[i] = compute(inputs[i]);
                var expected = singleClassDataModel.Outputs;

                var cms = new List<ConfusionMatrix>();
                for (var i = 0; i < singleClassDataModel.Classes.Count; i++)
                    cms.Add(new ConfusionMatrix(actual, expected, i));

                Console.Write(
                    $"{StringHelper.PrintConfusionMatrixString(cms, singleClassDataModel.Classes, singleClassDataModel.Name, "DecisionTree", splittingMethod)}\n");
            }
            catch (Exception ex)
            {
                logger.Warn($"Unable to load or analyse trained DecisionTree for this data: {ex.Message}");
            }
        }

        protected void ComputeConfusionMatrix(SingleClassDataModel singleClassDataModel,
            NaiveBayes<NormalDistribution> bayes, string splittingMethod)
        {
            try
            {
                logger.Info($"Printing ConfusionMatrix");
                var inputs = singleClassDataModel.Inputs;
                var actual = new int[inputs.Count()];
                for (var i = 0; i < inputs.Count(); i++)
                    actual[i] = bayes.Decide(inputs[i]);
                var expected = singleClassDataModel.Outputs;

                var cms = new List<ConfusionMatrix>();
                for (var i = 0; i < singleClassDataModel.Classes.Count; i++)
                    cms.Add(new ConfusionMatrix(actual, expected, i));

                Console.Write(
                    $"{StringHelper.PrintConfusionMatrixString(cms, singleClassDataModel.Classes, singleClassDataModel.Name, "NaiveBayes", splittingMethod)}\n");
            }
            catch (Exception ex)
            {
                logger.Warn($"Unable to load or analyse trained NaiveBayes for this data: {ex.Message}");
            }
        }

        protected void ComputeConfusionMatrix(SingleClassDataModel singleClassDataModel, KNearestNeighbors knn,
            string splittingMethod)
        {
            try
            {
                logger.Info($"Printing ConfusionMatrix");
                var inputs = singleClassDataModel.Inputs;
                var actual = new int[inputs.Count()];
                for (var i = 0; i < inputs.Count(); i++)
                    actual[i] = knn.Compute(inputs[i]);
                var expected = singleClassDataModel.Outputs;

                var cms = new List<ConfusionMatrix>();
                for (var i = 0; i < singleClassDataModel.Classes.Count; i++)
                    cms.Add(new ConfusionMatrix(actual, expected, i));

                Console.Write(
                    $"{StringHelper.PrintConfusionMatrixString(cms, singleClassDataModel.Classes, singleClassDataModel.Name, "KNearestNeighbors", splittingMethod)}\n");
            }
            catch (Exception ex)
            {
                logger.Warn($"Unable to load or analyse trained KNearestNeighbors for this data: {ex.Message}");
            }
        }

        protected void ComputeConfusionMatrix(SingleClassDataModel singleClassDataModel, ActivationNetwork ann,
            string splittingMethod)
        {
            try
            {
                logger.Info($"Printing ConfusionMatrix");
                var inputs = singleClassDataModel.Inputs;
                var actual = inputs.Apply(ann.Compute).GetColumn(0).Apply(Math.Sign);
                var expected = singleClassDataModel.Outputs;

                var cms = new List<ConfusionMatrix>();
                for (var i = 0; i < singleClassDataModel.Classes.Count; i++)
                    cms.Add(new ConfusionMatrix(actual, expected, i));

                Console.Write(
                    $"{StringHelper.PrintConfusionMatrixString(cms, singleClassDataModel.Classes, singleClassDataModel.Name, "NeuralNetwork", splittingMethod)}\n");
            }
            catch (Exception ex)
            {
                logger.Warn($"Unable to load or analyse trained NeuralNetwork for this data: {ex.Message}");
            }
        }
    }
}