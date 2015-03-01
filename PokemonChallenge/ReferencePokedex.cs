using System;
using System.Collections.Generic;
using System.Linq;

namespace PokemonChallenge.Reference
{
  public class Pokedex
  {
    private readonly string[] allPokemonNames;

    /// <summary>
    /// A list of Pokemon indexed by letter. In more detail:
    /// * The first 512 slots are the Pokemon containing the "first" letter (as defined by CalculatePokecodeMapping)
    /// * The next 512 slots are for the second letter, etc.
    /// * There is therefore a limit of 512 Pokemon per letter of the alphabet - this is hard-coded
    /// * Each Pokemon is represented as:
    /// ** Pokecode (lower 32 bits, although only 26 are used)
    /// ** Pokemon ID (next 16 bits)
    /// ** Length (top 16 bits)
    /// </summary>
    public readonly long[] PokecodesByLetter;

    public Pokedex(string[] pokemonList)
    {
      allPokemonNames = pokemonList;
      var pokecodeMapping = CalculatePokecodeMapping(pokemonList);

      var pokecodes = new long[pokemonList.Length];
      for (long i = 0; i < pokemonList.Length; i++)
      {
        pokecodes[i] = CalculatePokecode(pokecodeMapping, pokemonList[i], i);
      }

      PokecodesByLetter = GetPokecodesByLetter(pokecodes);
    }

    /// <summary>
    /// Calculate the mapping from letters of the alphabet to bit flags.
    /// We want the rarest letters to come first, because this will help us find a solution sooner.
    /// </summary>
    private int[] CalculatePokecodeMapping(string[] namesOfPokemon)
    {
      var letterCounts = new int[26];

      foreach (var pokemon in namesOfPokemon)
      {
        foreach (var letter in pokemon)
        {
          if (letter >= 'a' && letter <= 'z')
          {
            letterCounts[letter - 'a']++;
          }
        }
      }

      var letters = new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 };
      Array.Sort(letterCounts, letters);

      var mapping = new int[26];
      var bitFlag = 1;

      foreach (var letter in letters)
      {
        mapping[letter] = bitFlag;
        bitFlag <<= 1;
      }

      return mapping;
    }

    private static long CalculatePokecode(int[] pokecodeMapping, string name, long index)
    {
      long code = 0;

      foreach (char letter in name)
      {
        if (letter < 'a' || letter > 'z') continue;

        var codeForThisLetter = pokecodeMapping[letter - 'a'];
        code |= codeForThisLetter;
      }

      code |= (long)name.Length << 48 | index << 32;

      return code;
    }

    private long[] GetPokecodesByLetter(long[] pokecodeList)
    {
      var ret = new long[26 << 9];

      for (int letter = 0, letterCode = 1; letter < 26; letter++, letterCode <<= 1)
      {
        int resultIndex = letter << 9;

        for (long pokemonIndex = 0; pokemonIndex < pokecodeList.Length; pokemonIndex++)
        {
          var pokecode = pokecodeList[pokemonIndex];

          if ((pokecode & letterCode) != 0)
          {
            ret[resultIndex++] = pokecode;
          }
        }

        if (resultIndex >= (letter << 9) + 512)
        {
          throw new InvalidOperationException(
            "More than 512 Pokemon with a single letter - this algorithm can't cope with that!");
        }
      }

      return ret;
    }

    public string[] GetPokemonInSet(long[] set, int numberOfPokemon)
    {
      var sortedSet = (long[])set.Clone();
      Array.Sort(sortedSet);

      var ret = new string[numberOfPokemon];

      for (int i = 0; i < numberOfPokemon; i++)
      {
        ret[i] = allPokemonNames[set[i]];
      }

      return ret;
    }
  }
}