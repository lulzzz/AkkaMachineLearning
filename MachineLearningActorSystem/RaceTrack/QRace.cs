using System;
using System.Collections.Generic;
using System.Diagnostics;
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

            sb.AppendLine($"RaceNumber: {RaceNumber}");
            sb.AppendLine($"RaceDateTime: {RaceDateTime}");
            foreach (var agent in Agents)
            {
                sb.AppendLine(agent.ToString());
            }

            return sb.ToString();
        }

        public void Start()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            foreach (var agent in Agents)
            {
                agent.Race();
            }
            stopwatch.Stop();
            TotalRaceTime = stopwatch.Elapsed.ToString();
        }
    }
}