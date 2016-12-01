using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using ApiApp.Data.Racetrack;
using ApiApp.Data.Racetrack.Models;

namespace Racetrack
{
    public class QLearningGradientDescent
    {
        private double[][] ActionVectors;
        private double[][] WeightVectors;
        private readonly double _learningRate = 0.5;
        private readonly double _explorationRate = -1; // Not used atm, better results without it
        private readonly double _discounRate = -1; // Not used atm, better results without it
        private readonly IRacetrackRepository _racetrackRepository = new RacetrackRepository();

        public QLearningGradientDescent()
        {
            ActionVectors = GetActionVectorsForRange(new[] {-1, 0, 1}, new[] {-1, 0, 1});
            WeightVectors = GetRandomWeightVectors(9, 8, true);
        }

        public void Start(int numberOfRuns, int numberOfIterationsInRun, int maxSteps)
        {
            var racetrackResult = new RacetrackDaoCreate
            {
                CreatedDate = DateTime.Now,
                Name = Guid.NewGuid().ToString(),
                RacetrackType = "Q",
                RunResults = new List<RunResultsDao>()
            };

            var racetrackStopwatch = new Stopwatch();
            racetrackStopwatch.Start();
            var sim = new RaceTrackSim();
            var sb = new StringBuilder();

            foreach (var run in Enumerable.Range(0, numberOfRuns))
            {
                var resultStopwatch = new Stopwatch();
                resultStopwatch.Start();

                double totalIterationSuccess = 0;
                double totalIterationReward = 0;
                double totalIterationSteps = 0;

                foreach (var iteration in Enumerable.Range(0, numberOfIterationsInRun))
                {
                    WeightVectors = GetRandomWeightVectors(9, 8, true);
                    var simState = sim.startEpisode();
                    var state = GetState(simState[0], simState[1], simState[2], simState[3], 0, 0, simState[4],simState[5]);
                    int lastIndex = ActionVectors.ToList().FindIndex(a => a[0] == simState[4] && a[1] == simState[5]);

                    #region Iterate Through Map

                    foreach (var step in Enumerable.Range(0, maxSteps))
                    {
                        totalIterationSteps++;

                        var currQ = GetCurrentActionValue(state, WeightVectors[lastIndex]);
                        var actionValues = GetActionValues(state, WeightVectors, ActionVectors).ToList();
                        var actionIndex = actionValues.IndexOf(actionValues.Max());
                        lastIndex = actionIndex;
                        simState = sim.simulate(state[0], state[1], state[2], state[3], ActionVectors[actionIndex][0], ActionVectors[actionIndex][1]);
                        state = GetState(simState[0], simState[1], simState[2], simState[3], ActionVectors[actionIndex][0], ActionVectors[actionIndex][1], simState[4], simState[5]);
                        var stateValues = GetActionValues(state, WeightVectors, ActionVectors);
                        UpdateWeightVectors(state, stateValues, actionIndex, simState[simState.Length - 1] == 1, currQ);

                        totalIterationReward += state[state.Length - 2];

                        if (state[7] == 1)
                        {
                            totalIterationSuccess++;
                            break;
                        }
                    }

                    #endregion Iterate Through Map

                }

                resultStopwatch.Stop();

                // Average Results for run
                racetrackResult.RunResults.Add(new RunResultsDao
                {
                    CreatedDate = DateTime.Now,
                    DiscountRate = _discounRate,
                    ExecutionTime = resultStopwatch.Elapsed,
                    ExplorationRate = _explorationRate,
                    LearningRate = _learningRate,
                    Reward = totalIterationReward/numberOfIterationsInRun,
                    Steps = (int)totalIterationSteps / numberOfIterationsInRun,
                    SuccessRate = totalIterationSuccess / numberOfIterationsInRun
                });
            }

            racetrackResult.ExecutionTime = racetrackStopwatch.Elapsed;
            racetrackStopwatch.Stop();
            var racetrackRespone = _racetrackRepository.CreateRacetrack(racetrackResult);

            sb.AppendLine($"*******************************************************************************************************");
            sb.AppendLine($"Racetrack: MeanReward: {racetrackRespone.RunResults.Average(x => x.Reward)} MeanStepsTaken: {racetrackRespone.RunResults.Average(x => x.Steps)} MeanSuccessRate: {racetrackRespone.RunResults.Average(x => x.SuccessRate)}");
            sb.AppendLine($"Runs, ordered by Reward: ");
            foreach (var run in racetrackRespone.RunResults)
            {
                sb.AppendLine($" MeanReward: {run.Reward} MeanStepsTaken: {run.Steps} MeanSuccessRate: {run.SuccessRate.ToString()}");
            }

            Console.Write(sb);
        }

        private double[][] GetRandomWeightVectors(int nVectors, int nWeights, bool minusOneInit)
        {
            double[][] vectors = new double[nVectors][];
            for (int v = 0; v < nVectors; v++)
            {
                double[] weights = new double[nWeights];
                var maximum = 1.0;
                var minimum = -1.0;
                var random = new Random();
                for (int w = 1; w < nWeights; w++)
                {
                    var result = -1.0;
                    if (!minusOneInit)
                    {
                        do
                        {
                            result = random.NextDouble()*(maximum - minimum) + minimum;
                        } while (weights.Contains(result));
                    }
                    weights[w] = result;
                }
                weights[0] = 1;
                vectors[v] = weights;
            }

            return vectors;
        }

        private double[][] GetActionVectorsForRange(int[] xArr, int[] yArr)
        {
            int nActions = xArr.Length*yArr.Length;
            double[][] actions = new double[nActions][];
            int ai = 0;
            foreach (var x in xArr)
            {
                foreach (var y in yArr)
                {
                    actions[ai++] = new double[] {x, y};
                }
            }
            return actions;
        }

        private double[] GetState(double x, double y, double vx, double vy, double ax, double ay, double r, double t)
        {
            // 1 x y vx vy r t
            double[] state = {x, y, vx, vy, ax, ay, r , t};
            return state;
        }

        private double[] GetActionValues(IReadOnlyList<double> state, IReadOnlyList<double[]> weights, IReadOnlyList<double[]> actions)
        {
            var values = new double[actions.Count];
            for (int a = 0; a < actions.Count; a++)
            {
                var tempState = GetState(state[0], state[1], state[2], state[3], actions[a][0], actions[a][1], state[6], state[7]);
                var value = weights[a][0];
                for (int i = 1; i < state.Count; i++)
                {
                    value += tempState[i] * weights[a][i];
                }
                values[a] = value;
            }
            return values;
        }

        private double GetCurrentActionValue(IReadOnlyList<double> state, IReadOnlyList<double> weight)
        {
            var value = weight[0];
            for (int i = 1; i < state.Count; i++)
            {
                value += state[i] * weight[i];
            }
            return value;
        }

        private void UpdateWeightVectors(IReadOnlyList<double> state, IReadOnlyList<double> stateValues, int? aIndex, bool isTerminal, double currQ)
        {
            double r = state[state.Count-2];
            var maxI = stateValues.ToList().IndexOf(stateValues.Max());
            for (int a = aIndex ?? 0; a < (aIndex+1 ?? ActionVectors.Length); a++)
            {
                for (int w = 0; w < WeightVectors[a].Length-1; w++)
                {
                    var weight = WeightVectors[a][w];
                    var maxQ = isTerminal ? 0 : stateValues[maxI];
                    weight = (weight + _learningRate * (r + maxQ - currQ)) - currQ * state[w];
                    WeightVectors[a][w] = weight;
                }
            }
        }
    }
}
