using com.sun.tools.javah;

namespace MachineLearningActorSystem
{
    using System.Threading;
    using Core;
    using System;
    using System.IO;
    using System.Collections.Generic;
    using Events;

    class Program
    {
        static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();

            if (Environment.UserInteractive)
            {
                var commands = new List<KeyValuePair<string, string>>();
                commands.Add(new KeyValuePair<string, string>("cdc", "classifier decisiontree crossvalidation .\\Resources\\letter-recognition.data"));
                commands.Add(new KeyValuePair<string, string>("cdb", "classifier decisiontree bootstrap .\\Resources\\letter-recognition.data"));
                commands.Add(new KeyValuePair<string, string>("ckc", "classifier knn crossvalidation .\\Resources\\letter-recognition.data"));
                commands.Add(new KeyValuePair<string, string>("ckb", "classifier knn bootstrap .\\Resources\\letter-recognition.data"));
                commands.Add(new KeyValuePair<string, string>("nbc", "classifier naivebayes crossvalidation .\\Resources\\letter-recognition.data"));
                commands.Add(new KeyValuePair<string, string>("nbb", "classifier naivebayes bootstrap .\\Resources\\letter-recognition.data"));
                commands.Add(new KeyValuePair<string, string>("cfb", "classifier find best .\\Resources\\letter-recognition.data"));
                commands.Add(new KeyValuePair<string, string>("rv", "racetrack valueiteration .\\Resources\\Racetrack.map"));
                commands.Add(new KeyValuePair<string, string>("rq", "racetrack qlearning"));

                StreamWriter standardOutput = new StreamWriter(System.Console.OpenStandardOutput()) { AutoFlush = true };
                Console.SetOut(standardOutput);

                // If Ctrl-C is pressed in the console, we get to here.
                Console.CancelKeyPress += (cancelKeyHandler);

                MlActorSystem.Start();

                string cmd = string.Empty;
                string classifierDataFilePath = ".\\Resources\\letter-recognition.data";
                string explorerDataFilePath = ".\\Resources\\Racetrack.map";
                bool actorstarted = false;

                if(args.Length == 2)
                {
                    cmd = args[0].ToLower();
                    if (",cdc,cdb,ckc,ckb,nbc,nbb,cfb,rv".Contains($",{cmd},"))
                    {
                        var file = args[1].ToLower();

                        if (file.EndsWith(".map") && File.Exists(file))
                        {
                            explorerDataFilePath = file;
                        }
                        else if (file.EndsWith(".data") && File.Exists(file))
                        {
                            classifierDataFilePath = file;
                        }
                        else
                        {
                            Console.WriteLine("Incorrect file path or file extension.");
                            cmd = string.Empty;
                        }
                    }
                }

                do
                {
                    if(string.IsNullOrEmpty(cmd))
                    Console.WriteLine("Type a command and press enter to use default datasets.");
                    Console.WriteLine("    Note: Use command line arguments to load custom data, eg:");
                    Console.WriteLine("          MachineLearningActorSystem.exe rv .\\Resources\\Racetrack.map");
                    Console.WriteLine("    or:");
                    Console.WriteLine("          MachineLearningActorSystem.exe rq");
                    Console.WriteLine();
                    Console.WriteLine("***Press Ctrl+C to kill all running processes.***");
                    Console.WriteLine();
                    Console.WriteLine("Valid commands:");
                    foreach (var command in commands)
                    {
                        Console.WriteLine($"    {command.Key}: {command.Value}");
                    }
                    cmd = Console.ReadLine();

                    switch (cmd.ToLower())
                    {
                        case "rv":
                            Console.Clear();
                            Console.WriteLine("Explorer starting");
                            MlActorSystem.ExplorerCoordinatorActor.Tell(
                                new FileExplorerDataSourceEvent(
                                    explorerDataFilePath), null);
                            actorstarted = true;
                            break;

                        case "rq":
                            Console.Clear();
                            Console.WriteLine("Explorer starting");
                            MlActorSystem.ExplorerCoordinatorActor.Tell(
                                new RaceTrackQLearningEvent(), null);
                            actorstarted = true;
                            break;

                        case "cdc":
                        case "cdb":
                        case "ckc":
                        case "ckb":
                        case "nbc":
                        case "nbb":
                        case "cfb":
                            Console.Clear();
                            Console.WriteLine("Classifier starting");
                            MlActorSystem.ClassifierCoordinatorActor.Tell(
                                new FileClassifierDataSourceEvent(
                                    classifierDataFilePath, cmd), null);
                            actorstarted = true;
                            break;

                        default:
                            cmd = string.Empty; ;
                            break;
                    }
                } while (!actorstarted);

                Console.ReadKey(false);
            }
        }

        static void cancelKeyHandler(object sender, ConsoleCancelEventArgs args)
        {
            Console.WriteLine("Ctrl+C pressed.");
            Console.WriteLine("Press C to exit gracefully (may take a while if long process has started)");
            Console.WriteLine("OR press any other key to stop immediately (may result in corrupt data)");
            var key = Console.ReadKey(false);
            if (key.KeyChar.ToString().ToLower() == "c")
            {
                MlActorSystem.Stop();
            }
            else
            {
                MlActorSystem.Reaper();
            }
            
        }
    }
}
