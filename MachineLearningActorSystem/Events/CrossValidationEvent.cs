namespace MachineLearningActorSystem.Events
{
    using Models;

    public class CrossValidationEvent
    {
        public CrossValidationEvent(SingleClassDataModel data)
        {
            Data = data;
        }

        public SingleClassDataModel Data { get; set; }
    }
}
