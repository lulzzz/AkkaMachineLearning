using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using MachineLearningActorSystem.Core;
using sun.security.action;

namespace MachineLearningActorSystem.RaceTrack
{
    [Serializable]
    public class QAgent
    {
        public string AgentName { get; set; }
        public double LearningRate { get; set; }
        public ValueFunction ValueFunction { get; set; }
        public string RaceTime { get; set; }
        public double TotalReward { get; set; }
        public int TotalSteps { get; set; }
        public bool FailedRace { get; set; }
        public List<QState> Episode { get; set; }
        // Tuple(ax, ay, w1, w2, w3, w4, w5)
        public readonly List<Tuple<int, int, double, double, double, double, double>> ActionsAndWeights 
            = new List<Tuple<int, int, double, double, double, double, double>>
            {
                //                                                          ax ay     x    y    vx   vy
                new Tuple<int, int, double, double, double, double, double>( 0, 0, 1, 0.1, 0.1, 0.9, 0.9),
                new Tuple<int, int, double, double, double, double, double>(-1, 0, 1, 0.4, 0.3, 0.5, 0.9),
                new Tuple<int, int, double, double, double, double, double>( 0, 1, 1, 0.3, 0.3, 0.5, 0.9),
                new Tuple<int, int, double, double, double, double, double>( 0,-1, 1, 0.3, 0.3, 0.9, 0.5),
                new Tuple<int, int, double, double, double, double, double>( 1, 0, 1, 0.3, 0.4, 0.9, 0.5),
                new Tuple<int, int, double, double, double, double, double>(-1,-1, 1, 0.4, 0.4, 0.1, 0.1),
                new Tuple<int, int, double, double, double, double, double>(-1, 1, 1, 0.3, 0.4, 0.1, 0.3),
                new Tuple<int, int, double, double, double, double, double>( 1,-1, 1, 0.4, 0.3, 0.2, 0.2),
                new Tuple<int, int, double, double, double, double, double>( 1, 1, 1, 0.3, 0.3, 0.3, 0.1)
            };

        public QAgent()
        {
            Episode = new List<QState>();
        }

        public QAgent(double learningRate, ValueFunction valueFunction)
        {
            LearningRate = learningRate;
            ValueFunction = valueFunction;
            Episode = new List<QState>();
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"AgentName: {AgentName}");
            sb.AppendLine($"RaceTime: {RaceTime}");
            sb.AppendLine($"TotalReward: {TotalReward}");
            sb.AppendLine($"TotalSteps: {TotalSteps}");
            sb.AppendLine("Last steps in Episode:");
            foreach (var state in Episode)
            {
                sb.AppendLine($" {state}");
            }

            return sb.ToString();
        }

        public void Race()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            Episode = new List<QState>()
            {
                RaceTrackSimulator.StartEpisode()
            };

            AgentName = $"Agent.X{Episode.Single().X}.Y{Episode.Single().Y}";

            while (MoveAgent());

            stopwatch.Stop();
            RaceTime = stopwatch.Elapsed.ToString();
            TotalSteps = Episode.Count;
            TotalReward = Episode.Sum(x => x.Reward);
            Episode.RemoveRange(0, Episode.Count-2);
        }

        public bool MoveAgent()
        {
            try
            {
                switch (ValueFunction)
                {
                    case ValueFunction.One:
                        var newState = Episode.Last().NextQState(ValueFunctionOne());
                        Episode.Add(newState);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch (Exception ex)
            {
                FailedRace = true;
                Console.WriteLine($"Stopping Race due to Exception: {ex}");
            }
            
            return !FailedRace
                && Episode.Last().StateType == 0
                && Episode.Count < Config.ExplorerMaxIterations;
        }

        public Tuple<double, double> ValueFunctionOne()
        {
            var actionDiffs = new List<KeyValuePair<int, double>>();
            for (int i = 0; i < ActionsAndWeights.Count; i++)
            {
                actionDiffs.Add(ActionFunctionOne(i));                
            }
            var maxDiff = actionDiffs.Max(x => x.Value);
            var maxActions = actionDiffs.Where(x => x.Value == maxDiff).ToList();
            var random = new Random();
            var actionIndex = random.Next(0, maxActions.Count);
            return new Tuple<double, double>(ActionsAndWeights[actionIndex].Item1, ActionsAndWeights[actionIndex].Item2);
        }

        public KeyValuePair<int, double> ActionFunctionOne(int index)
        {
            var a = ActionsAndWeights[index];
            var curr = Episode.Last();
            var prev = curr.PrevQState;
            double resultCalc = 0;
            if (prev != null)
            {
                var currCalc = a.Item3 + a.Item4 * curr.X + a.Item5 * curr.Y + a.Item6 * curr.VX + a.Item7 * curr.VY;
                var prevCalc = a.Item3 + a.Item4 * prev.X + a.Item5 * prev.Y + a.Item6 * prev.VX + a.Item7 * prev.VY;
                resultCalc = curr.Reward + LearningRate * (currCalc - prevCalc);
            }
            return new KeyValuePair<int, double>(index, Math.Abs(resultCalc));
        }
    }
}