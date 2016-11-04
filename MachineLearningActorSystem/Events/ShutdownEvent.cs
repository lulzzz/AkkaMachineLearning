namespace MachineLearningActorSystem.Events
{
    using Core;
    using System;

    public class ShutdownEvent
    {
        public ShutdownEvent()
        {
            _shutdownStartedDateTime = DateTime.Now;
        }

        public int GetTimeoutSeconds()
        {
            var totalTimeoutSeconds = Config.CoreShutdownTimeoutSeconds;
            var startedSecondsAgo = (DateTime.Now - _shutdownStartedDateTime).Seconds;
            return totalTimeoutSeconds - startedSecondsAgo - 1;
        }

        private DateTime _shutdownStartedDateTime { get; set; }
    }
}