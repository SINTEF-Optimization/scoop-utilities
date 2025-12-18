//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using System;
using System.Linq;

namespace Sintef.Scoop.Utilities
{
	/// <summary>
	/// A permutation of the numbers 0 to n - 1
	/// </summary>
	public class Permutation
	{
		#region Private data members

		/// <summary>
		/// The number that is placed at position i by the permutation
		/// </summary>
		int[] _numberAt;
		/// <summary>
		/// The position at which number i is placed by the permutation
		/// </summary>
		int[] _positionOf;

		#endregion

		#region Constructors and factory functions

		/// <summary>
		/// Constructs the identity permutation of size n
		/// </summary>
		public Permutation(int n)
		{
			if (n <= 0)
				throw new ArgumentOutOfRangeException("n");

			_numberAt = Enumerable.Range(0, n).ToArray();
			_positionOf = Enumerable.Range(0, n).ToArray();
		}

		/// <summary>
		/// Copy constructor
		/// </summary>
		public Permutation(Permutation other)
		{
			_numberAt = other._numberAt.ToArray();
			_positionOf = other._positionOf.ToArray();
		}

		/// <summary>
		/// Creates the permutation that results from applying two permutations
		/// after each other, i.e. the product permutation
		/// </summary>
		/// <param name="first">The permutation applied first</param>
		/// <param name="second">The permutation applied second</param>
		public Permutation(Permutation first, Permutation second)
		{
			if (first.Length != second.Length)
				throw new ArgumentException("Lengths must be equal");

			int n = first.Length;
			_numberAt = new int[n];
			_positionOf = new int[n];

			for (int i = 0; i < n; ++i)
			{
				_numberAt[i] = second._numberAt[first._numberAt[i]];
				_positionOf[_numberAt[i]] = i;
			}
		}

		/// <summary>
		/// Returns the permutation that orders the given collection of values
		/// in the same way as Array.Sort()
		/// </summary>
		public static Permutation ThatSorts<T>(T[] values)
		{
			int n = values.Length;
			int[] numberAt = Enumerable.Range(0, n).ToArray();
			T[] tmpValues = (T[])values.Clone();

			Array.Sort(tmpValues, numberAt);

			int[] positionOf = new int[n];
			for (int i = 0; i < n; ++i)
			{
				positionOf[numberAt[i]] = i;
			}

			return new Permutation(numberAt, positionOf);
		}

		/// <summary>
		/// Returns a permutation that is the inverse of this one
		/// </summary>
		public Permutation Inverse()
		{
			return new Permutation((int[])_positionOf.Clone(), (int[])_numberAt.Clone());
		}

		#endregion

		#region Private members

		private Permutation(int[] numberAt, int[] positionOf)
		{
			_numberAt = numberAt;
			_positionOf = positionOf;
		}

		#endregion

		#region Inspectors

		/// <summary>
		/// The length of the permutation
		/// </summary>
		public int Length { get { return _numberAt.Length; } }

		/// <summary>
		/// The number at the given position in the permutation
		/// </summary>
		public int NumberAt(int position)
		{
			if (position < 0 || position >= _numberAt.Length)
				throw new ArgumentOutOfRangeException("position");
			return _numberAt[position];
		}

		/// <summary>
		/// The position of the given number in the position
		/// </summary>
		public int PositionOf(int number)
		{
			if (number < 0 || number >= _numberAt.Length)
				throw new ArgumentOutOfRangeException("number");
			return _positionOf[number];
		}

		#endregion

		#region Operator members

		/// <summary>
		/// Swaps the elements at positions pos1 and pos2
		/// </summary>
		public void Swap(int pos1, int pos2)
		{
			if (pos1 < 0 || pos1 >= _numberAt.Length)
				throw new ArgumentOutOfRangeException("pos1");
			if (pos2 < 0 || pos2 >= _numberAt.Length)
				throw new ArgumentOutOfRangeException("pos2");

			int num1 = _numberAt[pos1];
			int num2 = _numberAt[pos2];

			_numberAt[pos1] = num2;
			_numberAt[pos2] = num1;
			_positionOf[num1] = pos2;
			_positionOf[num2] = pos1;
		}

		#endregion
	}
}
