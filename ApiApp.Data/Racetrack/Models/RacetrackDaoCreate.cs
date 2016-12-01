using System;
using System.Collections.Generic;

namespace ApiApp.Data.Racetrack.Models
{
    public class RacetrackDaoCreate : RacetrackDaoUpdate
    {
        public string Name { get; set; }
        public TimeSpan ExecutionTime { get; set; }
        public DateTime CreatedDate { get; set; }
        public List<RunResultsDao> RunResults { get; set; }
        public string RacetrackType { get; set; } // Qlearning ValueIteration
    }
}
