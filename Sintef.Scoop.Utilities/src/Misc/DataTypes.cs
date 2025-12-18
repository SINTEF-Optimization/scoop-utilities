//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using System.Runtime.InteropServices;


namespace Sintef.Scoop.Utilities
{
  /// <summary>
  /// Data type of two integers
  /// </summary>
  [StructLayout(LayoutKind.Sequential)]
  public struct int2
  {
    /// <summary>
    /// The first integer
    /// </summary>
    public int First;

    /// <summary>
    /// The second integer
    /// </summary>
    public int Second;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="f">First value</param>
    /// <param name="s">Second value</param>
    public int2(int f, int s) { First = f; Second = s; }

		#region Operator overloads
		/// <inheritdoc/>
		public static bool operator ==(int2 x, int2 y) { return x.First == y.First && x.Second == y.Second; }
		/// <inheritdoc/>
		public static bool operator !=(int2 x, int2 y) { return !(x == y); }
		/// <inheritdoc/>
		public override bool Equals(object o) { return this == (int2)o; }
		/// <inheritdoc/>
		public override int GetHashCode() { return First + Second; }
    #endregion

  }

  /// <summary>
  /// Structure containing one bool, used when the value must be marshalled to a 1-byte C-like bool for unmanaged code. 
  /// Introduced because of problem with marshalling an array of bools to unmanaged code.
  /// A bug on this was reported to Microsoft in 2007 (see  http://social.msdn.microsoft.com/forums/en-US/clr/thread/a4b0cd27-95c9-41f4-aad9-9cc53af70e12),
  /// but still persists, it seems. You've got to love it...
  /// </summary>
  [StructLayout(LayoutKind.Sequential)]
  public struct BoolI1
  {
    private BoolI1(bool data) { _value = data; }

		#region Operator overloads
		/// <inheritdoc/>
		public static implicit operator BoolI1(bool val) { return new BoolI1(val); }
		/// <inheritdoc/>
		public static implicit operator bool(BoolI1 myval) { return myval._value; }
		/// <inheritdoc/>
		public static bool operator ==(BoolI1 x, BoolI1 y) { return x._value == y._value; }
		/// <inheritdoc/>
		public static bool operator !=(BoolI1 x, BoolI1 y) { return x._value != y._value; }
		/// <inheritdoc/>
		public override bool Equals(object o) { return this == (BoolI1)o; }
		/// <inheritdoc/>
		public override int GetHashCode() { return _value ? 1 : 0; }
    #endregion

    //       [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.I1)]
    [MarshalAs(UnmanagedType.I1)]
    private bool _value;
  }
 
}
