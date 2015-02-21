using System;
using System.Collections.Generic;

namespace PokemonChallenge
{
  public class Pokemon
  {
    public Pokemon(string name)
    {
      Name = name.ToLowerInvariant();
      Pokecode = CalculatePokecode(Name);
      ShorterSubsets = new List<Pokemon>();
    }

    public string Name { get; private set; }

    /// <summary>
    /// A Pokemon's Pokecode is a 21-bit number indicating the letters that the Pokemon's name contains. 'A' is least significant.
    /// </summary>
    public int Pokecode { get; private set; }

    private static int CalculatePokecode(string name)
    {
      int code = 0;

      foreach (char letter in name)
      {
        if (letter < 'a' || letter > 'z') continue;

        var codeForThisLetter = (int) Math.Pow(2, letter - 'a');
        code |= codeForThisLetter;
      }

      return code;
    }

    public List<Pokemon> ShorterSubsets { get; private set; }

    public void AddShorterSubsetIfApplicable(Pokemon candidate)
    {
      if ((candidate.Pokecode | Pokecode) == Pokecode)
      {
        ShorterSubsets.Add(candidate);
      }
    }

    public bool ContainsLetterIndex(int letterIndex)
    {
      return (Pokecode & (int) Math.Pow(2, letterIndex)) != 0;
    }
  }
}