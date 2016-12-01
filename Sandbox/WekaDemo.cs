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

    public class WekaDemo : IDemo
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
            m_Training.setClassIndex(m_Training.numAttributes() - 1);
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
