using System;
using System.Collections.Generic;
using System.Linq;

namespace PokemonChallenge.Reference
{
  public class Pokedex
  {
    private const int TargetPokecode = (1 << 26) - 1;
    private readonly List<string>[] pokemonByPokecode = new List<string>[TargetPokecode + 1];

    public readonly Pokemon[] PokemonByLetter;

    public Pokedex(string[] pokemonList)
    {
      var allPokemon = pokemonList.Select(pokemon => new Pokemon(pokemon)).ToArray();
      var pokecodeMapping = CalculatePokecodeMapping(pokemonList);

      foreach (var pokemon in allPokemon)
      {
        pokemon.CalculatePokecode(pokecodeMapping);
        AddPokemonByPokecode(pokemon.Pokecode, pokemon);
      }

      PokemonByLetter = GetPokemonByLetter(allPokemon);
    }

    /// <summary>
    /// Calculate the mapping from letters of the alphabet to bit flags.
    /// We want the rarest letters to come first, because this will help us find a solution sooner.
    /// </summary>
    private int[] CalculatePokecodeMapping(string[] allPokemon)
    {
      var letterCounts = new int[26];

      foreach (var pokemon in allPokemon)
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

    private void AddPokemonByPokecode(int pokecode, Pokemon pokemon)
    {
      if (pokemonByPokecode[pokecode] == null || pokemonByPokecode[pokecode][0].Length > pokemon.Length)
      {
        pokemonByPokecode[pokecode] = new List<string> { pokemon.Name };
      }
      else
      {
        pokemonByPokecode[pokecode].Add(pokemon.Name);
      }
    }

    private Pokemon[] GetPokemonByLetter(Pokemon[] interestingPokemon)
    {
      var ret = new Pokemon[26 << 9];

      for (int letter = 0; letter < 26; letter++)
      {
        int i = letter << 9;
        foreach (var pokemon in interestingPokemon)
        {
          if (pokemon.ContainsLetterIndex(letter))
          {
            ret[i++] = pokemon;
          }
        }
      }

      return ret;
    }

    public IEnumerable<string> GetPokemonInSet(int[] set, int numberOfPokemon)
    {
      return set.Take(numberOfPokemon).Select(pokecode => string.Join(" or ", pokemonByPokecode[pokecode])).OrderBy(self => self);
    }
  }
}