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
    private static readonly int TargetPokecode = (int)Math.Pow(2, 26) - 1;
    private static DateTime startTime = DateTime.Now;

    static void Main()
    {
      var pokedex = new Pokedex(@"..\..\..\Pokemon.txt");

      for (int maxPokemon = 2; maxPokemon <= 5; maxPokemon++)
      {
        int sizeLimit = maxPokemon;
        var target = pokedex.GetPokemonByLetter(0).Count;
        var progress = 0;

        Parallel.ForEach(pokedex.GetPokemonByLetter(0), firstPokemon =>
        {
          var pokeset = new int[sizeLimit];
          pokeset[0] = firstPokemon.Pokecode;

          FindCompleteSets(pokedex, 1, 2, firstPokemon.Pokecode, pokeset, 1, sizeLimit);

          Interlocked.Increment(ref progress);
          Console.WriteLine("... " + progress + "/" + target);
        });
        Console.WriteLine("Done looking for pokesets with " + maxPokemon + " pokemon");
      }

      Console.ReadKey();
    }

    private static void FindCompleteSets(Pokedex pokedex, int missingLetter, int pokecodeForMissingLetter, int pokecodeForSet, int[] set, int index, int maxPokemon)
    {
      if (pokecodeForSet == TargetPokecode)
      {
        // Success! This is a set of pokemon covering the whole alphabet
        Console.WriteLine(DateTime.Now.Subtract(startTime).TotalMilliseconds);
        Console.WriteLine(pokedex.GetPokemonDescriptionForSet(set));
        Console.ReadKey();
        return;
      }

      if (index >= maxPokemon)
      {
        // Failed! We don't want to look for bigger sets than this
        return;
      }

      while ((pokecodeForSet & pokecodeForMissingLetter) != 0)
      {
        missingLetter++;
        pokecodeForMissingLetter <<= 1;
      }

      int nextIndex = index + 1;

      foreach (var trialPokemon in pokedex.GetPokemonByLetter(missingLetter))
      {
        set[index] = trialPokemon.Pokecode;
        FindCompleteSets(pokedex, missingLetter + 1, pokecodeForMissingLetter << 1, pokecodeForSet | trialPokemon.Pokecode, set, nextIndex, maxPokemon);
      }
    }
  }
}
