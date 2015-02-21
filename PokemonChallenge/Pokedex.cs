using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace PokemonChallenge
{
  public class Pokedex
  {
    private readonly List<Pokemon> allPokemon;
    private readonly List<Pokemon>[] pokemonByLetter; 

    public Pokedex(string filename)
    {
      allPokemon = LoadPokedex(filename);
      pokemonByLetter = GetPokemonByLetter();
    }

    private static List<Pokemon> LoadPokedex(string filename)
    {
      var pokedex = new List<Pokemon>();
      dynamic json = JsonConvert.DeserializeObject(File.ReadAllText(filename));

      foreach (var pokemon in json)
      {
        pokedex.Add(new Pokemon(pokemon.Value));
      }

      return pokedex;
    }

    private List<Pokemon>[] GetPokemonByLetter()
    {
      return Enumerable.Range(0, 25)
        .Select(index => allPokemon.Where(pokemon => pokemon.ContainsLetterIndex(index)).ToList())
        .ToArray();
    }

    public List<Pokemon> GetPokemonByLetter(int letterIndex)
    {
      return pokemonByLetter[letterIndex];
    }

    public string GetPokemonDescriptionForSet(int[] set)
    {
      return "{" +
             string.Join(", ",
               set.Select(pokecode => string.Join(" or ", allPokemon.Where(pokemon => pokemon.Pokecode == pokecode).Select(pokemon => pokemon.Name)))) +
             "}";
    }
  }
}