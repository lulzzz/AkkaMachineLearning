using System;
using NanoApi.JsonFile;

namespace ApiApp.Data.Racetrack.Models
{
    public class RunResultsDao
    {
        [PrimaryKey]
        public int Id { get; set; }
        public double LearningRate { get; set; }
        public double ExplorationRate { get; set; }
        public double DiscountRate { get; set; }
        public TimeSpan ExecutionTime { get; set; }
        public DateTime CreatedDate { get; set; }
        public double Reward { get; set; }
        public int Steps { get; set; }
        public double SuccessRate { get; set; }
    }
}
