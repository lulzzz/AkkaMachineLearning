using System;
using System.Text;

namespace MachineLearningActorSystem.RaceTrack
{
    [Serializable]
    public class QAgentRun
    {
        public string RunTime { get; set; }
        public double TotalReward { get; set; }
        public int TotalSteps { get; set; }
        public int OffTrackSteps { get; set; }
        public int OnTrackSteps { get; set; }
        public bool FailedRace { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(
                $"Time:{RunTime} Reward:{TotalReward} Steps:{TotalSteps}  OffTrack:{OffTrackSteps}  OnTrack:{OnTrackSteps}");
            return sb.ToString();
        }
    }
}