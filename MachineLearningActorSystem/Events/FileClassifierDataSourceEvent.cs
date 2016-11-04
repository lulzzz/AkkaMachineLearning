namespace MachineLearningActorSystem.Events
{
    public class FileClassifierDataSourceEvent
    {
        public enum ClassifierMethod
        {
            DECISIONTREECV,
            DECISIONTREEBS,
            NAIVEBAYESCV,
            NAIVEBAYESBS,
            KNNCV,
            KNNBS,
            NEUROCV,
            NEUROBS,
            FINDBEST
        }

        public FileClassifierDataSourceEvent(string dataSourceFilePath, string classifier)
        {
            DataSourceFilePath = dataSourceFilePath;
            switch (classifier)
            {
                case "cdc":
                    Classifier = ClassifierMethod.DECISIONTREECV;
                    break;
                case "cdb":
                    Classifier = ClassifierMethod.DECISIONTREEBS;
                    break;
                case "ckc":
                    Classifier = ClassifierMethod.KNNCV;
                    break;
                case "ckb":
                    Classifier = ClassifierMethod.KNNBS;
                    break;
                case "nbc":
                    Classifier = ClassifierMethod.NAIVEBAYESCV;
                    break;
                case "nbb":
                    Classifier = ClassifierMethod.NAIVEBAYESBS;
                    break;
                case "cfb":
                    Classifier = ClassifierMethod.FINDBEST;
                    break;
            }
        }

        public string DataSourceFilePath { get; set; }
        public ClassifierMethod Classifier { get; set; }
    }
}