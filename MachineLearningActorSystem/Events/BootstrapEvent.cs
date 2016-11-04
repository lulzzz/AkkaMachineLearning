namespace MachineLearningActorSystem.Events
{
    using Models;

    public class BootstrapEvent
    {
        public BootstrapEvent(SingleClassDataModel data)
        {
            Data = data;
        }

        public SingleClassDataModel Data { get; set; }
    }
}