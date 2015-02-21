using System.IO;
using System.Threading;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonChallenge
{
  class Program
  {
    static void Main()
    {
      var pokedex = LoadPokedex();
      var pokemonByLetter = Enumerable.Range(0, 25)
        .Select(index => new Tuple<int, List<string>>(index, pokedex.Where(pokemon => pokemon.Contains((char)('a' + index))).ToList()))
        .ToDictionary(tuple => tuple.Item1, pair => pair.Item2);

      for (int maxPokemon = 2;; maxPokemon++)
      {
        int sizeLimit = maxPokemon;
        var target = pokemonByLetter[0].Count;
        var progress = 0;

        Parallel.ForEach(pokemonByLetter[0], firstPokemon =>
        {
          FindCompleteSets(pokemonByLetter, 1,
            new List<string> {firstPokemon}, sizeLimit);

          Interlocked.Increment(ref progress);
          Console.WriteLine("... " + progress + "/" + target);
        });
        Console.WriteLine("Done looking for pokesets with " + maxPokemon + " pokemon");
      }

      Console.ReadKey();
    }

    private static IEnumerable<string> LoadPokedex()
    {
      var pokedex = new List<string>();
      dynamic json = JsonConvert.DeserializeObject(File.ReadAllText(@"..\..\..\Pokemon.txt"));

      foreach (var pokemon in json)
      {
        pokedex.Add(pokemon.Value);
      }

      return pokedex;
    }

    private static void FindCompleteSets(Dictionary<int, List<string>> pokemonByLetter, int missingLetter, List<string> set, int maxPokemon)
    {
      if (missingLetter == 26)
      {
        // Success! This is a set of pokemon covering the whole alphabet
        Console.WriteLine("{" + string.Join(",", set) + "}");
        return;
      }

      if (SetContainsLetter(set, missingLetter))
      {
        FindCompleteSets(pokemonByLetter, missingLetter + 1, set, maxPokemon);
        return;
      }

      if (set.Count >= maxPokemon)
      {
        // Failed! We don't want to look for bigger sets than this
        return;
      }

      foreach (var trialPokemon in pokemonByLetter[missingLetter])
      {
        FindCompleteSets(pokemonByLetter, missingLetter + 1, new List<string>(set) {trialPokemon}, maxPokemon);
      }
    }

    private static bool SetContainsLetter(IEnumerable<string> set, int nextMissingLetter)
    {
      return set.Any(pokemon => pokemon.Contains((char)('a' + nextMissingLetter)));
    }

  }
}
