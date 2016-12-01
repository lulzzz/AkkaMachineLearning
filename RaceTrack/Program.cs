using System;
using ApiApp.Data;
using Autofac;
using Racetrack;

namespace GradientDescentTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var builder = new ContainerBuilder();

            builder.RegisterModule(new DataModule());
            var gd = new QLearningGradientDescent();
            gd.Start(10, 50, 10000);
            Console.ReadLine();
        }
    }
}
