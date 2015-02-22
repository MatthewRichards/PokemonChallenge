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
    private readonly Pokemon[] pokemonByPokecode = new Pokemon[TargetPokecode + 1];

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
      if (pokemonByPokecode[pokecode] != null)
      {
        if (pokemonByPokecode[pokecode].Length < pokemon.Length)
        {
          return;
        }
        // qq What about equal length names?
      }

      pokemonByPokecode[pokecode] = pokemon;
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
      return set.Select(pokecode => pokemonByPokecode[pokecode].Name).OrderBy(self => self);
    }
  }
}