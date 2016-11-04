using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MachineLearningActorSystem.RaceTrack
{
    [Serializable]
    public class State
    {
        public State()
        {

        }

        public int X { get; set; }
        public int Y { get; set; }
        public int VX { get; set; }
        public int VY { get; set; }
        public StateType Type { get; set; }
        public double Value { get; set; }
        public double? NewValue { get; set; }
        public List<Action> Policy { get; set; }

        public State(int x, int y, int vx, int vy, StateType type)
        {
            X = x;
            Y = y;
            VX = vx;
            VY = vy;
            Type = type;
            Value = type == StateType.Goal ? 0 : -1;
            Policy = new List<Action>();
        }

        public override string ToString()
        {
            string p = string.Empty;
            foreach (var action in Policy)
            {
                p += $"({action.X},{action.Y})";
            }
            var sb = new StringBuilder();
            sb.Append($"{Type} X: {X} Y: {Y} VX: {VX} VY: {VY} Policy: {p}");

            return sb.ToString();
        }

        public int CalculateValue(ref List<State> states)
        {
            if (Type == StateType.Goal || !Policy.Any()) return 0;

            foreach (var action in Policy)
            {
                int dvx = VX + action.X;
                int dvy = VY + action.Y;
                int dx = X + dvx;
                int dy = Y + dvy;
                action.ReturnValue = states.Single(s => s.X == dx && s.Y == dy && s.VX == dvx && s.VY == dvy).Value;
            }

            if (Policy.Any())
            {
                NewValue = Policy.Average(a => a.ReturnValue.Value);
                return Policy.Count;
            }

            return 0;
        }

        public double UpdateValue()
        {
            var delta = Math.Abs(Value - (NewValue ?? Value));
            Value = NewValue ?? Value;
            NewValue = null;
            return delta;
        }

        public int UpdatePolicy()
        {
            if (!Policy.Any()) return 0;

            var policyCount = Policy.Count;
            var maxReturnValue = Policy.Max(a => a.ReturnValue.Value);
            var deleteCount = Policy.Count(a => a.ReturnValue.Value != maxReturnValue);
            if (deleteCount != policyCount)
            {
                Policy.RemoveAll(a => a.ReturnValue.Value != maxReturnValue);
            }
            Policy.ForEach(x => x.ReturnValue = null);

            var delta = policyCount - Policy.Count;
            return delta;
        }
    }
}