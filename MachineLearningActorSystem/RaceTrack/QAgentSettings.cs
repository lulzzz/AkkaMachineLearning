using System;
using System.Text;

namespace MachineLearningActorSystem.RaceTrack
{
    [Serializable]
    public class QAgentSettings
    {
        public QAgentSettings()
        {
        }

        public QAgentSettings(double learningRate, double explorationRate, double discountRate, int runs)
        {
            LearningRate = learningRate;
            ExplorationRate = explorationRate;
            DiscountRate = discountRate;
            Runs = runs;
        }

        public double LearningRate { get; set; }
        public double ExplorationRate { get; set; }
        public double DiscountRate { get; set; }
        public int Runs { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append($"LR:{LearningRate} ER:{ExplorationRate} DR:{DiscountRate} Runs:{Runs}");
            return sb.ToString();
        }
    }
}