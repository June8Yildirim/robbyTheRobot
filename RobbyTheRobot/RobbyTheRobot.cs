using System;
using System.IO;
using GeneticLibrary;

namespace RobbyTheRobot
{
    public class RobbyTheRobot : IRobbyTheRobot
    {
        private Random _rand;
        private IGeneticAlgorithm _ga;
        private ContentsOfGrid[][,] _testGrids;

        public RobbyTheRobot(
            int numberOfGenerations,
            int populationSize,
            int numberOfTrials,
            int? seed)
        {
            NumberOfGenerations = numberOfGenerations;
            NumberOfActions = 200;
            NumberOfTestGrids = 100;
            GridSize = 10;
            MutationRate = 0.05;
            EliteRate = 0.05;

            _rand = seed.HasValue ? new Random(seed.Value) : new Random();

            _ga = GeneticLib.CreateGeneticAlgorithm(
                populationSize,
                243,   // numGenes: 3^5 situations
                7,     // lengthOfGene: 7 possible moves (PossibleMoves enum)
                MutationRate,
                EliteRate,
                numberOfTrials,
                RobbyFitness,
                seed
            );
        }

        public int NumberOfActions { get; }

        public int NumberOfTestGrids { get; }

        public int GridSize { get; }

        public int NumberOfGenerations { get; }

        public double MutationRate { get; }

        public double EliteRate { get; }

        /// <summary>
        /// Generates a single test grid with exactly 50% of positions containing a can.
        /// Uses a counter to guarantee the exact count.
        /// </summary>
        public ContentsOfGrid[,] GenerateRandomTestGrid()
        {
            var grid = new ContentsOfGrid[GridSize, GridSize];
            int totalCells = GridSize * GridSize;
            int cansToPlace = totalCells / 2;
            int placed = 0;
            int remaining = totalCells;

            for (int x = 0; x < GridSize; x++)
            {
                for (int y = 0; y < GridSize; y++)
                {
                    // Place a can proportionally so we hit exactly cansToPlace
                    if (_rand.Next(remaining) < cansToPlace - placed)
                    {
                        grid[x, y] = ContentsOfGrid.Can;
                        placed++;
                    }
                    else
                    {
                        grid[x, y] = ContentsOfGrid.Empty;
                    }
                    remaining--;
                }
            }

            return grid;
        }

        /// <summary>
        /// Fitness function: runs Robby on all test grids and returns the average score.
        /// </summary>
        private double RobbyFitness(IChromosome chromosome, IGeneration generation)
        {
            // Use Random.Shared (thread-safe) so parallel chromosome evaluation is safe
            var rng = Random.Shared;
            double totalScore = 0;
            for (int g = 0; g < _testGrids.Length; g++)
            {
                // Clone the grid since Robby removes cans as he picks them up
                ContentsOfGrid[,] gridCopy = (ContentsOfGrid[,])_testGrids[g].Clone();
                int x = rng.Next(GridSize);
                int y = rng.Next(GridSize);

                for (int a = 0; a < NumberOfActions; a++)
                {
                    totalScore += RobbyHelper.ScoreForAllele(chromosome.Genes, gridCopy, rng, ref x, ref y);
                }
            }
            return totalScore / NumberOfTestGrids;
        }

        /// <summary>
        /// Runs the GA for NumberOfGenerations and writes the best chromosome from
        /// generations 1, 20, 100, 200, 500, and 1000 to separate files in folderPath.
        /// File format: maxScore,numActions,gene0,gene1,...,gene242
        /// </summary>
        public void GeneratePossibleSolutions(string folderPath)
        {
            int[] generationsToSave = { 1, 20, 100, 200, 500, 1000 };
            Directory.CreateDirectory(folderPath);

            for (int gen = 1; gen <= NumberOfGenerations; gen++)
            {
                // Fresh test grids for this generation so all chromosomes are evaluated fairly
                _testGrids = new ContentsOfGrid[NumberOfTestGrids][,];
                for (int i = 0; i < NumberOfTestGrids; i++)
                    _testGrids[i] = GenerateRandomTestGrid();

                IGeneration currentGen = _ga.GenerateGeneration();

                Console.WriteLine($"Generation {gen}: best fitness = {currentGen[0].Fitness:F2}");

                if (Array.IndexOf(generationsToSave, gen) >= 0)
                {
                    IChromosome best = currentGen[0]; // index 0 = highest fitness after sort
                    string content = $"{best.Fitness},{NumberOfActions},{string.Join(",", best.Genes)}";
                    string fileName = Path.Combine(folderPath, $"generation_{gen}.txt");
                    File.WriteAllText(fileName, content);
                    Console.WriteLine($"  -> Saved to {fileName}");
                }
            }
        }
    }
}
