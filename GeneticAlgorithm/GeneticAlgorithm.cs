using System;

namespace GeneticLibrary
{
    class GeneticAlgorithm : IGeneticAlgorithm
    {
        private Random _rand;
        private long _generationCount;
        private IGeneration _currentGeneration;

        public GeneticAlgorithm(int populationSize, int numberOfGenes, int lengthOfGenes, double mutationRate, double eliteRate,
            int numberOfTrials, FitnessEventHandler fitnessFunc, int? seed = null)
        {
            PopulationSize = populationSize;
            NumberOfGenes = numberOfGenes;
            LengthOfGene = lengthOfGenes;
            MutationRate = mutationRate;
            EliteRate = eliteRate;
            NumberOfTrials = numberOfTrials;
            FitnessCalculation = fitnessFunc;
            _rand = seed.HasValue ? new Random(seed.Value) : new Random();
        }

        public int PopulationSize { get; }

        public int NumberOfGenes { get; }

        public int LengthOfGene { get; }

        public double MutationRate { get; }

        public double EliteRate { get; }

        public int NumberOfTrials { get; }

        public long GenerationCount => _generationCount;

        public IGeneration CurrentGeneration => _currentGeneration;

        public FitnessEventHandler FitnessCalculation { get; }

        /// <summary>
        /// Generates the next generation. On first call creates a random initial population.
        /// Subsequent calls apply elitism and crossover to evolve the population.
        /// </summary>
        public IGeneration GenerateGeneration()
        {
            if (_currentGeneration == null)
            {
                // First generation: random population
                var gen = new Generation(this, FitnessCalculation);
                gen.EvaluateFitnessOfPopulation();
                _currentGeneration = gen;
                _generationCount = 1;
            }
            else
            {
                // Determine elite count; must be even since offspring come in pairs
                int eliteCount = (int)(PopulationSize * EliteRate);
                if (eliteCount % 2 != 0) eliteCount--;

                IChromosome[] newChromosomes = new IChromosome[PopulationSize];

                // Carry over elite chromosomes (current generation is sorted descending)
                for (int i = 0; i < eliteCount; i++)
                {
                    newChromosomes[i] = new Chromosome(_currentGeneration[i]);
                }

                // Fill remaining slots with offspring
                var currentGen = (IGenerationDetails)_currentGeneration;
                for (int i = eliteCount; i < PopulationSize; i += 2)
                {
                    IChromosome parent1 = currentGen.SelectParent();
                    IChromosome parent2 = currentGen.SelectParent();
                    IChromosome[] offspring = parent1.Reproduce(parent2, MutationRate);
                    newChromosomes[i] = offspring[0];
                    if (i + 1 < PopulationSize)
                        newChromosomes[i + 1] = offspring[1];
                }

                var nextGen = new Generation(newChromosomes, this, FitnessCalculation);
                nextGen.EvaluateFitnessOfPopulation();
                _currentGeneration = nextGen;
                _generationCount++;
            }

            return _currentGeneration;
        }
    }
}
