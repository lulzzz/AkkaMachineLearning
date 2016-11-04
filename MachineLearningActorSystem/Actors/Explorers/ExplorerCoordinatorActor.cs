using System;
using System.Collections.Generic;
using System.IO;
using Akka.Actor;
using MachineLearningActorSystem.Events;
using MachineLearningActorSystem.RaceTrack;

namespace MachineLearningActorSystem.Actors.Explorers
{
    public class ExplorerCoordinatorActor : BaseActor
    {
        private readonly IActorRef _raceTrackValueIterationActor = Context.ActorOf(Props.Create<RaceTrackValueIterationActor>(), typeof(RaceTrackValueIterationActor).Name);
        private readonly IActorRef _raceTrackQLearningActor = Context.ActorOf(Props.Create<RaceTrackQLearningActor>(), typeof(RaceTrackQLearningActor).Name);

        public ExplorerCoordinatorActor()
        {
            Receive<FileExplorerDataSourceEvent>(fileDataSourceEvent =>
            {
                logger.Info($"{fileDataSourceEvent.GetType().Name} Received");
                var raceTrack = ReadMapFile(fileDataSourceEvent);
                _raceTrackValueIterationActor.Tell(raceTrack);
            });

            Receive<RaceTrackQLearningEvent>(raceTrackQLearningEvent =>
            {
                logger.Info($"{raceTrackQLearningEvent.GetType().Name} Received");
                var raceTrack = ReadQFile();
                _raceTrackQLearningActor.Tell(raceTrack);
            });
        }

        private RaceTrack.RaceTrack ReadMapFile(FileExplorerDataSourceEvent dataSource)
        {
            var raceTrack = RaceTrackHelper.LoadRaceTrack();

            if (raceTrack != null)
            {
                Console.WriteLine("RaceTrack map file already exists.. using xml");
                return raceTrack;
            }

            Console.WriteLine("Reading RaceTrack map file started");

            StreamReader reader = null;

            int mapWidth = -1;
            int mapHeight = -1;
            List<State> states = new List<State>();

            try
            {
                reader = File.OpenText(dataSource.DataSourceFilePath);

                string str = null;
                // line counter
                int lines = 0;
                int y = 0;

                // read the file
                while ((str = reader.ReadLine()) != null)
                {
                    str = str.Trim();

                    // skip comments and empty lines
                    if ((str == string.Empty) || (str[0] == ';') || (str[0] == '\0'))
                        continue;

                    // split the string
                    string[] strs = str.Split(' ');

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
                            for (int x = 0; x < mapWidth; x++)
                            {
                                var stateType = (StateType)int.Parse(strs[x]);
                                if (stateType != StateType.OffWorld)
                                {
                                    states.Add(new State(x, y, 0, 0, stateType));
                                }
                            }
                            y++;
                        }
                    }
                    lines++;
                }


                Console.WriteLine("Reading RaceTrack map file completed");

                raceTrack = new RaceTrack.RaceTrack(states);
                RaceTrackHelper.SaveRaceTrack(raceTrack);

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

        private QRaceTrack ReadQFile()
        {
            var raceTrack = RaceTrackHelper.LoadQRaceTrack();

            if (raceTrack != null)
            {
                return raceTrack;
            }
            else
            {
                return new QRaceTrack();
            }
        }
    }
}