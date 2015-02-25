using System;
using System.Collections.Generic;

namespace PokemonChallenge
{
  public class Pokemon
  {
    public Pokemon(string name)
    {
      Name = name.ToLowerInvariant();
      Length = name.Length;
    }

    public void CalculatePokecode(int[] pokecodeMapping)
    {
      Pokecode = CalculatePokecode(pokecodeMapping, Name);
    }

    public string Name { get; private set; }

    /// <summary>
    /// This is the length of this Pokemon's name.
    /// </summary>
    public int Length { get; private set; }

    /// <summary>
    /// A Pokemon's Pokecode is a 21-bit number indicating the letters that the Pokemon's name contains.
    /// The assignment of letters to bit flags is based on the Pokecode Mapping
    /// </summary>
    public int Pokecode { get; private set; }

    private static int CalculatePokecode(int[] pokecodeMapping, string name)
    {
      int code = 0;

      foreach (char letter in name)
      {
        if (letter < 'a' || letter > 'z') continue;

        var codeForThisLetter = pokecodeMapping[letter - 'a'];
        code |= codeForThisLetter;
      }

      return code;
    }

    public bool ContainsLetterIndex(int letterIndex)
    {
      return (Pokecode & (int) Math.Pow(2, letterIndex)) != 0;
    }
  }
}