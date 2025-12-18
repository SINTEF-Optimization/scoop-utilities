//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using System;
using System.Collections.Generic;
using System.Linq;

namespace Sintef.Scoop.Utilities
{       /// <summary>
		/// Represents a combination of two elements of type T and S.
		/// If S == T, the order is of no consequence
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="S"></typeparam>
	public class Combination<T, S> : IEquatable<Combination<T, S>> //where  T:IEquatable<T> where S:IEquatable<S>
	{


		#region Properties and fields

		private T _t;

		private S _s;

		#endregion

		#region Construction

		/// <summary>
		/// Constructor
		/// </summary>
		public Combination(T t, S s)
		{
			_t = t;
			_s = s;
		}

		#endregion

		#region Public members

		/// <inheritdoc/>
		public bool Equals(Combination<T, S> other)
		{
			return (_s.Equals(other._s) && _t.Equals(other._t)) ||
				(typeof(S) == typeof(T) && ((_s.Equals(other._t) && _t.Equals(other._s))));
		}

		#endregion

		#region Private members

		#endregion

		/// <summary>
		/// Returns the 'T' element.
		/// </summary>
		/// <returns></returns>
		public T GetTElement()
		{
			return _t;
		}

		/// <summary>
		/// Returns the 'S' element.
		/// </summary>
		/// <returns></returns>
		public S GetSElement()
		{
			return _s;
		}
	}

