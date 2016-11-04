using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using log4net;
using MachineLearningActorSystem.Core;

namespace MachineLearningActorSystem.RaceTrack
{
    [Serializable]
    public class QAgent
    {
        [NonSerialized] private readonly ILog Logger = LogManager.GetLogger(typeof(QAgent));

        public QAgent()
        {
            Episode = new List<QState>();
            Settings = new QAgentSettings();
            Runs = new List<QAgentRun>();
        }

        public QAgent(ValueFunction valueFunction, double learningRate, double explorationRate, double discountRate,
            int runs)
        {
            ValueFunction = valueFunction;
            Settings = new QAgentSettings(learningRate, explorationRate, discountRate, runs);
            Episode = new List<QState>();
            Runs = new List<QAgentRun>();
            AgentName = $"Agent.L({Settings.LearningRate}).E({Settings.ExplorationRate}).D({Settings.DiscountRate})";
        }

        public string AgentName { get; set; }
        public ValueFunction ValueFunction { get; set; }
        public string RaceTime { get; set; }

        [XmlIgnore]
        public List<QState> Episode { get; set; }

        public QAgentSettings Settings { get; set; }
        public List<QAgentRun> Runs { get; set; }
        public int MaxRunSteps { get; set; }
        public double MaxRunReward { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLine(
                $"   AgentName: {AgentName} RaceTime: {RaceTime} MaxRunReward: {MaxRunReward} MaxRunSteps: {MaxRunSteps}");
            sb.AppendLine($"   Settings: {Settings}");
            foreach (var run in Runs)
                sb.AppendLine($"    {run}");
            return sb.ToString();
        }

        public void Race()
        {
            Logger.Info("Starting Agent Race/Iteration");

            for (var i = 0; i < Settings.Runs; i++)
            {
                var run = new QAgentRun();
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                Episode.Add(RaceTrackSimulator.StartEpisode());

                try
                {
                    do
                    {
                        if (Episode.Last().Reward == -5)
                            run.OffTrackSteps++;

                        if (Episode.Last().Reward == -1)
                            run.OnTrackSteps++;
                        run.TotalReward += Episode.Last().Reward;
                        run.TotalSteps++;
                        var curr = Episode.Last();
                        Console.Write(
                            $"\r({curr.X},{curr.Y},{curr.VX},{curr.VY},{curr.AX},{curr.AY}) TP{curr.StateType} R{curr.Reward} st{run.TotalSteps} t{stopwatch.Elapsed.ToString(@"mm\:ss")}");
                        if (run.TotalSteps >= Config.ExplorerMaxIterations)
                            throw new Exception("Max iterations reached");
                    } while (MoveAgent(Episode.Last()).StateType != 1);
                    Console.WriteLine("");
                }
                catch (Exception ex)
                {
                    run.FailedRace = true;
                    Console.WriteLine("");
                    Logger.Warn($"Stopping Run due to Exception: {ex}");
                }

                stopwatch.Stop();
                run.RunTime = stopwatch.Elapsed.ToString();
                run.TotalSteps += 1;
                run.TotalReward += Episode.Last().Reward;
                Runs.Add(run);
            }

            if (Runs.Where(f => !f.FailedRace).Any())
            {
                MaxRunReward = Runs.Where(f => !f.FailedRace).DefaultIfEmpty().Max(x => x.TotalReward);
                MaxRunSteps = Runs.Where(f => !f.FailedRace).Max(x => x.TotalSteps);
            }
            else
            {
                MaxRunReward = double.MinValue;
                MaxRunSteps = int.MaxValue;
            }
            Console.WriteLine(ToString());
            Logger.Info("Agent Race/Iteration done");
        }

        public QState MoveAgent(QState currQstate)
        {
            switch (ValueFunction)
            {
                case ValueFunction.One:
                    return GetState_Vf1(currQstate);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public QState GetState_Vf1(QState cs)
        {
            try
            {
                QAction action = null;
                if (Episode.Count == 1)
                    action = cs.Actions.Actions.Single(a => (a.X == -1) && (a.Y == -1));
                else
                    action = cs.Actions.GetAction(cs);

                var ns = cs.NextQState(action);
                QState qState = null;
                if (Episode.Count != 0)
                    qState = Episode.LastOrDefault(x =>
                            (ns.X > x.XRange[0])
                            && (ns.Y > x.YRange[0])
                            && (ns.X < x.XRange[1])
                            && (ns.Y < x.YRange[1])
                            && (ns.VX > x.VX - x.VEst)
                            && (ns.VY > x.VY - x.VEst)
                            && (ns.VX < x.VX + x.VEst)
                            && (ns.VY < x.VY + x.VEst)
                    );
                if (qState != null)
                    ns.Actions = qState.Actions;

                Episode.Add(ns);

                var maxQ = ns.Actions.GetMaxExpectedQValue(ns);
                action.UpdateWeights(ns, maxQ);
                return ns;
            }
            catch (Exception ex)
            {
                Logger.Warn($"GetState_Vf1:{ex}");
                throw;
            }
        }
    }
}