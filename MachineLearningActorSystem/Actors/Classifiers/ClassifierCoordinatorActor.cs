namespace MachineLearningActorSystem.Actors.Classifiers
{
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
    using LiteDB;
    using Core;
    using Events;
    using Models;
    using ClassifierMethod = Events.FileClassifierDataSourceEvent.ClassifierMethod;

    public class ClassifierCoordinatorActor : BaseActor
    {
        private readonly IActorRef _decisionTreeActor = Context.ActorOf(Props.Create<DecisionTreeActor>(), typeof(DecisionTreeActor).Name);
        private readonly IActorRef _naiveBayesActor = Context.ActorOf(Props.Create<NaiveBayesActor>(), typeof(NaiveBayesActor).Name);
        private readonly IActorRef _knnActor = Context.ActorOf(Props.Create<KnnActor>(), typeof(KnnActor).Name);

        public ClassifierCoordinatorActor()
        {
            Receive<FileClassifierDataSourceEvent>(fileDataSourceEvent =>
            {
                logger.Info($"{fileDataSourceEvent.GetType().Name} Received");
                var singleClassDataModel = ReadFile(fileDataSourceEvent);
                StartClassifiers(singleClassDataModel, fileDataSourceEvent.Classifier);
            });
        }

        private SingleClassDataModel ReadFile(FileClassifierDataSourceEvent dataSource)
        {
            try
            {
                var data = File.ReadAllText(dataSource.DataSourceFilePath);

                string[] lines = data.TrimEnd('\n').Split('\n');
                var inputColumnNames = lines[0].Replace("attribute-names:","").Split(',');
                var outputColumn = new KeyValuePair<int, string>(int.Parse(lines[2].Replace("class-index:", "")),lines[1].Replace("class-name:", ""));

                var allColumns = new List<string>();
                allColumns.Add(outputColumn.Value);
                allColumns.AddRange(inputColumnNames);

                var dataSetName = Path.GetFileNameWithoutExtension(dataSource.DataSourceFilePath);

                DataTable table = new DataTable(dataSetName);
                table.Columns.Add(outputColumn.Value);
                foreach (var inputColumnName in inputColumnNames)
                {
                    table.Columns.Add(inputColumnName);
                }

                foreach (var line in lines.Get(3,lines.Length-1))
                {
                    table.Rows.Add(line.Split(','));
                }

                Codification codebook = new Codification(table, allColumns.ToArray());
                var classes = new List<KeyValuePair<int, string>>();
                foreach (var c in codebook.Columns[0].Mapping)
                {
                    classes.Add(new KeyValuePair<int, string>(c.Value, c.Key));
                }

                DataTable symbols = codebook.Apply(table);
                double[][] inputs = symbols.ToArray(inputColumnNames);
                int[] outputs = symbols.ToArray<int>(outputColumn.Value);

                // Open database (or create if not exits)
                SingleClassDataModel singleClassDataModel = null;
                using (var db = new LiteDatabase(@".\Resources\MlAsData.db"))
                {
                    var singleClassDataModels = db.GetCollection<SingleClassDataModel>("SingleClassDataModel");

                    singleClassDataModel = new SingleClassDataModel
                    {
                        Inputs = inputs,
                        Outputs = outputs,
                        AttributeNames = inputColumnNames,
                        ClassAttributeName = outputColumn.Value,
                        Name = dataSetName,
                        Classes = classes,
                    };

                    singleClassDataModels.Insert(singleClassDataModel);
                }

                return singleClassDataModel;

            }
            catch (Exception ex)
            {
                logger.Error($"Failed to load data from file: {dataSource.DataSourceFilePath}. Error: {ex.Message}");
            }

            return null;
        }

        void StartClassifiers(SingleClassDataModel singleClassDataModel, ClassifierMethod classifier)
        {

            List<ClassifierResultEvent> classifiers = new List<ClassifierResultEvent>();

            if (classifier == ClassifierMethod.DECISIONTREEBS || classifier == ClassifierMethod.FINDBEST)
            {
                classifiers.Add((ClassifierResultEvent)_decisionTreeActor.Ask(new BootstrapEvent(singleClassDataModel)).Result);
            }
            if (classifier == ClassifierMethod.DECISIONTREECV || classifier == ClassifierMethod.FINDBEST)
            {
                classifiers.Add((ClassifierResultEvent)_decisionTreeActor.Ask(new CrossValidationEvent(singleClassDataModel)).Result);
            }
            if (classifier == ClassifierMethod.KNNBS || classifier == ClassifierMethod.FINDBEST)
            {
                classifiers.Add((ClassifierResultEvent)_knnActor.Ask(new BootstrapEvent(singleClassDataModel)).Result);
            }
            if (classifier == ClassifierMethod.KNNCV || classifier == ClassifierMethod.FINDBEST)
            {
                classifiers.Add((ClassifierResultEvent)_knnActor.Ask(new CrossValidationEvent(singleClassDataModel)).Result);
            }
            if (classifier == ClassifierMethod.NAIVEBAYESBS || classifier == ClassifierMethod.FINDBEST)
            {
                classifiers.Add((ClassifierResultEvent)_naiveBayesActor.Ask(new CrossValidationEvent(singleClassDataModel)).Result);
            }
            if (classifier == ClassifierMethod.NAIVEBAYESCV || classifier == ClassifierMethod.FINDBEST)
            {
                classifiers.Add((ClassifierResultEvent)_naiveBayesActor.Ask(new BootstrapEvent(singleClassDataModel)).Result);
            }

            var minError = classifiers.Min(x => x.Error);
            var bestClassifier = classifiers.First(x => x.Error == minError);

            var tree = bestClassifier.Classifier as DecisionTree;
            if (tree != null)
            {
                ComputeConfusionMatrix(singleClassDataModel, tree, bestClassifier.SplittingMethod);
            }

            var knn = bestClassifier.Classifier as KNearestNeighbors;
            if (knn != null)
            {
                ComputeConfusionMatrix(singleClassDataModel, knn, bestClassifier.SplittingMethod);
            }

            var bayes = bestClassifier.Classifier as NaiveBayes<NormalDistribution>;
            if (bayes != null)
            {
                ComputeConfusionMatrix(singleClassDataModel, bayes, bestClassifier.SplittingMethod);
            }

            var ann = bestClassifier.Classifier as ActivationNetwork;
            if (ann != null)
            {
                ComputeConfusionMatrix(singleClassDataModel, ann, bestClassifier.SplittingMethod);
            }

            using (var db = new LiteDatabase(@".\Resources\MlAsData.db"))
            {
                var classifierModels = db.GetCollection<ClassifierModel>("ClassifierModel").Find(x => x.BatchId == bestClassifier.BatchId);
                logger.Info($"\n Best Classifier Details:\n {Newtonsoft.Json.JsonConvert.SerializeObject(classifierModels.OrderBy(x => x.TrainingError).First())}");
            }
        }

        protected void ComputeConfusionMatrix(SingleClassDataModel singleClassDataModel, DecisionTree tree, string splittingMethod)
        {
            try
            {
                var compute = tree.ToExpression().Compile();

                var inputs = singleClassDataModel.Inputs;
                var actual = new int[inputs.Count()];
                for (var i = 0; i < inputs.Count(); i++)
                    actual[i] = compute(inputs[i]);
                var expected = singleClassDataModel.Outputs;

                var cms = new List<ConfusionMatrix>();
                for (var i = 0; i < singleClassDataModel.Classes.Count; i++)
                    cms.Add(new ConfusionMatrix(actual, expected, i));

                logger.Info($"\n{StringHelper.PrintConfusionMatrixString(cms, singleClassDataModel.Classes, singleClassDataModel.Name, "DecisionTree", splittingMethod)}");
            }
            catch (Exception ex)
            {
                logger.Warn($"Unable to load or analyse trained DecisionTree for this data: {ex.Message}");
            }
        }

        protected void ComputeConfusionMatrix(SingleClassDataModel singleClassDataModel, NaiveBayes<NormalDistribution> bayes, string splittingMethod)
        {
            try
            {
                var inputs = singleClassDataModel.Inputs;
                var actual = new int[inputs.Count()];
                for (var i = 0; i < inputs.Count(); i++)
                    actual[i] = bayes.Decide(inputs[i]);
                var expected = singleClassDataModel.Outputs;

                var cms = new List<ConfusionMatrix>();
                for (var i = 0; i < singleClassDataModel.Classes.Count; i++)
                    cms.Add(new ConfusionMatrix(actual, expected, i));

                logger.Info($"\n{StringHelper.PrintConfusionMatrixString(cms, singleClassDataModel.Classes, singleClassDataModel.Name, "NaiveBayes", splittingMethod)}");
            }
            catch (Exception ex)
            {
                logger.Warn($"Unable to load or analyse trained NaiveBayes for this data: {ex.Message}");
            }
        }

        protected void ComputeConfusionMatrix(SingleClassDataModel singleClassDataModel, KNearestNeighbors knn, string splittingMethod)
        {
            try
            {
                var inputs = singleClassDataModel.Inputs;
                var actual = new int[inputs.Count()];
                for (var i = 0; i < inputs.Count(); i++)
                    actual[i] = knn.Compute(inputs[i]);
                var expected = singleClassDataModel.Outputs;

                var cms = new List<ConfusionMatrix>();
                for (var i = 0; i < singleClassDataModel.Classes.Count; i++)
                    cms.Add(new ConfusionMatrix(actual, expected, i));

                logger.Info($"\n{StringHelper.PrintConfusionMatrixString(cms, singleClassDataModel.Classes, singleClassDataModel.Name, "KNearestNeighbors", splittingMethod)}");
            }
            catch (Exception ex)
            {
                logger.Warn($"Unable to load or analyse trained KNearestNeighbors for this data: {ex.Message}");
            }
        }

        protected void ComputeConfusionMatrix(SingleClassDataModel singleClassDataModel, ActivationNetwork ann, string splittingMethod)
        {
            try
            {
                var inputs = singleClassDataModel.Inputs;
                var actual = inputs.Apply(ann.Compute).GetColumn(0).Apply(System.Math.Sign);
                var expected = singleClassDataModel.Outputs;

                var cms = new List<ConfusionMatrix>();
                for (var i = 0; i < singleClassDataModel.Classes.Count; i++)
                    cms.Add(new ConfusionMatrix(actual, expected, i));

                logger.Info($"\n{StringHelper.PrintConfusionMatrixString(cms, singleClassDataModel.Classes, singleClassDataModel.Name, "NeuralNetwork", splittingMethod)}");
            }
            catch (Exception ex)
            {
                logger.Warn($"Unable to load or analyse trained NeuralNetwork for this data: {ex.Message}");
            }
        }
    }
}