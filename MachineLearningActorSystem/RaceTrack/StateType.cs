using System;

namespace MachineLearningActorSystem.RaceTrack
{
    [Serializable]
    public enum StateType
    {
        World = 0,
        OffWorld = 1,
        Goal = 2,
        Start = 3
    }
}