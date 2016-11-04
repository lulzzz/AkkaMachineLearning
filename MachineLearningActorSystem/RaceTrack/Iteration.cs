using System;
using System.Text;

namespace MachineLearningActorSystem.RaceTrack
{
    [Serializable]
    public class Iteration
    {
        public Iteration()
        {
            
        }

        public Iteration(int iterationId, string timeToLearn, double valueDelta, int policyDelta)
        {
            IterationId = iterationId;
            TimeToLearn = timeToLearn;
            ValueDelta = valueDelta;
            PolicyDelta = policyDelta;
        }
        public int IterationId { get; set; }
        public string TimeToLearn { get; set; }
        public double ValueDelta { get; set; }
        public int PolicyDelta { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLine($"IterationId: {IterationId}");
            sb.AppendLine($"TimeToLearn: {TimeToLearn}");
            sb.AppendLine($"ValueDelta: {ValueDelta}");
            sb.AppendLine($"PolicyDelta: {PolicyDelta}");

            return sb.ToString();
        }
    }
}