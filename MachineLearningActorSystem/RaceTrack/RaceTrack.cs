using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MachineLearningActorSystem.RaceTrack
{
    [Serializable]
    public class RaceTrack
    {
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

        public RaceTrack()
        {
            InitialStates = new List<State>();
            StatesAfterLearning = new List<State>();
            Iterations = new List<Iteration>();
            Races = new List<Race>();
        }

        public RaceTrack(List<State> states)
        {
            Console.WriteLine("Creation of Full Initial RaceTrack and actions started");

            StatesAfterLearning = new List<State>();
            Iterations = new List<Iteration>();
            Races = new List<Race>();
            InitialStates = new List<State>();

            foreach (var state in states)
            {
                for (int vx = -4; vx <= 4; vx++)
                {
                    for (int vy = -4; vy <= 4; vy++)
                    {
                        // Don't add velocity 0,0 again
                        if (vx == 0 && vy == 0)
                        {
                            InitialStates.Add(state);
                        }
                        else
                        {
                            // Don't create states off world
                            var dx = state.X + vx + (vx > 0 ? -1 : vx < 0 ? 1 : 0);
                            var dy = state.Y + vy + (vy > 0 ? -1 : vy < 0 ? 1 : 0);
                            if (states.Exists(s => s.X == dx && s.Y == dy && s.VX == 0 && s.VY == 0))
                            {
                                var newStateType = state.Type == StateType.Start
                                    ? StateType.World
                                    : state.Type;

                                // Create states for all velocities
                                InitialStates.Add(new State(state.X, state.Y, vx, vy, newStateType));
                            }
                        }
                    }
                }
            }

            foreach (var state in InitialStates)
            {
                for (int ax = -1; ax <= 1; ax++)
                {
                    for (int ay = -1; ay <= 1; ay++)
                    {
                        // Filter out actions causing "stopped" loops
                        if (!(state.VX == 0 && state.VY == 0 && ax == 0 && ay == 0))
                        {
                            var dvx = state.VX + ax;
                            var dvy = state.VY + ay;
                            var dx = state.X + dvx;
                            var dy = state.Y + dvy;

                            // Actions created if they point to states within the racetrack
                            if (InitialStates.Exists(s => s.X == dx && s.Y == dy && s.VX == dvx && s.VY == dvy))
                            {
                                // Don't create actions for terminal states
                                if (state.Type != StateType.Goal)
                                {
                                    state.Policy.Add(new Action(ax, ay));
                                }
                            }
                        }
                    }
                }
            }

            NumberOfInitialStates = InitialStates.Count;
            NumberOfInitialActions = InitialStates.Sum(s => s.Policy?.Count ?? 0);

            Console.WriteLine("Creation of Full Initial RaceTrack and actions completed");
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"TimeToLearn: {TimeToLearn}");
            sb.AppendLine($"NumberOfIterations: {NumberOfIterations}");
            sb.AppendLine($"NumberOfInitialStates: {NumberOfInitialStates}");
            sb.AppendLine($"NumberOfInitialActions: {NumberOfInitialActions}");
            sb.AppendLine($"NumberOfStatesAfterLearning: {NumberOfStatesAfterLearning}");
            sb.AppendLine($"NumberOfActionsAfterLearning: {NumberOfActionsAfterLearning}");
            foreach (var iteration in Iterations)
            {
                sb.AppendLine(iteration.ToString());
            }
            foreach (var race in Races)
            {
                sb.AppendLine(race.ToString());
            }

            return sb.ToString();
        }
    }
}
