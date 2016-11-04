namespace MachineLearningActorSystem.Events
{
    public class ExplorerResultEvent
    {

        public ExplorerResultEvent(string batchId)
        {
            BatchId = batchId;
        }

        public string BatchId { get; set; }

    }
}
