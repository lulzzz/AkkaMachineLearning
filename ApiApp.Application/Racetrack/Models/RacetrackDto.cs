using System;

namespace ApiApp.Application.Racetrack.Models
{
    public class RacetrackDto : RacetrackDtoCreate
    {
        public int TotalRuns { get; set; }
        public double MeanReward { get; set; }
        public double MeanSteps { get; set; }
        public  TimeSpan MeanExecutionTime { get; set; }
        public double SuccessRate { get; set; }
    }
}
