using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MachineLearningActorSystem.Core;
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
                logger.Info($"{raceTrack.GetType().Name} ValueIteration Completed");
            });
        }

        private RaceTrack.RaceTrack StartValueIteration(RaceTrack.RaceTrack raceTrack)
        {
            if ((raceTrack.StatesAfterLearning == null) || !raceTrack.StatesAfterLearning.Any())
            {
                logger.Info("RaceTrack ValueIteration started");

                var statesToIterate = new List<State>(raceTrack.InitialStates);
                var allIterationsStopwatch = new Stopwatch();
                allIterationsStopwatch.Start();

                var iterations = 0;
                do
                {
                    var iterationStopwatch = new Stopwatch();
                    iterationStopwatch.Start();

                    var delta = Iterate(ref statesToIterate);
                    var valueDelta = delta.Item1;
                    var policyDelta = delta.Item2;
                    var iterationTime = iterationStopwatch.Elapsed;

                    raceTrack.Iterations.Add(new Iteration(iterations, iterationTime.ToString(), valueDelta, policyDelta));
                    Console.WriteLine("Iteration: {2} ValueDelta: {0} PolicyDelta: {1}\n", valueDelta, policyDelta,
                        iterations);

                    if ((valueDelta < 1e-9) || (policyDelta == 0)) break;

                    iterations++;
                } while (true);

                raceTrack.NumberOfIterations = raceTrack.Iterations.Count;
                raceTrack.TimeToLearn = allIterationsStopwatch.Elapsed.ToString();
                raceTrack.StatesAfterLearning = statesToIterate;
                raceTrack.NumberOfStatesAfterLearning = statesToIterate.Count;
                raceTrack.NumberOfActionsAfterLearning = statesToIterate.Sum(s => s.Policy?.Count ?? 0);

                logger.Info("RaceTrack ValueIteration completed");

                XmlHelper.SaveRaceTrack(raceTrack);
            }

            return raceTrack;
        }

        private RaceTrack.RaceTrack StartAgents(RaceTrack.RaceTrack raceTrack)
        {
            if ((raceTrack.StatesAfterLearning != null) && raceTrack.StatesAfterLearning.Any())
            {
                logger.Info("Agent Race started");

                var startStates = raceTrack.StatesAfterLearning.Where(x => x.Type == StateType.Start).ToList();

                if (startStates.Any())
                {
                    var raceNumber = 1;
                    if ((raceTrack.Races != null) && (raceTrack.Races.Count > 0))
                        raceNumber = raceTrack.Races.Count + 1;
                    else
                        raceTrack.Races = new List<Race>();

                    var agents = startStates.Select(startState => new Agent(startState)).ToList();
                    var race = new Race(raceNumber, DateTime.Now.ToString(), agents);
                    race.Start(raceTrack.StatesAfterLearning);
                    raceTrack.Races.Add(race);

                    logger.Info("Agent Race completed");

                    XmlHelper.SaveRaceTrack(raceTrack);
                }
            }
            return raceTrack;
        }

        public Tuple<double, int> Iterate(ref List<State> states)
        {
            var totalActions = states.Sum(x => x.Policy.Count);
            var numberOfActionsTaken = 0;
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            logger.Info("Calculating State Values");

            foreach (var state in states)
            {
                numberOfActionsTaken += state.CalculateValue(ref states);

                Console.Write(
                    "\r({3},{4},{5},{6}) Val:{7} NVal:{8} TimeElapsed:{9} {2}% Actions:{0}\\{1}",
                    numberOfActionsTaken.ToString().PadLeft(7),
                    totalActions,
                    ((int) (numberOfActionsTaken/(double) totalActions*100)).ToString().PadLeft(3),
                    state.X.ToString().PadLeft(3),
                    state.Y.ToString().PadLeft(3),
                    state.VX.ToString().PadLeft(3),
                    state.VY.ToString().PadLeft(3),
                    state.Value.ToString("0.00").PadLeft(6),
                    (state.NewValue.HasValue ? state.NewValue.Value.ToString("0.00") : string.Empty).PadLeft(6),
                    stopWatch.Elapsed.ToString(@"mm\:ss"));
            }
            Console.WriteLine("");

            logger.Info("Calculating State Values Completed");

            logger.Info("Setting New State Values");

            var valueDelta = states.Sum(state => state.UpdateValue());

            logger.Info("Setting New State Values completed");

            logger.Info("Calculating New Policy");

            var policyDelta = states.Sum(state => state.UpdatePolicy());

            logger.Info("Calculating New Policy completed");

            stopWatch.Stop();

            return new Tuple<double, int>(valueDelta, policyDelta);
        }
    }
}