using System.IO;
using System.Threading;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonChallenge
{
  public class Program
  {
    private static readonly int TargetPokecode = (int)Math.Pow(2, 26) - 1;
    private static readonly DateTime StartTime = DateTime.Now;
    private static int shortestSolution = int.MaxValue;
    private static readonly List<List<string>> MinimalSolutionsSoFar = new List<List<string>>();
    private static readonly int[] MissingLetterCounts = GetMissingLetterCounts();

    static void Main()
    {
      var pokedex = new Pokedex(@"..\..\..\Pokemon.txt");

      for (int maxPokemon = 2; maxPokemon <= 5; maxPokemon++)
      {
        int sizeLimit = maxPokemon;
        var target = pokedex.GetPokemonByLetter(0).Count;
        var progress = 0;

        Parallel.ForEach(pokedex.GetPokemonByLetter(0), firstPokemon =>
        {
          var pokeset = new int[sizeLimit];
          pokeset[0] = firstPokemon.Pokecode;

          FindCompleteSets(pokedex, 1, 2, firstPokemon.Pokecode, firstPokemon.LowestPossibleLength, pokeset, 1, sizeLimit);

          Interlocked.Increment(ref progress);
          Console.WriteLine("... " + progress + "/" + target);
        });
        Console.WriteLine("Done looking for pokesets with " + maxPokemon + " pokemon after " +
                          DateTime.Now.Subtract(StartTime).TotalMilliseconds + "ms");
      }

      Console.ReadKey();
    }

    private static void FindCompleteSets(Pokedex pokedex, int missingLetter, int pokecodeForMissingLetter, int pokecodeForSet, int lengthOfSet, int[] set, int index, int maxPokemon)
    {
      if (pokecodeForSet == TargetPokecode)
      {
        // Success! This is a set of pokemon covering the whole alphabet
        ExtractSolutions(pokedex, set);
        return;
      }

      var missingLetterCount = MissingLetterCounts[pokecodeForSet];

      if (index >= maxPokemon || lengthOfSet + missingLetterCount > shortestSolution)
      {
        // Failed! We don't want to look for bigger sets than this
        return;
      }

      while ((pokecodeForSet & pokecodeForMissingLetter) != 0)
      {
        missingLetter++;
        pokecodeForMissingLetter <<= 1;
      }

      int nextIndex = index + 1;

      foreach (var trialPokemon in pokedex.GetPokemonByLetter(missingLetter))
      {
        set[index] = trialPokemon.Pokecode;
        FindCompleteSets(pokedex, missingLetter + 1, pokecodeForMissingLetter << 1,
          pokecodeForSet | trialPokemon.Pokecode, lengthOfSet + trialPokemon.LowestPossibleLength, 
          set, nextIndex, maxPokemon);
      }
    }

    private static int[] GetMissingLetterCounts()
    {
      var missingLetterCounts = new int[TargetPokecode + 1];
      missingLetterCounts[0] = 26;

      for (int i = 1; i <= TargetPokecode; i++)
      {
        missingLetterCounts[i] = missingLetterCounts[i >> 1] - (i & 1);
      }

      return missingLetterCounts;
    }

    public static int GetMissingLetterCount(int pokecodeForSet)
    {
      return MissingLetterCounts[pokecodeForSet];
    }

    private static void ExtractSolutions(Pokedex pokedex, int[] set)
    {
      var candidates = pokedex.GetCompletePokesets(set);

      lock (MinimalSolutionsSoFar)
      {
        foreach (var candidate in candidates)
        {
          var length = candidate.Sum(name => name.Length);

          if (length < shortestSolution)
          {
            shortestSolution = length;
            MinimalSolutionsSoFar.Clear();
            MinimalSolutionsSoFar.Add(candidate);

            Console.WriteLine("New shortest solution: {" + string.Join(", ", candidate) + "}");
          }
          else if (length == shortestSolution)
          {
            MinimalSolutionsSoFar.Add(candidate);

            Console.WriteLine("Joint shortest solution: {" + string.Join(", ", candidate) + "}");
          }
        }
      }
    }
  }
}
