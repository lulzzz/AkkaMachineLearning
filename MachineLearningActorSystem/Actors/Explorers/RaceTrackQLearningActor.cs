using System;
using System.Collections.Generic;
using System.Linq;
using MachineLearningActorSystem.Core;
using MachineLearningActorSystem.RaceTrack;

namespace MachineLearningActorSystem.Actors.Explorers
{
    public class RaceTrackQLearningActor : BaseActor
    {
        public RaceTrackQLearningActor()
        {
            // TODO: LearningEvent
            // TODO: TestingEvent

            Receive<QRaceTrack>(raceTrack =>
            {
                logger.Info($"{raceTrack.GetType().Name} Received");
                raceTrack = StartAgents(raceTrack);
                Console.Write(raceTrack);
            });
        }

        private QRaceTrack StartAgents(QRaceTrack raceTrack)
        {
            if (raceTrack.Races != null)
            {
                Console.WriteLine("Agent Race started");

                var qAgents = new List<QAgent>() { new QAgent(Config.ExplorerLearningRate, ValueFunction.One) };
                var qRace = new QRace(raceTrack.Races.Count + 1, DateTime.Now.ToString(), qAgents);
                qRace.Start();
                raceTrack.Races.Add(qRace);
                Console.WriteLine(raceTrack.ToString());
                Console.WriteLine("Agent Race completed");

                RaceTrackHelper.SaveQRaceTrack(raceTrack);
            }
            
            return raceTrack;
        }
    }
}