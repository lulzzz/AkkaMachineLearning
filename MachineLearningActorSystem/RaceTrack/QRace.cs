using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace MachineLearningActorSystem.RaceTrack
{
    [Serializable]
    public class QRace
    {
        public QRace()
        {
            Agents = new List<QAgent>();
        }

        public QRace(int raceNumber, string raceDateTime, List<QAgent> agents)
        {
            RaceNumber = raceNumber;
            RaceDateTime = raceDateTime;
            Agents = agents;
        }

        public int RaceNumber { get; set; }
        public string RaceDateTime { get; set; }
        public string TotalRaceTime { get; set; }
        public List<QAgent> Agents { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("");
            sb.AppendLine("*************************************************************************************");
            sb.AppendLine($"RaceNumber: {RaceNumber} RaceDateTime: {RaceDateTime} TotalRaceTime: {TotalRaceTime}");
            sb.AppendLine("*************************************************************************************");
            sb.AppendLine($"  Top 3 Agents (Max Reward in 1 of 3 iterations):");
            foreach (var agent in Agents.OrderByDescending(a => a.MaxRunReward).Take(3))
                sb.AppendLine(agent.ToString());
            sb.AppendLine("*************************************************************************************");
            sb.AppendLine("");
            return sb.ToString();
        }

        public void Start()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            foreach (var agent in Agents)
                agent.Race();
            stopwatch.Stop();
            TotalRaceTime = stopwatch.Elapsed.ToString();
        }
    }
}