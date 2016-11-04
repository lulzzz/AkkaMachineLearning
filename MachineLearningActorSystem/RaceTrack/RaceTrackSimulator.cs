using System;

namespace MachineLearningActorSystem.RaceTrack
{
    public static class RaceTrackSimulator
    {
        // RaceTrackSim was converted from racetrack.jar (java bytecode) to IKVM. (IKVM is a java interpreter for C#)
        private static readonly RaceTrackSim Current = new RaceTrackSim();
        private static Guid _guid;

        public static QState StartEpisode()
        {
            _guid = Guid.NewGuid();

            var es = Current.startEpisode();

            return new QState(_guid, es[0], es[1], es[2], es[3], es[4], es[5]);
        }

        public static QState Simulate(double ax, double ay, QState currQState)
        {
            if (_guid != currQState.EpisodeGuid)
                throw new ArgumentException("An episode can only simulate movement for QStates in the same episode.");

            var es = Current.simulate(currQState.X, currQState.Y, currQState.VX, currQState.VY, ax, ay);
            return new QState(es[0], es[1], es[2], es[3], es[4], es[5], ax, ay, currQState);
        }
    }
}