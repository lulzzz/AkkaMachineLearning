using System;
using System.Collections.Generic;
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
                logger.Info("Agent Race started");

                var qAgents = new List<QAgent>();

                for (var lr = 1; lr <= 10; lr++)
                    for (var er = 1; er <= 10; er++)
                        for (var dr = 1; dr <= 10; dr++)
                            qAgents.Add(new QAgent(ValueFunction.One, lr*0.1, er*0.1, dr*0.1, 3));
                var qRace = new QRace(raceTrack.Races.Count + 1, DateTime.Now.ToString(), qAgents);
                qRace.Start();
                raceTrack.Races.Add(qRace);

                logger.Info("Agent Race completed");

                XmlHelper.SaveQRaceTrack(raceTrack);
            }

            return raceTrack;
        }
    }
}