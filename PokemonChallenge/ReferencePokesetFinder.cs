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
    private bool[][] impossibleSolutionsByLengthByPokecode;

    public List<string> FindPokesets(IEnumerable<string> pokemonList)
    {
      var pokedex = new Pokedex(pokemonList);

      // Note: This hard-codes an upper limit to the possible size of the set, which needs improving really
      const int maxPossibleSetSize = 26;
      impossibleSolutionsByLengthByPokecode = new bool[maxPossibleSetSize + 1][];

      for (int maxPokemon = 1; minimalSolutionsSoFar.Count == 0 && maxPokemon <= maxPossibleSetSize; maxPokemon++)
      {
        int sizeLimit = maxPokemon;

        for (int i = 0; i <= sizeLimit; i++)
        {
          impossibleSolutionsByLengthByPokecode[i] = new bool[TargetPokecode + 1];
        }

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

      if (index >= maxPokemon) return false; // We're not looking for longer Pokemon yet

      if (lengthOfSet + GetMissingLetterCount(pokecodeForSet) > shortestSolution) return false; // We can't possibly complete the problem without too long a solution

      if (impossibleSolutionsByLengthByPokecode[index][pokecodeForSet]) return false; // We've already checked this pokecode

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

    public static int GetMissingLetterCount2(int letter)
    {
      // Invert the relevant bits, so 1 = missing letter
      letter = letter ^ TargetPokecode;

      // See http://stackoverflow.com/questions/109023/how-to-count-the-number-of-set-bits-in-a-32-bit-integer
      letter = letter - ((letter >> 1) & 0x55555555);
      letter = (letter & 0x33333333) + ((letter >> 2) & 0x33333333);
      letter = (letter + (letter >> 4)) & 0x0F0F0F0F;
      letter = letter + (letter >> 8);
      letter = letter + (letter >> 16);
      return letter & 0x0000003F;
    }

    public static int GetMissingLetterCount3(int v)
    {
      // Invert the relevant bits, so 1 = missing letter
      v = v ^ TargetPokecode;

      // Count the 1-bits, as per https://graphics.stanford.edu/~seander/bithacks.html#CountBitsSetParallel
      v = v - ((v >> 1) & 0x55555555);                       // reuse input as temporary
      v = (v & 0x33333333) + ((v >> 2) & 0x33333333);        // temp
      return ((v + (v >> 4) & 0xF0F0F0F) * 0x1010101) >> 24; // count
    }

    static int[] s = { 1, 2, 4, 8, 16 };
    static int[] b = { 0x55555555, 0x33333333, 0x0F0F0F0F, 0x00FF00FF, 0x0000FFFF };

    public static int GetMissingLetterCount4(int v)
    {
      // Invert the relevant bits, so 1 = missing letter
      v = v ^ TargetPokecode;

      // From https://graphics.stanford.edu/~seander/bithacks.html#CountBitsSetParallel

      int c = v - ((v >> 1) & b[0]);
      c = ((c >> s[1]) & b[1]) + (c & b[1]);
      c = ((c >> s[2]) + c) & b[2];
      c = ((c >> s[3]) + c) & b[3];
      c = ((c >> s[4]) + c) & b[4];

      return c;
    }

    public static int GetMissingLetterCount5(int v)
    {
      // Invert the relevant bits, so 1 = missing letter
      v = v ^ TargetPokecode;

      // From https://graphics.stanford.edu/~seander/bithacks.html#CountBitsSetParallel
      // option 3, for at most 32-bit values in v:
      long c = ((v & 0xfff) * 0x1001001001001 & 0x84210842108421) % 0x1f;
      c += (((v & 0xfff000) >> 12) * 0x1001001001001 & 0x84210842108421) % 0x1f;
      c += ((v >> 24) * 0x1001001001001 & 0x84210842108421) % 0x1f;

      return (int)c;
    }

    public static int GetMissingLetterCount(int v)
    {
      // Invert the relevant bits, so 1 = missing letter
      v = v ^ TargetPokecode;

      // From https://graphics.stanford.edu/~seander/bithacks.html#CountBitsSetParallel
      int c; // c accumulates the total bits set in v
      for (c = 0; v != 0; c++)
      {
        v &= v - 1; // clear the least significant bit set
      }

      return c;
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