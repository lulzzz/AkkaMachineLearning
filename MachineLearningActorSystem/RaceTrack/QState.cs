using System;
using System.Text;

namespace MachineLearningActorSystem.RaceTrack
{
    [Serializable]
    public class QState
    {
        public Guid EpisodeGuid { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double VX { get; set; }
        public double VY { get; set; }
        public double AX { get; set; }
        public double AY { get; set; }
        public double Reward { get; set; }
        public double StateType { get; set; } // 1 = Terminal, 0 = Other
        public QState PrevQState { get; set; }

        public QState()
        {
            PrevQState = null;
        }

        // Start Episode
        public QState(Guid guid, double x, double y, double vx, double vy, double ax, double ay)
        {
            EpisodeGuid = guid;
            X = x;
            Y = y;
            VX = vx;
            VY = vy;
            AX = ax;
            AY = ay;
            StateType = 0;
            Reward = 0;
            PrevQState = null;
        }

        // Simulate
        public QState(double x, double y, double vx, double vy, double reward, double stateType, double ax, double ay, QState prevQState)
        {
            EpisodeGuid = prevQState.EpisodeGuid;
            X = x;
            Y = y;
            VX = vx;
            VY = vy;
            AX = ax;
            AY = ax;
            StateType = stateType;
            Reward = reward;
            PrevQState = prevQState;
        }

        public QState NextQState(Tuple<double, double> action)
        {
            return RaceTrackSimulator.Simulate(action.Item1, action.Item2, this);
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append($"Type:{StateType} X: {X} Y: {Y} VX: {VX} VY: {VY}");

            return sb.ToString();
        }
    }
}