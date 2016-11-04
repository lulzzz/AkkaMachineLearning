using System;
using System.Collections.Generic;
using System.IO;
using log4net.Config;
using MachineLearningActorSystem.Core;
using MachineLearningActorSystem.Events;

namespace MachineLearningActorSystem
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            XmlConfigurator.Configure();

            if (Environment.UserInteractive)
            {
                var commands = new List<KeyValuePair<string, string>>();
                commands.Add(new KeyValuePair<string, string>("cdc",
                    "classifier decisiontree crossvalidation .\\Resources\\letter-recognition.data"));
                commands.Add(new KeyValuePair<string, string>("cdb",
                    "classifier decisiontree bootstrap .\\Resources\\letter-recognition.data"));
                commands.Add(new KeyValuePair<string, string>("ckc",
                    "classifier knn crossvalidation .\\Resources\\letter-recognition.data"));
                commands.Add(new KeyValuePair<string, string>("ckb",
                    "classifier knn bootstrap .\\Resources\\letter-recognition.data"));
                commands.Add(new KeyValuePair<string, string>("nbc",
                    "classifier naivebayes crossvalidation .\\Resources\\letter-recognition.data"));
                commands.Add(new KeyValuePair<string, string>("nbb",
                    "classifier naivebayes bootstrap .\\Resources\\letter-recognition.data"));
                commands.Add(new KeyValuePair<string, string>("cfb",
                    "classifier find best .\\Resources\\letter-recognition.data"));
                commands.Add(new KeyValuePair<string, string>("rv",
                    "racetrack valueiteration .\\Resources\\Racetrack.map"));
                commands.Add(new KeyValuePair<string, string>("rq", "racetrack qlearning"));

                var standardOutput = new StreamWriter(Console.OpenStandardOutput()) {AutoFlush = true};
                Console.SetOut(standardOutput);

                // If Ctrl-C is pressed in the console, we get to here.
                Console.CancelKeyPress += cancelKeyHandler;

                MlActorSystem.Start();

                var cmd = string.Empty;
                var classifierDataFilePath = ".\\Resources\\letter-recognition.data";
                var explorerDataFilePath = ".\\Resources\\Racetrack.map";
                var actorstarted = false;

                if (args.Length == 2)
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
                else if (args.Length == 1)
                {
                    cmd = args[0].ToLower();
                }

                do
                {
                    if (string.IsNullOrEmpty(cmd))
                    {
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
                            Console.WriteLine($"    {command.Key}: {command.Value}");
                        cmd = Console.ReadLine();
                    }

                    switch (cmd.ToLower())
                    {
                        case "rv":
                            Console.Clear();
                            Console.WriteLine("ValueIteration selected.");
                            Console.WriteLine("Press P to print past results or any other key to start a new run.");
                            cmd = Console.ReadKey(false).KeyChar.ToString().ToLower();
                            MlActorSystem.ExplorerCoordinatorActor.Tell(
                                new FileExplorerDataSourceEvent(explorerDataFilePath, cmd == "p"), null);
                            actorstarted = true;
                            break;

                        case "rq":
                            Console.Clear();
                            Console.WriteLine("QLearning selected.");
                            Console.WriteLine("Press P to print past results or any other key to start a new run.");
                            cmd = Console.ReadKey(false).KeyChar.ToString().ToLower();
                            MlActorSystem.ExplorerCoordinatorActor.Tell(
                                new RaceTrackQLearningEvent(cmd == "p"), null);
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
                            cmd = string.Empty;
                            break;
                    }
                    cmd = string.Empty;
                } while (!actorstarted);

                do
                {
                    Console.WriteLine("Ctrl+C to quit");
                    Console.ReadKey();
                } while (true);
            }
        }

        private static void cancelKeyHandler(object sender, ConsoleCancelEventArgs args)
        {
            Console.WriteLine("Ctrl+C pressed.");
            Console.WriteLine("Press any key to exit gracefully (may take a while if long process has started)");
            Console.WriteLine("OR press T key TWICE to stop immediately (may result in corrupt data)");
            var key = Console.ReadKey(false);
            if (key.KeyChar.ToString().ToLower() == "t")
                MlActorSystem.Stop();
            else
                MlActorSystem.Reaper();
        }
    }
}