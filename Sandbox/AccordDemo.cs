namespace Sandbox
{
    using AbstractClassifier = weka.classifiers.AbstractClassifier;
    using Evaluation = weka.classifiers.Evaluation;
    using Instances = weka.core.Instances;
    using OptionHandler = weka.core.OptionHandler;
    using Utils = weka.core.Utils;
    using Filter = weka.filters.Filter;
    using FileReader = java.io.FileReader;
    using BufferedReader = java.io.BufferedReader;
    using StringBuffer = java.lang.StringBuffer;
    using System;
    using Accord;
    using Accord.IO;
    using Accord.MachineLearning.DecisionTrees;
    using Accord.MachineLearning.DecisionTrees.Learning;
    using Accord.Math;
    using Accord.Statistics.Analysis;
    using AForge;
    using System.Data;
    using System.Collections.Generic;
    using Accord.Statistics.Filters;

    public class AccordDemo : IDemo
    {
        // The classifier used internally
        private AbstractClassifier m_Classifier { get; set; }

        // The filter to use
        private Filter m_Filter { get; set; }

        // The training file
        private string m_TrainingFile = null;

        // The training instances
        private Instances m_Training = null;

        // For evaluating the classifier
        private Evaluation m_Evaluation = null;

        public void SetClassifier(string name, string[] options)
        {
            
            m_Classifier = (AbstractClassifier)AbstractClassifier.forName(name, options);
        }

        public void SetFilter(string name, string[] options)
        {
            m_Filter = (Filter)java.lang.Class.forName(name).newInstance();
            if (m_Filter is OptionHandler)
            {
                ((OptionHandler)m_Filter).setOptions(options);
            }
        }

        // Sets the file to use for training
        public void SetTraining(string name)
        {
            m_TrainingFile = name;
            m_Training = new Instances( new BufferedReader(new FileReader(m_TrainingFile)));
            double[,] table = new double[m_Training.numAttributes(), m_Training.numInstances()];

            DataTable data = new DataTable(m_Training.relationName());
            List<DecisionVariable> attributes = new List<DecisionVariable>();
            List<string> attr = new List<string>();
            string outP = string.Empty;

            for (int i = 0; i< m_Training.numAttributes(); i++)
            {
                string item = m_Training.attribute(i).name();
                data.Columns.Add(item);
                attributes.Add(new DecisionVariable(item, m_Training.numDistinctValues(i)));
                attr.Add(item);
            }
            outP = attr[attr.Count - 1]; 
            attr.RemoveAt(attr.Count-1);
            attributes.RemoveAt(attributes.Count - 1);

            var arrayData = m_Training.toArray();

            for(int i = 0; i < m_Training.numInstances(); i++)
            {
                data.Rows.Add(arrayData[i].ToString().Split(','));
            }

            Codification codebook = new Codification(data);

            int classCount = m_Training.numDistinctValues(m_Training.numAttributes() - 1);

            DecisionTree tree = new DecisionTree(attributes, classCount);

            // Create a new instance of the ID3 algorithm
            C45Learning c45Learning = new C45Learning(tree);

            // Translate our training data into integer symbols using our codebook:
            DataTable symbols = codebook.Apply(data);
            double[][] inputs = symbols.ToArray(attr.ToArray());
            int[] outputs = symbols.ToIntArray(outP).GetColumn(0);

            // Learn the training instances!
            c45Learning.Run(inputs, outputs);

            // Convert to an expression tree
            var expression = tree.ToExpression();

            // Compiles the expression to IL
            var func = expression.Compile();
        }

        // Runs 10fold CV over the training file
        public void Execute()
        {
            // run filter
            m_Filter.setInputFormat(m_Training);
            Instances filtered = Filter.useFilter(m_Training, m_Filter);

            // train classifier on complete file for tree
            m_Classifier.buildClassifier(filtered);

            // 10fold CV with seed=1
            m_Evaluation = new Evaluation(filtered);
            m_Evaluation.crossValidateModel(
            m_Classifier, filtered, 10, m_Training.getRandomNumberGenerator(1));
        }

        // Outputs some data about the classifier
        public override string ToString()
        {
            StringBuffer result;

            result = new StringBuffer();
            result.append("Weka - Demo\n===========\n\n");

            result.append("Classifier...: "
                + m_Classifier.getClass().getName() + " "
                + Utils.joinOptions(m_Classifier.getOptions()) + "\n");

            if (m_Filter is OptionHandler)
            {
                result.append("Filter.......: "
                + m_Filter.getClass().getName() + " "
                + Utils.joinOptions(((OptionHandler)m_Filter).getOptions()) + "\n");
            }
            else
            {
                result.append("Filter.......: "
                + m_Filter.getClass().getName() + "\n");
            }

            result.append("Training file: "
            + m_TrainingFile + "\n");

            result.append("\n");

            result.append(m_Classifier.toString() + "\n");
            result.append(m_Evaluation.toSummaryString() + "\n");
            try
            {
                result.append(m_Evaluation.toMatrixString() + "\n");
            }
            catch (Exception e)
            {
                Console.Write(e.Message, e.ToString());
            }
            try
            {
                result.append(m_Evaluation.toClassDetailsString() + "\n");
            }
            catch (Exception e)
            {
                Console.Write(e.Message, e.ToString());
            }

            return result.toString();
        }

        public string Usage()
        {
            return
                "Usage:\n"
                + "  Sandbox.exe\n"
                + "  CLASSIFIER <classname> [options] \n"
                + "  FILTER <classname> [options]\n"
                + "  DATASET <trainingfile>\n\n"
                + "e.g., \n"
                + "  Sandbox.exe \n"
                + "    CLASSIFIER weka.classifiers.trees.J48 -U \n"
                + "    FILTER weka.filters.unsupervised.instance.Randomize \n"
                + "    DATASET iris.arff\n";
        }
    }
}
