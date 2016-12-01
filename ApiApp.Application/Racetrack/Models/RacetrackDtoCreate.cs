using System;
using System.Collections.Generic;

namespace ApiApp.Application.Racetrack.Models
{
    public class RacetrackDtoCreate : RacetrackDtoUpdate
    {
        public string Name { get; set; }
        public TimeSpan ExecutionTime { get; set; }
        public DateTime CreatedDate { get; set; }
        public List<RunResultsDto> RunResults { get; set; }
        public string RacetrackType { get; set; } // Qlearning ValueIteration
    }
}
