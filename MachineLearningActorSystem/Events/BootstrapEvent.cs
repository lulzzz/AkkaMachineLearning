using MachineLearningActorSystem.Models;

namespace MachineLearningActorSystem.Events
{
    public class BootstrapEvent
    {
        public BootstrapEvent(Classifiers data)
        {
            Data = data;
        }

        public Classifiers Data { get; set; }
    }
}