namespace MachineLearningActorSystem.Events
{
    public class ClassifierResultEvent
    {
        public ClassifierResultEvent(object classifier, double error, string splittingMethod, string batchId)
        {
            Error = error;
            Classifier = classifier;
            SplittingMethod = splittingMethod;
            BatchId = batchId;
        }

        public object Classifier { get; set; }
        public double Error { get; set; }
        public string SplittingMethod { get; set; }
        public string BatchId { get; set; }
    }
}