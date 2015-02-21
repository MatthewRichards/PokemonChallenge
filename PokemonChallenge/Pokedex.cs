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

      foreach (var pokemon in allPokemon)
      {
        allPokemon.Except(new[] {pokemon}).ToList().ForEach(candidate => pokemon.AddShorterSubsetIfApplicable(candidate));
      }

      var interestingPokemon = allPokemon.Where(
        pokemon => !allPokemon.Any(biggerPokemon => biggerPokemon.ShorterSubsets.Contains(pokemon)));
      pokemonByLetter = GetPokemonByLetter(interestingPokemon);
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

    private List<Pokemon>[] GetPokemonByLetter(IEnumerable<Pokemon> interestingPokemon)
    {
      return Enumerable.Range(0, 26)
        .Select(index => interestingPokemon.Where(pokemon => pokemon.ContainsLetterIndex(index)).ToList())
        .ToArray();
    }

    public List<Pokemon> GetPokemonByLetter(int letterIndex)
    {
      return pokemonByLetter[letterIndex];
    }

    /// <summary>
    /// Given a candidate solution as a list of pokecodes, generate all the actual candidate solutions as lists of pokenames.
    /// There may be several, because one anagrams have the same pokecode, and because we may be able to derive a valid
    /// solution from the "shorter subsets" of any of the Pokemons in this set.
    /// </summary>
    /// <param name="set"></param>
    /// <returns></returns>
    public List<List<string>> GetCompletePokesets(int[] set)
    {
      var candidatesForEachPosition = set.Select(
        pokecode =>
          allPokemon.Where(pokemon => pokemon.Pokecode == pokecode)
            .SelectMany(pokemon => pokemon.ShorterSubsets.Union(new[] {pokemon}))).ToList();

      return GetPokesets(candidatesForEachPosition, new List<Pokemon>())
        .Where(IsCompletePokeset)
        .Select(pokeset => pokeset.Select(pokemon => pokemon.Name).ToList())
        .ToList();
    }

    private IEnumerable<List<Pokemon>> GetPokesets(
      List<IEnumerable<Pokemon>> candidatesForEachPosition,
      List<Pokemon> pokesetSoFar)
    {
      if (!candidatesForEachPosition.Any())
      {
        yield return pokesetSoFar;
      }
      else
      {
        foreach (Pokemon candidateForThisPosition in candidatesForEachPosition.First())
        {
          foreach (
            var result in
              GetPokesets(candidatesForEachPosition.Skip(1).ToList(),
                new List<Pokemon>(pokesetSoFar) {candidateForThisPosition}))
          {
            yield return result;
          }
        }
      }
    }

    private bool IsCompletePokeset(IEnumerable<Pokemon> set)
    {
      int setPokecode = set.Aggregate(0, (current, pokemon) => current | pokemon.Pokecode);
      return setPokecode == (int)Math.Pow(2, 26) - 1;
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