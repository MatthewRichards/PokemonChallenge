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
    private readonly List<Pokemon>[] pokemonByLetter;
    private readonly List<Pokemon>[] pokemonByPokecode = new List<Pokemon>[TargetPokecode + 1]; 

    public Pokedex(string filename)
    {
      allPokemon = LoadPokedex(filename);

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
      pokemonByLetter = GetPokemonByLetter(interestingPokemon);
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
      var candidatesForEachPosition = set.Select(pokecode => pokemonByPokecode[pokecode]).ToArray();

      var results = new List<List<string>>();
      PopulatePokesets(candidatesForEachPosition, 0, 0, new Pokemon[candidatesForEachPosition.Length], results);
      
      return results;
    }

    private void PopulatePokesets(
      List<Pokemon>[] candidatesForEachPosition,
      int currentPosition,
      int workingPokecode,
      Pokemon[] workingPokeset,
      List<List<string>> results)
    {
      if (currentPosition == workingPokeset.Length)
      {
        if (workingPokecode == TargetPokecode)
        {
          results.Add(workingPokeset.Select(pokemon => pokemon.Name).ToList());
        }
      }
      else
      {
        foreach (Pokemon candidateForThisPosition in candidatesForEachPosition[currentPosition])
        {
          workingPokeset[currentPosition] = candidateForThisPosition;
          int newPokecode = workingPokecode | candidateForThisPosition.Pokecode;

          PopulatePokesets(candidatesForEachPosition,
            currentPosition + 1,
            newPokecode,
            workingPokeset,
            results);
        }
      }
    }
  }
}