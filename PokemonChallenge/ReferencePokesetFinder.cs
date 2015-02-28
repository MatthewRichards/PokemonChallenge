using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;

namespace PokemonChallenge
{
  public class ReferencePokesetFinder
  {
    private const int TargetPokecode = 67108863; //(int)Math.Pow(2, 26) - 1;
    private int shortestSolution = int.MaxValue;
    private int smallestSolution = int.MaxValue;
    private readonly List<string> minimalSolutionsSoFar = new List<string>();
    private int[][] impossibleSolutionsByLengthByPokecode;

    public List<string> FindPokesets(IEnumerable<string> pokemonList)
    {
      var pokedex = new Pokedex(pokemonList);

      // Note: This hard-codes an upper limit to the possible size of the set, which needs improving really
      const int maxPossibleSetSize = 20;
      impossibleSolutionsByLengthByPokecode = new int[maxPossibleSetSize + 1][];
      for (int i = 0; i <= maxPossibleSetSize; i++)
      {
        impossibleSolutionsByLengthByPokecode[i] = new int[TargetPokecode + 1];
      }

      for (int maxPokemon = 1; minimalSolutionsSoFar.Count == 0 && maxPokemon <= maxPossibleSetSize; maxPokemon++)
      {
        int sizeLimit = maxPokemon;

        Parallel.ForEach(pokedex.PokemonByLetter[0], firstPokemon =>
        {
          var pokeset = new int[sizeLimit];
          pokeset[0] = firstPokemon.Pokecode;

          FindCompleteSets(pokedex, 1, 2, firstPokemon.Pokecode, firstPokemon.Length, pokeset, 1, sizeLimit);
        });
      }

      return minimalSolutionsSoFar;
    }

    private bool FindCompleteSets(Pokedex pokedex, int missingLetter, int pokecodeForMissingLetter, int pokecodeForSet,
      int lengthOfSet, int[] set, int index, int maxPokemon)
    {
      if (pokecodeForSet == TargetPokecode)
      {
        // Success! This is a set of pokemon covering the whole alphabet
        ExtractSolutions(pokedex, set, index, lengthOfSet);
        return true;
      }

      if (index > smallestSolution || index >= maxPokemon ||
          impossibleSolutionsByLengthByPokecode[index][pokecodeForSet] >= maxPokemon)
      {
        // Failed! We don't want to look for bigger sets than this
        return false;
      }

      var missingLetterCount = GetMissingLetterCount(pokecodeForSet);

      if (lengthOfSet > shortestSolution - missingLetterCount)
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
        impossibleSolutionsByLengthByPokecode[index][pokecodeForSet] = maxPokemon;
      }

      return result;
    }

    public static int GetMissingLetterCount2(int letter)
    {
      // See http://stackoverflow.com/questions/109023/how-to-count-the-number-of-set-bits-in-a-32-bit-integer
      letter = letter - ((letter >> 1) & 0x55555555);
      letter = (letter & 0x33333333) + ((letter >> 2) & 0x33333333);
      letter = (letter + (letter >> 4)) & 0x0F0F0F0F;
      letter = letter + (letter >> 8);
      letter = letter + (letter >> 16);
      return letter & 0x0000003F;
    }

    public static int GetMissingLetterCount(int v)
    {
      // Invert the relevant bits, so 1 = missing letter
      v = v ^ TargetPokecode;

      // Count the 1-bits, as per https://graphics.stanford.edu/~seander/bithacks.html#CountBitsSetParallel
      v = v - ((v >> 1) & 0x55555555);                    // reuse input as temporary
      v = (v & 0x33333333) + ((v >> 2) & 0x33333333);     // temp
      return ((v + (v >> 4) & 0xF0F0F0F) * 0x1010101) >> 24; // count
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