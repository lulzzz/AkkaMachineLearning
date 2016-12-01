namespace Sandbox
{
    using System;
    using System.Threading;
    using Vector = java.util.Vector;

    class Program
    {
        private static bool _stop;

        static void Main(string[] args)
        {
            Console.CancelKeyPress += new ConsoleCancelEventHandler(cancelKeyHandler);

            IDemo demo = new AccordDemo();

            if (args.Length < 6)
            {
                Console.WriteLine(demo.Usage());
                return;
            }

            // parse command line
            string classifier = "";
            string filter = "";
            string dataset = "";
            Vector classifierOptions = new Vector();
            Vector filterOptions = new Vector();

            int i = 0;
            string current = "";
            bool newPart = false;
            do
            {
                // determine part of command line
                if (args[i].Equals("CLASSIFIER"))
                {
                    current = args[i];
                    i++;
                    newPart = true;
                }
                else if (args[i].Equals("FILTER"))
                {
                    current = args[i];
                    i++;
                    newPart = true;
                }
                else if (args[i].Equals("DATASET"))
                {
                    current = args[i];
                    i++;
                    newPart = true;
                }

                if (current.Equals("CLASSIFIER"))
                {
                    if (newPart)
                        classifier = args[i];
                    else
                        classifierOptions.add(args[i]);
                }
                else if (current.Equals("FILTER"))
                {
                    if (newPart)
                        filter = args[i];
                    else
                        filterOptions.add(args[i]);
                }
                else if (current.Equals("DATASET"))
                {
                    if (newPart)
                        dataset = args[i];
                }

                // next parameter
                i++;
                newPart = false;
            }
            while (i < args.Length);
            
            // everything provided?
            if ( classifier.Equals("") || filter.Equals("") || dataset.Equals("") )
            {
                Console.WriteLine("Not all parameters provided!");
                Console.WriteLine(demo.Usage());
                return;
            }

            demo.SetClassifier(classifier, (string[]) classifierOptions.toArray(new string[classifierOptions.size()]));
            demo.SetFilter(filter, (string[]) filterOptions.toArray(new string[filterOptions.size()]));
            demo.SetTraining(dataset);

            demo.Execute();

            Console.Write(demo.ToString());

            while (!_stop)
            {
                Thread.Sleep(5000);
            }
        }

        static void cancelKeyHandler(object sender, ConsoleCancelEventArgs args)
        {
            _stop = true;
            System.Console.WriteLine("Ctrl+C pressed.");
        }
    }
}
