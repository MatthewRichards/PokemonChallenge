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
    private readonly List<Pokemon> allPokemon;
    private readonly List<Pokemon>[] pokemonByPokecode = new List<Pokemon>[TargetPokecode + 1];

    public readonly Pokemon[][] PokemonByLetter;

    public Pokedex(string pokemonList)
    {
      allPokemon = LoadPokedex(pokemonList);

      foreach (var pokemon in allPokemon)
      {
        AddPokemonByPokecode(pokemon.Pokecode, pokemon);
        allPokemon.Except(new[] {pokemon}).ToList().ForEach(candidate => pokemon.AddShorterSubsetIfApplicable(candidate));

        foreach (var candidate in pokemon.ShorterSubsets)
        {
          AddPokemonByPokecode(pokemon.Pokecode, candidate);
        }
      }

      var interestingPokemon = allPokemon.Where(
        pokemon => !allPokemon.Any(biggerPokemon => biggerPokemon.ShorterSubsets.Contains(pokemon)));

      PokemonByLetter = GetPokemonByLetter(interestingPokemon);
    }

    private void AddPokemonByPokecode(int pokecode, Pokemon pokemon)
    {
      if (pokemonByPokecode[pokecode] == null)
      {
        pokemonByPokecode[pokecode] = new List<Pokemon>();
      }

      if (!pokemonByPokecode[pokecode].Contains(pokemon))
      {
        pokemonByPokecode[pokecode].Add(pokemon);
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

    /// <summary>
    /// Given a candidate solution as a list of pokecodes, generate all the actual candidate solutions as lists of pokenames.
    /// There may be several, because one anagrams have the same pokecode, and because we may be able to derive a valid
    /// solution from the "shorter subsets" of any of the Pokemons in this set.
    /// </summary>
    /// <param name="set"></param>
    /// <returns></returns>
    public List<List<string>> GetCompletePokesets(int[] set)
    {
      var candidatesForEachPosition = set.Select(pokecode => pokemonByPokecode[pokecode].ToArray()).ToArray();

      var results = new List<List<string>>();
      PopulatePokesets(candidatesForEachPosition, set.Length, 0, new Pokemon[candidatesForEachPosition.Length], results);
      
      return results;
    }

    private void PopulatePokesets(
      Pokemon[][] candidatesForEachPosition,
      int currentPosition,
      int workingPokecode,
      Pokemon[] workingPokeset,
      List<List<string>> results)
    {
      if (currentPosition == 0)
      {
        if (workingPokecode == TargetPokecode)
        {
          results.Add(workingPokeset.Select(pokemon => pokemon.Name).ToList());
        }
      }
      else
      {
        // Performance at the expense of clarity... It's slightly preferable to have currentPosition be 1-based rather
        // than 0-based, hence why we use nextPosition in the array indexing below.
        var nextPosition = currentPosition - 1;

        foreach (Pokemon candidateForThisPosition in candidatesForEachPosition[nextPosition])
        {
          workingPokeset[nextPosition] = candidateForThisPosition;
          int newPokecode = workingPokecode | candidateForThisPosition.Pokecode;

          PopulatePokesets(candidatesForEachPosition,
            nextPosition,
            newPokecode,
            workingPokeset,
            results);
        }
      }
    }
  }
}