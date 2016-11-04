using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;

namespace MachineLearningActorSystem.RaceTrack
{
    [Serializable]
    public class RaceTrack
    {
        [NonSerialized] private readonly ILog Logger = LogManager.GetLogger(typeof(RaceTrack));

        public RaceTrack()
        {
            InitialStates = new List<State>();
            StatesAfterLearning = new List<State>();
            Iterations = new List<Iteration>();
            Races = new List<Race>();
        }

        public RaceTrack(List<State> states)
        {
            Logger.Info("Creating initial RaceTrack and policies...");

            StatesAfterLearning = new List<State>();
            Iterations = new List<Iteration>();
            Races = new List<Race>();
            InitialStates = new List<State>();

            foreach (var state in states)
                for (var vx = -4; vx <= 4; vx++)
                    for (var vy = -4; vy <= 4; vy++)
                        if ((vx == 0) && (vy == 0))
                        {
                            InitialStates.Add(state);
                        }
                        else
                        {
                            // Don't create states off world
                            var dx = state.X + vx + (vx > 0 ? -1 : vx < 0 ? 1 : 0);
                            var dy = state.Y + vy + (vy > 0 ? -1 : vy < 0 ? 1 : 0);
                            if (states.Exists(s => (s.X == dx) && (s.Y == dy) && (s.VX == 0) && (s.VY == 0)))
                            {
                                var newStateType = state.Type == StateType.Start
                                    ? StateType.World
                                    : state.Type;

                                // Create states for all velocities
                                InitialStates.Add(new State(state.X, state.Y, vx, vy, newStateType));
                            }
                        }

            foreach (var state in InitialStates)
                for (var ax = -1; ax <= 1; ax++)
                    for (var ay = -1; ay <= 1; ay++)
                        if (!((state.VX == 0) && (state.VY == 0) && (ax == 0) && (ay == 0)))
                        {
                            var dvx = state.VX + ax;
                            var dvy = state.VY + ay;
                            var dx = state.X + dvx;
                            var dy = state.Y + dvy;

                            // Actions created if they point to states within the racetrack
                            if (InitialStates.Exists(s => (s.X == dx) && (s.Y == dy) && (s.VX == dvx) && (s.VY == dvy)))
                                if (state.Type != StateType.Goal)
                                    state.Policy.Add(new Action(ax, ay));
                        }

            NumberOfInitialStates = InitialStates.Count;
            NumberOfInitialActions = InitialStates.Sum(s => s.Policy?.Count ?? 0);
        }

        public int NumberOfInitialStates { get; set; }
        public int NumberOfInitialActions { get; set; }
        public int NumberOfStatesAfterLearning { get; set; }
        public int NumberOfActionsAfterLearning { get; set; }
        public string TimeToLearn { get; set; }
        public int NumberOfIterations { get; set; }
        public List<Iteration> Iterations { get; set; }
        public List<Race> Races { get; set; }
        public List<State> InitialStates { get; set; }
        public List<State> StatesAfterLearning { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("*************************************************************************************");
            sb.AppendLine($"TimeToLearn: {TimeToLearn} NumberOfIterations: {NumberOfIterations}");
            sb.AppendLine($"StatesPreLearn: {NumberOfInitialStates} ActionsPreLearn: {NumberOfInitialActions}");
            sb.AppendLine(
                $"StatesPostLearn: {NumberOfStatesAfterLearning} ActionsPostLearn: {NumberOfActionsAfterLearning}");
            sb.AppendLine("*************************************************************************************");
            sb.AppendLine($"Iterations:");
            foreach (var iteration in Iterations)
                sb.AppendLine($" {iteration}");
            sb.AppendLine("*************************************************************************************");
            foreach (var race in Races)
                sb.AppendLine(race.ToString());

            return sb.ToString();
        }
    }
}