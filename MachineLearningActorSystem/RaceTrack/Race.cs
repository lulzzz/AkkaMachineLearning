using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace MachineLearningActorSystem.RaceTrack
{
    [Serializable]
    public class Race
    {
        public Race()
        {
            Agents = new List<Agent>();
        }

        public Race(int raceNumber, string raceDateTime, List<Agent> agents)
        {
            RaceNumber = raceNumber;
            RaceDateTime = raceDateTime;
            Agents = agents;
        }

        public int RaceNumber { get; set; }
        public string RaceDateTime { get; set; }
        public string TotalRaceTime { get; set; }
        public List<Agent> Agents { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("");
            sb.AppendLine("*************************************************************************************");
            sb.AppendLine($"RaceNumber: {RaceNumber} RaceDateTime: {RaceDateTime} TotalRaceTime: {TotalRaceTime}");
            sb.AppendLine("*************************************************************************************");
            sb.AppendLine($"  Agents for each start state (Ordered by Max Reward):");
            foreach (var agent in Agents.OrderByDescending(a => a.TotalReward))
                sb.AppendLine(agent.ToString());
            sb.AppendLine("*************************************************************************************");
            return sb.ToString();
        }

        public void Start(List<State> states)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            foreach (var agent in Agents)
                agent.Race(ref states);
            stopwatch.Stop();
            TotalRaceTime = stopwatch.Elapsed.ToString();
        }
    }
}