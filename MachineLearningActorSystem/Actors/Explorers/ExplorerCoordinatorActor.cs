using System;
using System.Collections.Generic;
using System.IO;
using Akka.Actor;
using MachineLearningActorSystem.Core;
using MachineLearningActorSystem.Events;
using MachineLearningActorSystem.RaceTrack;

namespace MachineLearningActorSystem.Actors.Explorers
{
    public class ExplorerCoordinatorActor : BaseActor
    {
        private readonly IActorRef _raceTrackValueIterationActor =
            Context.ActorOf(Props.Create<RaceTrackValueIterationActor>(), typeof(RaceTrackValueIterationActor).Name);

        public ExplorerCoordinatorActor()
        {
            Receive<FileExplorerDataSourceEvent>(fileDataSourceEvent =>
            {
                logger.Info($"{fileDataSourceEvent.GetType().Name} Received");
                var raceTrack = ReadMapFile(fileDataSourceEvent);
                if ((raceTrack.Races != null) && (raceTrack.Races.Count > 0) && fileDataSourceEvent.PrintLastRun)
                {
                    Console.Write(raceTrack);
                    logger.Info($"{raceTrack.GetType().Name} ValueIteration Results Printed");
                }
                else
                {
                    _raceTrackValueIterationActor.Tell(raceTrack);
                }
            });
        }

        private RaceTrack.RaceTrack ReadMapFile(FileExplorerDataSourceEvent dataSource)
        {
            var raceTrack = XmlHelper.LoadRaceTrack();

            if (dataSource.PrintLastRun && (raceTrack != null))
            {
                logger.Info("RaceTrack map file already exists.. using xml");
                return raceTrack;
            }

            logger.Info("Reading RaceTrack map file started");

            StreamReader reader = null;

            var mapWidth = -1;
            var mapHeight = -1;
            var states = new List<State>();

            try
            {
                reader = File.OpenText(dataSource.DataSourceFilePath);

                string str = null;
                // line counter
                var lines = 0;
                var y = 0;

                // read the file
                while ((str = reader.ReadLine()) != null)
                {
                    str = str.Trim();

                    // skip comments and empty lines
                    if ((str == string.Empty) || (str[0] == ';') || (str[0] == '\0'))
                        continue;

                    // split the string
                    var strs = str.Split(' ');

                    // check the line
                    if (lines == 0) // Map Size
                    {
                        // get world size
                        mapWidth = int.Parse(strs[0]);
                        mapHeight = int.Parse(strs[1]);
                    }
                    else if (lines > 0) // World
                    {
                        // map lines
                        if (y < mapHeight)
                        {
                            for (var x = 0; x < mapWidth; x++)
                            {
                                var stateType = (StateType) int.Parse(strs[x]);
                                if (stateType != StateType.OffWorld)
                                    states.Add(new State(x, y, 0, 0, stateType));
                            }
                            y++;
                        }
                    }
                    lines++;
                }


                logger.Info("Reading RaceTrack map file completed");

                raceTrack = new RaceTrack.RaceTrack(states);
                XmlHelper.SaveRaceTrack(raceTrack);

                return raceTrack;
            }
            catch (Exception ex)
            {
                logger.Error($"Failed to load data from file: {dataSource.DataSourceFilePath}. Error: {ex.Message}");
            }
            finally
            {
                // close file
                reader?.Close();
            }

            return null;
        }
    }
}