using System;
using System.IO;
using System.Runtime;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace PokemonChallenge
{
  public class Program
  {
    static void Main()
    {
      DateTime startRead = DateTime.Now;
      string[] pokemonList = ReadPokemon();
      Console.WriteLine("Read Pokemon into string array in " + (DateTime.Now - startRead).TotalMilliseconds + "ms");

      GCSettings.LatencyMode = GCLatencyMode.LowLatency;

      for (int i = 0; i < 1; i++)
      {
        GC.Collect();
        new PokesetFinder().FindPokesets(pokemonList);
      }
    }

    private static string[] ReadPokemon()
    {
      var pokelist = new List<string>();
      dynamic json = JsonConvert.DeserializeObject(File.ReadAllText(@"..\..\..\Pokemon.txt"));

      foreach (var pokemon in json)
      {
        pokelist.Add(pokemon.Value);
      }

      return pokelist.ToArray();
    }


  }
}
