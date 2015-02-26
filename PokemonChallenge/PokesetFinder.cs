using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PokemonChallenge
{
  public class PokesetFinder
  {
    private const int TargetPokecode = 67108863; //(int)Math.Pow(2, 26) - 1;
    private int shortestSolution = int.MaxValue;
    private int smallestSolution = int.MaxValue;
    private readonly List<string> minimalSolutionsSoFar = new List<string>();
    private readonly int[] missingLetterCounts = GetMissingLetterCounts();
    private bool[][] impossibleSolutionsByLengthByPokecode;

    public void FindPokesets(IEnumerable<string> pokemonList)
    {
      var startTime = DateTime.Now;

      var pokedex = new Pokedex(pokemonList);

      for (int maxPokemon = 10; minimalSolutionsSoFar.Count == 0; maxPokemon++)
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

      Console.WriteLine();
      Console.WriteLine("Solutions found in " + DateTime.Now.Subtract(startTime).TotalMilliseconds + "ms:");
      minimalSolutionsSoFar.ForEach(Console.WriteLine);
      Console.WriteLine();
    }

    private bool FindCompleteSets(Pokedex pokedex, int missingLetter, int pokecodeForMissingLetter, int pokecodeForSet, int lengthOfSet, int[] set, int index, int maxPokemon)
    {
      if (pokecodeForSet == TargetPokecode)
      {
        // Success! This is a set of pokemon covering the whole alphabet
        ExtractSolutions(pokedex, set, index, lengthOfSet);
        return true;
      }

      if (index > smallestSolution || index >= maxPokemon || impossibleSolutionsByLengthByPokecode[index][pokecodeForSet])
      {
        // Failed! We don't want to look for bigger sets than this
        return false;
      }

      var missingLetterCount = missingLetterCounts[pokecodeForSet];

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

    public int GetMissingLetterCount(int pokecodeForSet)
    {
      return missingLetterCounts[pokecodeForSet];
    }

    private void ExtractSolutions(Pokedex pokedex, int[] set, int numberOfPokemon, int lengthOfSet)
    {
      lock (minimalSolutionsSoFar)
      {
        if (numberOfPokemon < smallestSolution || (numberOfPokemon == smallestSolution && lengthOfSet <= shortestSolution))
        {
          var candidate = string.Join(", ", pokedex.GetPokemonInSet(set, numberOfPokemon));

          if (numberOfPokemon < smallestSolution || lengthOfSet < shortestSolution)
          {
            smallestSolution = numberOfPokemon;
            shortestSolution = lengthOfSet;
            minimalSolutionsSoFar.Clear();
          }
          else if (minimalSolutionsSoFar.Contains(candidate))
          {
            return; // Duplicate
          }

          minimalSolutionsSoFar.Add(candidate);
        }
      }
    } 
  }
}