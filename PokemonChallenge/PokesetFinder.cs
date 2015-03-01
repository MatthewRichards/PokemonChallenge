﻿using System.Collections.Generic;
using System.Threading.Tasks;

namespace PokemonChallenge
{
  public class PokesetFinder
  {
    private const int TargetPokecode = (1 << 26) - 1;
    private const int OneShiftedPastPokecode = 1 << 26;
    private int shortestSolution = int.MaxValue;
    private int smallestSolution = int.MaxValue;
    private readonly List<string> minimalSolutionsSoFar = new List<string>();
    private bool[] impossibleSolutionsByLengthByPokecode;

    public List<string> FindPokesets(string[] pokemonList)
    {
      var pokedex = new Pokedex(pokemonList);

      const int maxPossibleSetSize = 26;

      for (int maxPokemon = 2; minimalSolutionsSoFar.Count == 0 && maxPokemon <= maxPossibleSetSize; maxPokemon++)
      {
        int sizeLimit = maxPokemon;
        impossibleSolutionsByLengthByPokecode = new bool[(sizeLimit << 26) + (TargetPokecode + 1)];

        int numberOfFirstPokemons;
        for (numberOfFirstPokemons = 0; numberOfFirstPokemons < 512; numberOfFirstPokemons++)
        {
          if (pokedex.PokemonByLetter[numberOfFirstPokemons] == null) break;
        }

        Parallel.For(0, numberOfFirstPokemons, firstPokemonIndex =>
        {
          var firstPokemon = pokedex.PokemonByLetter[firstPokemonIndex];
          int firstLetters = firstPokemon.Pokecode;
          int secondLetterCode = 2;
          int secondLetter = 1;

          // Get the next letter we haven't already found
          while ((firstLetters & secondLetterCode) == secondLetterCode)
          {
            secondLetterCode <<= 1;
            secondLetter++;
          }

          int numberOfSecondPokemons;
          for (numberOfSecondPokemons = (secondLetter << 9); numberOfSecondPokemons < (secondLetter << 10); numberOfSecondPokemons++)
          {
            if (pokedex.PokemonByLetter[numberOfSecondPokemons] == null) break;
          }

          Parallel.For(secondLetter << 9, numberOfSecondPokemons, secondPokemonIndex =>
          {
            var secondPokemon = pokedex.PokemonByLetter[secondPokemonIndex];
            var pokeset = new int[sizeLimit];
            pokeset[0] = firstPokemon.Pokecode;
            pokeset[1] = secondPokemon.Pokecode;

            FindCompleteSets(pokedex, secondLetter + 1, secondLetterCode << 1,
              (firstPokemon.Pokecode | secondPokemon.Pokecode) + OneShiftedPastPokecode + OneShiftedPastPokecode,
              firstPokemon.Length + secondPokemon.Length, pokeset,
              2, sizeLimit);
          });
        });
      }

      return minimalSolutionsSoFar;
    }

    private bool FindCompleteSets(Pokedex pokedex, int missingLetter, int pokecodeForMissingLetter, int pokecodeForSetWithIndex,
      int lengthOfSet, int[] set, int index, int maxPokemon)
    {
      if ((pokecodeForSetWithIndex & TargetPokecode) == TargetPokecode)
      {
        // Success! This is a set of pokemon covering the whole alphabet
        ExtractSolutions(pokedex, set, index, lengthOfSet);
        return true;
      }

      if (index >= maxPokemon) return false; // We're not looking for longer Pokemon yet

      if (lengthOfSet + GetMissingLetterCount(pokecodeForSetWithIndex) > shortestSolution) return false; // We can't possibly complete the problem without too long a solution

      if (impossibleSolutionsByLengthByPokecode[pokecodeForSetWithIndex]) return false; // We've already checked this pokecode

      while ((pokecodeForSetWithIndex & pokecodeForMissingLetter) != 0)
      {
        missingLetter++;
        pokecodeForMissingLetter <<= 1;
      }

      int nextIndex = index + 1;
      int pokecodeForSetWithNextIndex = pokecodeForSetWithIndex + OneShiftedPastPokecode;

      Pokemon[] pokemonByLetter = pokedex.PokemonByLetter;
      int trialIndex = missingLetter << 9;
      bool result = false;
      var trialPokemon = pokemonByLetter[trialIndex];

      for (int i = trialIndex; trialPokemon != null; trialPokemon = pokemonByLetter[++trialIndex])
      {
        set[index] = trialPokemon.Pokecode;
        result |= FindCompleteSets(pokedex, missingLetter + 1, pokecodeForMissingLetter << 1,
          pokecodeForSetWithNextIndex | trialPokemon.Pokecode, lengthOfSet + trialPokemon.Length,
          set, nextIndex, maxPokemon);
      }

      if (!result)
      {
        impossibleSolutionsByLengthByPokecode[pokecodeForSetWithIndex] = true;
      }

      return result;
    }

    public static int GetMissingLetterCount(int v)
    {
      // Invert the relevant bits, so 1 = missing letter
      v = (v ^ TargetPokecode) & TargetPokecode;

      // From https://graphics.stanford.edu/~seander/bithacks.html#CountBitsSetParallel
      int c; // c accumulates the total bits set in v
      for (c = 0; v != 0; c++)
      {
        v &= v - 1; // clear the least significant bit set
      }

      return c;
    }

    private void ExtractSolutions(Pokedex pokedex, int[] set, int numberOfPokemon, int lengthOfSet)
    {
      lock (minimalSolutionsSoFar)
      {
        if (numberOfPokemon < smallestSolution || (numberOfPokemon == smallestSolution && lengthOfSet <= shortestSolution))
        {
          var candidate = string.Join(", ", pokedex.GetPokemonInSet(set, numberOfPokemon));

          if (numberOfPokemon < smallestSolution || lengthOfSet < shortestSolution)
          {
            smallestSolution = numberOfPokemon;
            shortestSolution = lengthOfSet;
            minimalSolutionsSoFar.Clear();
          }
          else if (minimalSolutionsSoFar.Contains(candidate))
          {
            return; // Duplicate
          }

          minimalSolutionsSoFar.Add(candidate);
        }
      }
    }
  }
}