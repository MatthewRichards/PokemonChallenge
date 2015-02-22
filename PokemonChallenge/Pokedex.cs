using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace PokemonChallenge
{
  public class Pokedex
  {
    private static readonly int TargetPokecode = (int)Math.Pow(2, 26) - 1;
    private readonly List<string>[] pokemonByPokecode = new List<string>[TargetPokecode + 1];

    public readonly Pokemon[][] PokemonByLetter;

    public Pokedex(string pokemonList)
    {
      List<Pokemon> allPokemon = LoadPokedex(pokemonList);
      
      foreach (var pokemon in allPokemon)
      {
        AddPokemonByPokecode(pokemon.Pokecode, pokemon);
      }

      PokemonByLetter = GetPokemonByLetter(allPokemon);
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

    private static List<Pokemon> LoadPokedex(string pokemonList)
    {
      var pokedex = new List<Pokemon>();
      dynamic json = JsonConvert.DeserializeObject(pokemonList);

      foreach (var pokemon in json)
      {
        pokedex.Add(new Pokemon(pokemon.Value));
      }

      return pokedex;
    }

    private Pokemon[][] GetPokemonByLetter(IEnumerable<Pokemon> interestingPokemon)
    {
      return Enumerable.Range(0, 26)
        .Select(index => interestingPokemon.Where(pokemon => pokemon.ContainsLetterIndex(index)).ToArray())
        .ToArray();
    }

    public IEnumerable<string> GetPokemonInSet(int[] set)
    {
      return set.Select(pokecode => string.Join(" or ", pokemonByPokecode[pokecode])).OrderBy(self => self);
    }
  }
}