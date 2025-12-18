//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using System;
using System.Collections.Generic;
using System.Linq;

namespace Sintef.Scoop.Utilities
{
	/// <summary>
	/// A simple roulette wheel class, that you can spin to
	/// get a random element based on the probability of selecting each element.
	/// Randomness can be made semi-random by using the fixed seed setting for the RandomCreator.GlobalSeedUsageForRandomCreation
	/// flag.
	/// The generic argument T is the type of elements.
	/// One may specify minimum selection probabilities per element, the default of this is zero.
	/// Note that probabilities are normalised automatically. This normalisation makes sure that
	/// all probabilities sum up to 1. This means that if there is, e.g. only one element with
	/// zero selection probability (and zero minimum selection probability), the normalized selection
	/// probability for that element will be 1 (sampling will therefore always return something, as long as
	/// at least one element has been added).
	/// </summary>
	public class RouletteWheel<T>
	{
		private readonly Dictionary<T, double> _minimumProbabilities;
		private readonly Dictionary<T, double> _elementValues;
		Dictionary<T, double> _normalisedProbabilities;
		private readonly Random _rand;

		/// <summary>
		/// Constructor
		/// </summary>
		public RouletteWheel()
		{
			_elementValues = new Dictionary<T, double>();
			_minimumProbabilities = new Dictionary<T, double>();
			_normalisedProbabilities = new Dictionary<T, double>();
			_rand = RandomCreator.GetRandomGenerator();
		}

		/// <summary>
		/// Constructor
		/// </summary>
		public RouletteWheel(Random random)
		{
			_elementValues = new Dictionary<T, double>();
			_minimumProbabilities = new Dictionary<T, double>();
			_normalisedProbabilities = new Dictionary<T, double>();
			_rand = random;
		}

		/// <summary>
		/// Copy constructor. The random generator is not copied, but created anew.
		/// </summary>
		/// <param name="rouletteWheel"></param>
		public RouletteWheel(RouletteWheel<T> rouletteWheel)
		{
			_elementValues = new Dictionary<T, double>(rouletteWheel._elementValues);
			_minimumProbabilities = new Dictionary<T, double>(rouletteWheel._minimumProbabilities);
			_normalisedProbabilities = new Dictionary<T, double>(rouletteWheel._normalisedProbabilities);
			_rand = RandomCreator.GetRandomGenerator();
		}

		/// <summary>
		/// Adds an element, with an associated probability-related value, to the collection.
		/// If the element already existed, its probability is overwritten but the minimum probability is not changed.
		/// </summary>
		/// <param name="el"></param>
		/// <param name="value">The initial value for the element. This will be normalised to a probability in such a way that the sum of probabilities
		/// for all elements will be 1. The resulting probability can be extracted by calling 'GetProbability(el)'.</param>
		/// <param name="minProb">Optional minimum selection probability (in [0,1]). The default value is zero. If the sum of minimum probabilities for all elements 
		/// exceed 1, an exception will be thrown.</param>
		public void AddElement(T el, double value, double minProb = 0)
		{
			if (minProb < 0 || minProb > 1)
				throw new Exception("RouletteWheel.AddElement: the minimum selection probability must be in [0,1]");
			if (!_elementValues.ContainsKey(el))
			{
				if (_minimumProbabilities.Values.Sum() > 1)
					throw new Exception("RouletteWheel.AddElement: Minimum selection probabilities sum to more than 1.");
			}
			_minimumProbabilities[el] = minProb;
			_elementValues[el] = value;
			NormaliseProbabilities();
		}

