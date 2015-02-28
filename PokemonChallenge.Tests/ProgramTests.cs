using System;
using System.Linq;
using NUnit.Framework;

namespace PokemonChallenge.Tests
{
  public class ProgramTests
  {
    [Test]
    [TestCase("abcdefghijklmnopqrstuvwxyz", 0)]
    [TestCase("a cdefghi klmnopqrst vwxy ", 4)]
    public void GetMissingLetterCount_IsCorrect(string lettersInSet, int expectedMissingLetters)
    {
      int pokecode = lettersInSet.Aggregate(0, (soFar, letter) => soFar | (int)Math.Pow(2, letter - 'a'));

      int missingLetters = PokesetFinder.GetMissingLetterCount(pokecode);

      Assert.AreEqual(expectedMissingLetters, missingLetters);
    }
  }
}