	/// <summary>
	/// Simple utility to calculate all the combinations of elements from two lists,
	/// one of each of the template types.
	/// </summary>
	public static class CombinationsFromTwoLists<T, S>
	//where T : IEquatable<T>
	//where S : IEquatable<S>
	{
		#region Internal classes

		/// <summary>
		/// Delegate for calculating a double based on a T,S pair.
		/// </summary>
		/// <param name="t"></param>
		/// <param name="s"></param>
		/// <returns></returns>
		public delegate double WeightFunction(T t, S s);


		#endregion
		#region Properties and fields
		#endregion

		#region Construction

		#endregion

		#region Public members


		/// <summary>
		/// Returns all combinations of the two list
		/// </summary>
		/// <param name="listT"></param>
		/// <param name="listS"></param>
		/// <returns>A list of different ways of combining all elements of listT with all elements of listS.</returns>
		public static List<List<Combination<T, S>>> GetAllCombinations(List<T> listT, List<S> listS)
		{
			//Starting recursion.
			return GetAllCombinations(listT, listS, null, 0);
		}

		/// <summary>
		/// Calculates the optimal matching of the entries in the two list.
		/// For the time being, this is based on simple enumeration.
		/// For the time being, we require that the two lists are of equal length.
		/// The returned matching value is in [0,1],
		/// </summary>
		/// <param name="listT"></param>
		/// <param name="listS"></param>
		/// <param name="F">Combination weight function, used to calculate the weights.  Should return a weight in [0,1]</param>
		/// <param name="totalWeightOfReturnedMatching"></param>
		/// <returns></returns>
		public static List<Combination<T, S>> GetOptimalMatching(List<T> listT, List<S> listS, WeightFunction F, out double totalWeightOfReturnedMatching)
		{
			if (listT.Count != listS.Count)
				throw new NotImplementedException("GetOptimalMatching not yet implemented for lists of different lengths.");

			//Match by enumeration

			//Get all combinations, with weights
			List<Dictionary<Combination<T, S>, double>> matchingMatrix = GetAllCombinationsWithWeights(listT, listS, F);

			//Retrieve the best matching
			Dictionary<Combination<T, S>, double> maxScoreMatching = matchingMatrix.MaxBy(m => m.Sum(kvp => kvp.Value));

			//...and its total weight
			totalWeightOfReturnedMatching = maxScoreMatching.Sum(kvp => kvp.Value) / ((double)maxScoreMatching.Count);

			List<Combination<T, S>> maxScoreMatchingAsList = maxScoreMatching.Keys.ToList();

			return maxScoreMatchingAsList;
		}


		/// <summary>
		/// Returns all combinations of the two list, by recursively calling itself.
		/// </summary>
		/// <param name="listT"></param>
		/// <param name="listS"></param>
		/// <param name="F">Combination weight function, used to calculate the weights.  Should return a weight in [0,1]</param>
		/// <returns>A list of ways of combining the two lists. </returns>
		public static List<Dictionary<Combination<T, S>, double>> GetAllCombinationsWithWeights(List<T> listT, List<S> listS, WeightFunction F)
		{
			return GetAllCombinationsWithWeights(listT, listS, F, null, 0);
		}


		/// <summary>
		/// Returns all combinations of the two list, by recursively calling itself.
		/// </summary>
		/// <param name="listT"></param>
		/// <param name="listS"></param>
		/// <param name="level">Leave out or set to 0 for top level call (used in the recursion).</param>
		/// <param name="comboFromAbove">Leave out or set to null for top level call (used in the recursion)</param>
		/// <param name="F">Combination weight function, used to calculate the weights. Should return a weight in [0,1]</param>
		/// <returns>A list of ways of combining the two lists. </returns>
		private static List<Dictionary<Combination<T, S>, double>> GetAllCombinationsWithWeights(List<T> listT, List<S> listS, WeightFunction F, Dictionary<Combination<T, S>, double> comboFromAbove, int level)
		{
			int nd = listT.Count;
			List<S> remainingSToPassOn = (level == 0) ? new List<S>(listS) : listS;

			List<Dictionary<Combination<T, S>, double>> result = new();
			T t = listT[level];
			Dictionary<Combination<T, S>, double> combo = comboFromAbove ?? new Dictionary<Combination<T, S>, double>();

			List<S> myRemainingSs = new(remainingSToPassOn);
			foreach (S sForT in myRemainingSs)
			{
				//if (level == 0)
				double weight = F(t, sForT);

				if (weight < 0.0 || weight > 1.0)
				{
#if DEBUG
					throw new Exception(string.Format("Weight function between combinations {0} and {1} returned {2}, should be between 0 and 1",
						t, sForT, weight));
#else
					weight = Math.Max(0, Math.Min(1, weight));
#endif
				}

				Combination<T, S> combi = new(t, sForT);
				combo[combi] = weight;

				if (listT.Count > level + 1 && listS.Count >= 1)
				{
					remainingSToPassOn.Remove(sForT);
					result.AddRange(GetAllCombinationsWithWeights(listT, remainingSToPassOn, F, combo, level + 1));
					remainingSToPassOn.Add(sForT);
				}
				else
				{
					result.Add(new Dictionary<Combination<T, S>, double>(combo));
				}
				combo.Remove(combi);
			}
			return result;
		}


		/// <summary>
		/// Returns all combinations of the two list, by recursively calling itself.
		/// </summary>
		/// <param name="listT"></param>
		/// <param name="listS"></param>
		/// <param name="comboFromAbove"></param>
		/// <param name="level"></param>
		/// <returns></returns>
		private static List<List<Combination<T, S>>> GetAllCombinations(List<T> listT, List<S> listS, List<Combination<T, S>> comboFromAbove, int level)
		{
			//int nd = listT.Count;
			//int nr = rooms.Count();
			List<S> remainingSToPassOn = (level == 0) ? new List<S>(listS) : listS;

			List<Combination<T, S>> combo = (comboFromAbove == null) ? new List<Combination<T, S>>() : comboFromAbove;

			List<List<Combination<T, S>>> result = new();
			T t = listT[level];
			List<S> myRemainingSs = new(remainingSToPassOn);
			foreach (S sForT in myRemainingSs)
			{
				Combination<T, S> combi = new(t, sForT);
				combo.Add(combi);

				remainingSToPassOn.Remove(sForT);
				if (listT.Count > level + 1 && listS.Count >= 1)
				{
					remainingSToPassOn.Remove(sForT);
					result.AddRange(GetAllCombinations(listT, remainingSToPassOn, combo, level + 1));
					remainingSToPassOn.Add(sForT);
				}
				else
					result.Add(new List<Combination<T, S>>(combo));

				combo.Remove(combi);
			}
			return result;
		}


		#endregion

		#region Private members

		#endregion
	}
}
