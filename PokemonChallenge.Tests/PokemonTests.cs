using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace PokemonChallenge.Tests
{
  public class PokemonTests
  {
    [Test]
    [TestCase("a", "1")]
    [TestCase("b", "10")]
    [TestCase("ab", "11")]
    [TestCase("z", "10000000000000000000000000")]
    [TestCase("a-b", "11")]
    [TestCase("C", "100")]
    [TestCase("-", "0")]
    public void Pokecode_UsesCorrectBits(string name, string expectedCodeInBinary)
    {
      var pokemon = new Pokemon(name);

      var expectedCode = Convert.ToInt32(expectedCodeInBinary, 2);
      Assert.AreEqual(expectedCode, pokemon.Pokecode);
    }

    [Test]
    [TestCase("ab", 1, true)]
    [TestCase("ab", 3, false)]
    public void ContainsLetterIndex_ReturnsCorrectAnswer(string name, int letterIndex, bool expectedResult)
    {
      var pokemon = new Pokemon(name);

      Assert.AreEqual(expectedResult, pokemon.ContainsLetterIndex(letterIndex));
    }

    [Test]
    [TestCase("abc", "b", true)]
    [TestCase("abc", "d", false)]
    [TestCase("ab", "ba", true)]
    [TestCase("ba", "ab", false)]
    [TestCase("aaa", "a", false)]
    [TestCase("a", "aaa", true)]
    public void AddShorterSubset_AddsCorrectPokemon(string name, string candidate, bool shouldBeAdded)
    {
      var originalPokemon = new Pokemon(name);
      var candidatePokemon = new Pokemon(candidate);

      originalPokemon.AddShorterSubsetIfApplicable(candidatePokemon);

      Assert.AreEqual(shouldBeAdded, originalPokemon.ShorterSubsets.Contains(candidatePokemon));
    }
  }
}
