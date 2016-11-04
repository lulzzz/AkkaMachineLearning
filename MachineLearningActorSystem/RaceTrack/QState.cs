using System;
using System.Text;
using System.Xml.Serialization;

namespace MachineLearningActorSystem.RaceTrack
{
    [Serializable]
    public class QState
    {
        public QState()
        {
            Actions = new QActions();
            XRange = new double[2];
            YRange = new double[2];
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
            Actions = new QActions(true);
            XRange = new double[2];
            YRange = new double[2];
        }

        // Simulate
        public QState(double x, double y, double vx, double vy, double reward, double stateType, double ax, double ay,
            QState prevQState)
        {
            if (prevQState.PrevQState == null)
            {
                var pdxR = Math.Abs(x - prevQState.X);
                var pdyR = Math.Abs(y - prevQState.Y);
                prevQState.XRange[0] = pdxR - pdxR/2;
                prevQState.XRange[1] = pdxR + pdxR/2;
                prevQState.YRange[0] = pdyR - pdyR/2;
                prevQState.YRange[1] = pdyR + pdyR/2;
                var dvxR = Math.Abs(vx - prevQState.VX);
                var dvyR = Math.Abs(vy - prevQState.VY);
                prevQState.VEst = dvxR + dvyR/2;
            }

            var dxR = Math.Abs((prevQState.XRange[1] - prevQState.XRange[0])/2);
            var dyR = Math.Abs((prevQState.YRange[1] - prevQState.YRange[0])/2);
            XRange = new[] {x - dxR, x + dxR};
            YRange = new[] {y - dyR, y + dyR};
            VEst = prevQState.VEst;

            EpisodeGuid = prevQState.EpisodeGuid;
            X = x;
            Y = y;
            VX = vx;
            VY = vy;
            AX = ax;
            AY = ay;
            StateType = stateType;
            Reward = reward;
            PrevQState = prevQState;
            Actions = new QActions(true);
        }

        public Guid EpisodeGuid { get; set; }
        public double[] XRange { get; set; }
        public double[] YRange { get; set; }
        public double VEst { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double VX { get; set; }
        public double VY { get; set; }
        public double AX { get; set; }
        public double AY { get; set; }
        public double Reward { get; set; }
        public double StateType { get; set; } // 1 = Terminal, 0 = Other

        [XmlIgnore]
        public QState PrevQState { get; set; }

        public QActions Actions { get; set; }

        public QState NextQState(QAction a)
        {
            return RaceTrackSimulator.Simulate(a.X, a.Y, this);
        }

        public override string ToString()
        {
            var type = StateType == 1 ? "TERMINAL" : "WORLD";
            var sb = new StringBuilder();
            sb.Append($"{type} X: {X} Y: {Y} VX: {VX} VY: {VY} AX: {AX} AY: {AY} R: {Reward}");

            return sb.ToString();
        }
    }
}