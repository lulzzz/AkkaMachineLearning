namespace MachineLearningActorSystem.Events
{
    public class RaceTrackQLearningEvent
    {
        public RaceTrackQLearningEvent(bool printLastRun)
        {
            PrintLastRun = printLastRun;
        }

        public bool PrintLastRun { get; set; }
    }
}