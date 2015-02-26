using System;
using System.Collections.Generic;
using System.Linq;

namespace PokemonChallenge
{
  public class Pokedex
  {
    private static readonly int TargetPokecode = (int)Math.Pow(2, 26) - 1;
    private readonly List<string>[] pokemonByPokecode = new List<string>[TargetPokecode + 1];

    public readonly Pokemon[][] PokemonByLetter;

    public Pokedex(IEnumerable<string> pokemonList)
    {
      var allPokemon = pokemonList.Select(pokemon => new Pokemon(pokemon)).OrderByDescending(pokemon => pokemon.Length).ToList();
      var pokecodeMapping = CalculatePokecodeMapping(allPokemon);
      
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
    private int[] CalculatePokecodeMapping(IEnumerable<Pokemon> allPokemon)
    {
      var letterFrequencies =
        allPokemon.SelectMany(pokemon => pokemon.Name.ToCharArray().Where(letter => letter >= 'a' && letter <= 'z'))
          .GroupBy(self => self)
          .OrderBy(group => group.Count());

      var mapping = new int[26];
      var bitFlag = 1;

      foreach (var letterFrequency in letterFrequencies)
      {
        mapping[letterFrequency.Key - 'a'] = bitFlag;
        bitFlag *= 2;
      }

      return mapping;
    }

    private void AddPokemonByPokecode(int pokecode, Pokemon pokemon)
    {
      if (pokemonByPokecode[pokecode] == null || pokemonByPokecode[pokecode][0].Length > pokemon.Length)
      {
        pokemonByPokecode[pokecode] = new List<string> {pokemon.Name};
      }
      else
      {
        pokemonByPokecode[pokecode].Add(pokemon.Name);
      }
    }

    private Pokemon[][] GetPokemonByLetter(IEnumerable<Pokemon> interestingPokemon)
    {
      return Enumerable.Range(0, 26)
        .Select(index => interestingPokemon.Where(pokemon => pokemon.ContainsLetterIndex(index)).ToArray())
        .ToArray();
    }

    public IEnumerable<string> GetPokemonInSet(int[] set, int numberOfPokemon)
    {
      return set.Take(numberOfPokemon).Select(pokecode => string.Join(" or ", pokemonByPokecode[pokecode])).OrderBy(self => self);
    }
  }
}