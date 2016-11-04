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

            sb.Append($"{IterationId} TimeToLearn: {TimeToLearn} ValueDelta: {ValueDelta} PolicyDelta: {PolicyDelta}");

            return sb.ToString();
        }
    }
}