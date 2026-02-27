using System;

namespace RobbyIterationGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            string outputFolder = args.Length > 0 ? args[0] : "output";

            Console.WriteLine("Starting Robby the Robot Genetic Algorithm...");
            Console.WriteLine($"Output folder: {outputFolder}");

            var robby = new RobbyTheRobot.RobbyTheRobot(
                numberOfGenerations: 1000,
                populationSize: 200,
                numberOfTrials: 1,
                seed: null
            );

            robby.GeneratePossibleSolutions(outputFolder);

            Console.WriteLine($"Done! Results saved to '{outputFolder}'.");
        }
    }
}
