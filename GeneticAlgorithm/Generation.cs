using System;
using System.Linq;
using System.Threading.Tasks;


namespace GeneticLibrary
{
  public class Generation : IGenerationDetails
  {
    public IGeneticAlgorithm GenericAlgorithm { get; }
    public IChromosome[] Chromosomes { get; set; }
    private FitnessEventHandler _fitnessEventHandler;
    private Random _rand;

    /// <summary>
    /// Creates a generation populated with random Chromosomes.
    /// </summary>
    public Generation(IGeneticAlgorithm geneticAlgorithm,
      FitnessEventHandler fitnessEventHandler, int? seed = null)
    {
      _fitnessEventHandler = fitnessEventHandler;
      GenericAlgorithm = geneticAlgorithm;
      _rand = seed.HasValue ? new Random(seed.Value) : new Random();

      Chromosomes = new IChromosome[geneticAlgorithm.PopulationSize];
      for (int i = 0; i < geneticAlgorithm.PopulationSize; i++)
      {
        int? chromosomeSeed = seed.HasValue ? (int?)(seed.Value + i) : null;
        Chromosomes[i] = new Chromosome(
          geneticAlgorithm.NumberOfGenes,
          geneticAlgorithm.LengthOfGene,
          chromosomeSeed);
      }
    }

    /// <summary>
    /// Creates a generation from a pre-built array of Chromosomes (used for next-generation creation).
    /// </summary>
    internal Generation(IChromosome[] chromosomes, IGeneticAlgorithm geneticAlgorithm,
      FitnessEventHandler fitnessEventHandler)
    {
      _fitnessEventHandler = fitnessEventHandler;
      GenericAlgorithm = geneticAlgorithm;
      Chromosomes = chromosomes;
      _rand = new Random();
    }

    /// <summary>
    /// Deep copy constructor from an existing IGeneration.
    /// </summary>
    public Generation(IGeneration generation)
    {
      Chromosomes = new IChromosome[(int)generation.NumberOfChromosomes];
      for (var i = 0; i < Chromosomes.Length; i++)
      {
        Chromosomes[i] = new Chromosome(generation[i]);
      }
      _rand = new Random();
    }

    public IChromosome this[int index]
    {
      get { return Chromosomes[index]; }
      set { Chromosomes[index] = value; }
    }

    public double AverageFitness => Chromosomes.Average(x => x.Fitness);

    public double MaxFitness => Chromosomes.Max(x => x.Fitness);

    public long NumberOfChromosomes => Chromosomes.Length;

    /// <summary>
    /// Selects a parent by picking 10 random indices and returning the Chromosome
    /// at the smallest index (smallest index = highest fitness after sort).
    /// Must be called after EvaluateFitnessOfPopulation.
    /// </summary>
    public IChromosome SelectParent()
    {
      int minIndex = _rand.Next(Chromosomes.Length);
      for (int i = 1; i < 10; i++)
      {
        int idx = _rand.Next(Chromosomes.Length);
        if (idx < minIndex)
          minIndex = idx;
      }
      return Chromosomes[minIndex];
    }

    /// <summary>
    /// Evaluates fitness of all Chromosomes, then sorts descending so index 0 is the fittest.
    /// </summary>
    public void EvaluateFitnessOfPopulation()
    {
      // Each chromosome's fitness is independent — safe to evaluate in parallel
      Parallel.For(0, Chromosomes.Length, i =>
      {
        double fitness = _fitnessEventHandler.Invoke(Chromosomes[i], this);
        ((Chromosome)Chromosomes[i]).Fitness = fitness;
      });
      // Sort ascending then reverse so highest fitness is at index 0
      Array.Sort(Chromosomes);
      Array.Reverse(Chromosomes);
    }
  }
}