		/// <summary>
		/// Normalises all probabilities based on values and minimum probabilities,
		/// so that the sum of all probabilitites equals one. This holds even 
		/// when all probabilities, and minimum probabilities, are originally zero.
		/// </summary>
		private void NormaliseProbabilities()
		{
			if (_elementValues.Any())
			{

				Dictionary<T, double> probs = new(_elementValues);
				double sumProb = probs.Values.Sum();

				//Normalise
				if (sumProb != 0)
					probs = probs.ToDictionary(kvp => kvp.Key, kvp => kvp.Value / sumProb);

					//Special case, when all values and all minimum probabilities are zero. 
				else if (probs.Count > 0 && probs.All(kvp => _minimumProbabilities[kvp.Key] == 0))
				{
					double frac = 1 / ((double)probs.Count);
					probs.Do(kvp => _normalisedProbabilities[kvp.Key] = frac);
					return;
				}

				//add minimum probabilities
				double sumOfMinProbs = 0;
				double sumOfProbsLargerThanMin = 0;
				Dictionary<T, double> probDeltas = new();
				foreach (KeyValuePair<T, double> kvp in probs)
				{
					if (kvp.Value <= _minimumProbabilities[kvp.Key])
					{
						probDeltas[kvp.Key] = 0;
						sumOfMinProbs += _minimumProbabilities[kvp.Key];
					}
					else
					{
						sumOfProbsLargerThanMin += kvp.Value;
						probDeltas[kvp.Key] = kvp.Value;// -_minimumProbabilities[kvp.Key];
					}
				}


				//Normalise the deltas and create the final probabilities
				if (sumOfMinProbs > 0)
				{
					if (sumOfProbsLargerThanMin > 0)
					{
						probDeltas = probDeltas.ToDictionary(kvp => kvp.Key, kvp => kvp.Value * (1 - sumOfMinProbs) / sumOfProbsLargerThanMin);
						_normalisedProbabilities = probDeltas.Join(_minimumProbabilities, k => k.Key, k => k.Key, (kd, km) => new KeyValuePair<T, double>(kd.Key, (kd.Value > 0) ? kd.Value : km.Value)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
					}
					else
						_normalisedProbabilities = probs.ToDictionary(kvp => kvp.Key, kvp => _minimumProbabilities[kvp.Key] / sumOfMinProbs);
				}
				else
					_normalisedProbabilities = probs;
			}
		}

		/// <summary>
		/// Removes the given element from the collection, if it exists.
		/// </summary>
		/// <param name="el"></param>
		public void RemoveElement(T el)
		{
			_elementValues.Remove(el);
			_minimumProbabilities.Remove(el);
			_normalisedProbabilities.Remove(el);
			NormaliseProbabilities();
		}

		/// <summary>
		/// Returns the probability (in [0, 1]) associated with the given element.
		/// Returns 0 if that element is not in the collection.
		/// </summary>
		/// <param name="el"></param>
		/// <returns></returns>
		public double GetProbability(T el)
		{
			if (_normalisedProbabilities.ContainsKey(el))
				return _normalisedProbabilities[el];
			else
				return 0;
		}

		/// <summary>
		/// Draws a random element, based on the defined probabilities.
		/// </summary>
		/// <param name="where">Optional predicate. If not null, the drawing will be only amongst those elements that satisfy the predicate.</param>
		/// <returns>Returns the selected element, or default(T) if there are no elements with non-zero probability.</returns>
		public T Draw(Func<KeyValuePair<T, double>, bool> where = null)
		{
			Dictionary<T, double> temp = where == null ? _normalisedProbabilities : _normalisedProbabilities.Where(where).ToDictionary(x => x.Key, x => x.Value);
			return Draw(temp);
		}

		/// <summary>
		/// Draws a random element, based on the given probabilities. These must sum to something less than one.
		/// If all probabilities are zero, draws from a uniform distribution.
		/// </summary>
		/// <returns>Returns the selected element, or default(T) if there are no elements with non-zero probability.</returns>
		public T Draw(Dictionary<T, double> elementProbabilities, bool dreadfullyVerboseForDebugging = false)
		{
			if (elementProbabilities.Count == 0)
				return default;

			Dictionary<T, double> probs = new(elementProbabilities);
			double sumProb = probs.Values.Sum();

			if (sumProb == 0)
			{
				int pcnt = _rand.Next(0, (int)probs.Count);
				if(dreadfullyVerboseForDebugging)
					Console.WriteLine("Draw: sumProb = 0, selected element index " + pcnt);
				return probs.ElementAt(pcnt).Key;
			}
			else
			{
				//Scale probabilities to be able to use Rand:
				double pcnt = _rand.NextDouble()*sumProb;// Next(0, (int)scalingFactor + 1);
				double sum = 0.0;
				foreach (KeyValuePair<T, double> elm in probs)
				{
					sum += elm.Value;
					if (sum.GreaterOrEqualWithTolerance(pcnt, 0.0000000001))
					{
						if (dreadfullyVerboseForDebugging)
							Console.WriteLine("Draw: sumProb = "+ sumProb +"Random number " + pcnt + " gave sum " + sum + ", => Selected element " + elm.Key);
						return elm.Key;
					}
				}
				return default;
			}
		}



		/// <summary>
		/// Returns all elements, in an order chosen by roulette sampling.
		/// </summary>
		/// <param name="where">Optional predicate. If not null, the returned sequence will only contain those elements that satisfy the predicate.</param>
		/// <param name="dreadfullyVerboseForDebugging"></param>
		/// <returns>Returns the sequence in an order that is chosen by roulette sampling.</returns>
		public List<T> DrawSequence(Func<KeyValuePair<T, double>, bool> where = null, bool dreadfullyVerboseForDebugging = false)
		{
			Dictionary<T, double> temp = where == null ? _normalisedProbabilities : _normalisedProbabilities.Where(where).ToDictionary(x => x.Key, x => x.Value);
			Dictionary<T, double> rem = new(temp);
			return DrawSequence(rem, dreadfullyVerboseForDebugging);
		}

		/// <summary>
		/// Returns the given elements, in an order chosen by roulette sampling.
		/// </summary>
		/// <returns></returns>
		private List<T> DrawSequence(Dictionary<T, double> rem, bool dreadfullyVerboseForDebugging = false)
		{
			if(dreadfullyVerboseForDebugging)
			{
				Console.WriteLine("DrawSequence from probs:");
				rem.Do(kvp => Console.WriteLine("Unit " + kvp.Key.ToString() + ", prob = " + kvp.Value));
				Console.WriteLine();
			}

			List<T> result = new();
			while (rem.Count > 0)
			{
				T el = Draw(rem, dreadfullyVerboseForDebugging);
				result.Add(el);
				rem.Remove(el);
			}
			return result;
		}

		/// <summary>
		/// All elements
		/// </summary>
		public IEnumerable<T> Elements { get { return _elementValues.Keys; } }

		/// <summary>
		/// Returns the most probable element (or one of them, if there are several with
		/// the same probability.
		/// </summary>
		/// <returns></returns>
		public T GetMostProbable()
		{
			return _elementValues.MaxBy(kvp => kvp.Value).Key;
		}

		/// <summary>
		/// Returns the least probable element (or one of them, if there are several with
		/// the same probability.
		/// </summary>
		/// <returns></returns>
		public T GetLeastProbable()
		{
			return _elementValues.MinBy(kvp => kvp.Value).Key;
		}
	}
}
