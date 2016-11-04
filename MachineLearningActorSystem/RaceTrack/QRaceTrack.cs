using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MachineLearningActorSystem.RaceTrack
{
    [Serializable]
    public class QRaceTrack
    {
        public List<QRace> Races { get; set; }

        public QRaceTrack()
        {
            Races = new List<QRace>();
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (var race in Races)
            {
                sb.AppendLine(race.ToString());
            }

            return sb.ToString();
        }
    }
}
