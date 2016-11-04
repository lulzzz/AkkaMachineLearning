using System;
using MachineLearningActorSystem.Core;

namespace MachineLearningActorSystem.Events
{
    public class ShutdownEvent
    {
        public ShutdownEvent()
        {
            _shutdownStartedDateTime = DateTime.Now;
        }

        private DateTime _shutdownStartedDateTime { get; }

        public int GetTimeoutSeconds()
        {
            var totalTimeoutSeconds = Config.CoreShutdownTimeoutSeconds;
            var startedSecondsAgo = (DateTime.Now - _shutdownStartedDateTime).Seconds;
            return totalTimeoutSeconds - startedSecondsAgo - 1;
        }
    }
}