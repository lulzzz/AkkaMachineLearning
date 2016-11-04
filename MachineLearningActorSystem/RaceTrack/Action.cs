using System;

namespace MachineLearningActorSystem.RaceTrack
{
    [Serializable]
    public class Action
    {
        public Action()
        {
        }

        public Action(int x, int y)
        {
            X = x;
            Y = y;
        }

        public int X { get; set; }
        public int Y { get; set; }
        public double? ReturnValue { get; set; }
    }
}