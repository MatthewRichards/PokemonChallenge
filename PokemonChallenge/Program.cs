using System;
using System.Diagnostics;
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

      GCSettings.LatencyMode = GCLatencyMode.LowLatency;

      new PokesetFinder().FindPokesets(new[] { "abcdefghijklmnopqrstuvwxyz" });

      GC.Collect();

      long totalTime = 0;
      long referenceTime = 0;
      long maxTime = 0;
      long minTime = long.MaxValue;
      const int iterations = 50;

      for (int i = 0; i < iterations; i++)
      {
        GC.Collect();

        var stopwatch = new Stopwatch();
        stopwatch.Start();

        var solutions = new PokesetFinder().FindPokesets(pokemonList);

        stopwatch.Stop();
        var duration = stopwatch.ElapsedMilliseconds;

        Console.WriteLine("Solutions found in {0} ms: {1}", duration, string.Join("; ", solutions));

        totalTime += duration;

        if (duration > maxTime) maxTime = duration;
        if (duration < minTime) minTime = duration;

        GC.Collect();

        stopwatch.Restart();

        new ReferencePokesetFinder().FindPokesets(pokemonList);

        stopwatch.Stop();
        duration = stopwatch.ElapsedMilliseconds;

        referenceTime += duration;
      }

      Console.WriteLine("Iterations: {0}", iterations);
      Console.WriteLine("Max time: {0}ms", maxTime);
      Console.WriteLine("Min time: {0}ms", minTime);
      Console.WriteLine("Average time: {0}ms", totalTime/iterations);
      Console.WriteLine("Reference implementation: {0}ms", referenceTime/iterations);
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
