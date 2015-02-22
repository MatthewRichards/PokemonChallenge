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
    private const int TargetPokecode = 67108863; //(int)Math.Pow(2, 26) - 1;
    private static int shortestSolution = int.MaxValue;
    private static readonly List<string> MinimalSolutionsSoFar = new List<string>();
    private static readonly int[] MissingLetterCounts = GetMissingLetterCounts();
    private static bool[][] impossibleSolutionsByLengthByPokecode;

    static void Main()
    {
      string pokemonList = File.ReadAllText(@"..\..\..\Pokemon.txt");

      var startTime = DateTime.Now;

      var pokedex = new Pokedex(pokemonList);

      for (int maxPokemon = 2; maxPokemon <= 5; maxPokemon++)
      {
        int sizeLimit = maxPokemon;

        impossibleSolutionsByLengthByPokecode = new bool[maxPokemon + 1][];
        for (int i = 0; i <= maxPokemon; i++)
        {
          impossibleSolutionsByLengthByPokecode[i] = new bool[TargetPokecode + 1];
        }

        Parallel.ForEach(pokedex.PokemonByLetter[0], firstPokemon =>
        {
          var pokeset = new int[sizeLimit];
          pokeset[0] = firstPokemon.Pokecode;

          FindCompleteSets(pokedex, 1, 2, firstPokemon.Pokecode, firstPokemon.Length, pokeset, 1, sizeLimit);
        });
        Console.WriteLine("Done looking for pokesets with " + maxPokemon + " pokemon after " +
                          DateTime.Now.Subtract(startTime).TotalMilliseconds + "ms");
      }
    }

    private static bool FindCompleteSets(Pokedex pokedex, int missingLetter, int pokecodeForMissingLetter, int pokecodeForSet, int lengthOfSet, int[] set, int index, int maxPokemon)
    {
      if (pokecodeForSet == TargetPokecode)
      {
        // Success! This is a set of pokemon covering the whole alphabet
        ExtractSolutions(pokedex, set, lengthOfSet);
        return true;
      }

      if (index >= maxPokemon || impossibleSolutionsByLengthByPokecode[index][pokecodeForSet])
      {
        // Failed! We don't want to look for bigger sets than this
        return false;
      }

      var missingLetterCount = MissingLetterCounts[pokecodeForSet];

      if (lengthOfSet + missingLetterCount > shortestSolution)
      {
        // There's no way we can get a solution without ending up with a non-shortest solution
        return false;
      }

      while ((pokecodeForSet & pokecodeForMissingLetter) != 0)
      {
        missingLetter++;
        pokecodeForMissingLetter <<= 1;
      }

      int nextIndex = index + 1;

      Pokemon[] pokemonContainingMissingLetter = pokedex.PokemonByLetter[missingLetter];
      int numberOfTrials = pokemonContainingMissingLetter.Length;
      bool result = false;

      for (int i = 0; i < numberOfTrials; i++)
      {
        var trialPokemon = pokemonContainingMissingLetter[i];
        set[index] = trialPokemon.Pokecode;
        result |= FindCompleteSets(pokedex, missingLetter + 1, pokecodeForMissingLetter << 1,
          pokecodeForSet | trialPokemon.Pokecode, lengthOfSet + trialPokemon.Length, 
          set, nextIndex, maxPokemon);
      }

      if (!result)
      {
        impossibleSolutionsByLengthByPokecode[index][pokecodeForSet] = true;
      }

      return result;
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

    private static void ExtractSolutions(Pokedex pokedex, int[] set, int lengthOfSet)
    {
      if (lengthOfSet <= shortestSolution)
      {
        lock (MinimalSolutionsSoFar)
        {
          var candidate = string.Join(", ", pokedex.GetPokemonInSet(set));
          string description;

          if (lengthOfSet < shortestSolution)
          {
            shortestSolution = lengthOfSet;
            MinimalSolutionsSoFar.Clear();
            description = "New";
          }
          else if (MinimalSolutionsSoFar.Contains(candidate))
          {
            return; // Duplicate
          }
          else
          {
            description = "Joint";
          }

          MinimalSolutionsSoFar.Add(candidate);

          Console.WriteLine(description + " shortest solution: {" + candidate + "}");
        }
      }
    }
  }
}
