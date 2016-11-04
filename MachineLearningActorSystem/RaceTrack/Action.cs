using System;

namespace MachineLearningActorSystem.RaceTrack
{
    [Serializable]
    public class Action
    {
        public Action()
        {

        }

        public int X { get; set; }
        public int Y { get; set; }
        public double? ReturnValue { get; set; }

        public Action(int x, int y)
        {
            X = x;
            Y = y;
        }

    }
}