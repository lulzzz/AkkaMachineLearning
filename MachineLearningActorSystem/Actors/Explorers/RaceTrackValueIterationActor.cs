using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MachineLearningActorSystem.RaceTrack;

namespace MachineLearningActorSystem.Actors.Explorers
{
    public class RaceTrackValueIterationActor : BaseActor
    {
        public RaceTrackValueIterationActor()
        {
            // TODO: LearningEvent
            // TODO: TestingEvent

            Receive<RaceTrack.RaceTrack>(raceTrack =>
            {
                logger.Info($"{raceTrack.GetType().Name} Received");
                raceTrack = StartValueIteration(raceTrack);
                raceTrack = StartAgents(raceTrack);
                Console.Write(raceTrack);
            });
        }

        private RaceTrack.RaceTrack StartValueIteration(RaceTrack.RaceTrack raceTrack)
        {
            if (raceTrack.StatesAfterLearning == null || !raceTrack.StatesAfterLearning.Any())
            {
                Console.WriteLine("RaceTrack ValueIteration started");

                var statesToIterate = new List<State>(raceTrack.InitialStates);
                Stopwatch allIterationsStopwatch = new Stopwatch();
                allIterationsStopwatch.Start();

                int iterations = 0;
                do
                {
                    Stopwatch iterationStopwatch = new Stopwatch();
                    iterationStopwatch.Start();

                    var delta = Iterate(ref statesToIterate);
                    var valueDelta = delta.Item1;
                    var policyDelta = delta.Item2;
                    var iterationTime = iterationStopwatch.Elapsed;

                    raceTrack.Iterations.Add(new Iteration(iterations, iterationTime.ToString(), valueDelta, policyDelta));
                    Console.WriteLine("Iteration: {2} ValueDelta: {0} PolicyDelta: {1}\n", valueDelta, policyDelta,
                        iterations);

                    if (valueDelta < 1e-9 || policyDelta == 0) break;

                    iterations++;

                } while (true);

                raceTrack.NumberOfIterations = raceTrack.Iterations.Count;
                raceTrack.TimeToLearn = allIterationsStopwatch.Elapsed.ToString();
                raceTrack.StatesAfterLearning = statesToIterate;
                raceTrack.NumberOfStatesAfterLearning = statesToIterate.Count;
                raceTrack.NumberOfActionsAfterLearning = statesToIterate.Sum(s => s.Policy?.Count ?? 0);

                Console.WriteLine("RaceTrack ValueIteration completed");

                RaceTrackHelper.SaveRaceTrack(raceTrack);
            }

            return raceTrack;
        }

        private RaceTrack.RaceTrack StartAgents(RaceTrack.RaceTrack raceTrack)
        {
            if (raceTrack.StatesAfterLearning != null && raceTrack.StatesAfterLearning.Any())
            {
                Console.WriteLine("Agent Race started");

                var startStates = raceTrack.StatesAfterLearning.Where(x => x.Type == StateType.Start).ToList();

                if (startStates.Any())
                {
                    int raceNumber = 1;
                    if (raceTrack.Races != null && raceTrack.Races.Count > 0)
                    {
                        raceNumber = raceTrack.Races.Count + 1;
                    }
                    else
                    {
                        raceTrack.Races = new List<Race>();
                    }

                    var agents = startStates.Select(startState => new Agent(startState)).ToList();
                    var race = new Race(raceNumber, DateTime.Now.ToString(), agents);
                    race.Start(raceTrack.StatesAfterLearning);
                    raceTrack.Races.Add(race);

                    Console.WriteLine("Agent Race completed");

                    RaceTrackHelper.SaveRaceTrack(raceTrack);
                }
            }
            return raceTrack;
        }

        public Tuple<double, int> Iterate(ref List<State> states)
        {
            int totalActions = states.Sum(x => x.Policy.Count);
            int numberOfActionsTaken = 0;
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            Console.WriteLine("Calculating State Values");

            foreach (var state in states)
            {
                numberOfActionsTaken += state.CalculateValue(ref states);

                if (true)
                {
                    Console.Write(
                            "\rX: {3} Y: {4} vX: {5} vY: {6} Value: {7} NewValue: {8} Delta: {9} TimeElapsed: {10}\nPercent: {2} ActionsTaken: {0} Total: {1}",
                            numberOfActionsTaken.ToString().PadLeft(7),
                            totalActions.ToString().PadLeft(7),
                            (numberOfActionsTaken / (double)totalActions).ToString("0.00").PadLeft(6),
                            state.X.ToString().PadLeft(3),
                            state.Y.ToString().PadLeft(3),
                            state.VX.ToString().PadLeft(3),
                            state.VY.ToString().PadLeft(3),
                            state.Value.ToString("0.00").PadLeft(6),
                            (state.NewValue.HasValue ? state.NewValue.Value.ToString("0.00") : string.Empty).PadLeft(6),
                            (state.NewValue.HasValue ? (state.Value - state.NewValue.Value) : 0).ToString("0.00")
                                .PadLeft(6),
                            stopWatch.Elapsed);
                }
            }

            Console.WriteLine("Calculating State Values Completed");

            Console.WriteLine("Setting New State Values");

            double valueDelta = states.Sum(state => state.UpdateValue());

            Console.WriteLine("Setting New State Values completed");

            Console.WriteLine("Calculating New Policy");

            int policyDelta = states.Sum(state => state.UpdatePolicy());

            Console.WriteLine("Calculating New Policy completed");

            stopWatch.Stop();

            return new Tuple<double, int>(valueDelta, policyDelta);
        }
    }
}