using MachineLearningActorSystem.Models;

namespace MachineLearningActorSystem.Events
{
    public class CrossValidationEvent
    {
        public CrossValidationEvent(Classifiers data)
        {
            Data = data;
        }

        public Classifiers Data { get; set; }
    }
}