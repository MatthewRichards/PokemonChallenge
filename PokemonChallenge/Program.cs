using System;
using System.IO;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace PokemonChallenge
{
  public class Program
  {
    static void Main()
    {
      int startRead = Environment.TickCount;
      string[] pokemonList = ReadPokemon();
      Console.WriteLine("Read Pokemon into string array in " + (Environment.TickCount - startRead) + "ms");

      new PokesetFinder().FindPokesets(new[] {"abcdefghijklmnopqrstuvwxyz"});

      GCSettings.LatencyMode = GCLatencyMode.LowLatency;

      int totalTime = 0;
      int maxTime = 0;
      const int iterations = 10;

      for (int i = 0; i < iterations; i++)
      {
        GC.Collect();
        var time = new PokesetFinder().FindPokesets(pokemonList);
        totalTime += time;

        if (time > maxTime)
        {
          maxTime = time;
        }
      }

      Console.WriteLine("Max time: {0}ms", maxTime);
      Console.WriteLine("Average time: {0}ms", totalTime/iterations);
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
