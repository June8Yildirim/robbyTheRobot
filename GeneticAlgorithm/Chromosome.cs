using System;

namespace GeneticLibrary
{
  public class Chromosome : IChromosome
  {
    private double _fitness;
    private readonly int _lengthOfGene;

    public Chromosome(int numberOfGenes, int lengthOfGene, int? seed = null)
    {
      _lengthOfGene = lengthOfGene;
      Genes = new int[numberOfGenes];
      var rand = seed.HasValue ? new Random(seed.Value) : new Random();
      for (int i = 0; i < numberOfGenes; i++)
      {
        Genes[i] = rand.Next(lengthOfGene);
      }
      Fitness = 0;
    }

    // Deep copy constructor
    public Chromosome(IChromosome chromosome)
    {
      _lengthOfGene = chromosome is Chromosome c ? c._lengthOfGene : 7;
      Fitness = 0;
      Genes = new int[chromosome.Length];
      for (var i = 0; i < chromosome.Length; i++)
      {
        Genes[i] = chromosome[i];
      }
    }

    public int this[int index]
    {
      get { return Genes[index]; }
      set { Genes[index] = value; }
    }

    public int CompareTo(IChromosome other)
    {
      return Fitness.CompareTo(other.Fitness);
    }

    /// <summary>
    /// Creates two offspring via single-point crossover, then applies mutation to all genes.
    /// </summary>
    public IChromosome[] Reproduce(IChromosome spouse, double mutationProb)
    {
      var rand = new Random();

      // Create offspring as deep copies of each parent
      var child1 = new Chromosome(this);
      var child2 = new Chromosome(spouse);

      // Single-point crossover: swap genes after the crossover point
      int crossoverPoint = rand.Next((int)Length);
      for (int i = crossoverPoint; i < Length; i++)
      {
        child1.Genes[i] = spouse[i];
        child2.Genes[i] = this[i];
      }

      // Apply mutation to all genes of both children
      for (int i = 0; i < Length; i++)
      {
        if (rand.NextDouble() < mutationProb)
          child1.Genes[i] = rand.Next(_lengthOfGene);
        if (rand.NextDouble() < mutationProb)
          child2.Genes[i] = rand.Next(_lengthOfGene);
      }

      return new IChromosome[] { child1, child2 };
    }

    public double Fitness
    {
      get { return _fitness; }
      set { _fitness = value; }
    }

    public int[] Genes { get; }

    // The Length = 243 for Robby
    public long Length { get { return Genes.Length; } }
  }
}
