//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace Sintef.Scoop.Utilities
{
	//Utility classes for randomizing functionality that is not available in .Net

	/// <summary>
	/// A random generator with some debugging functionality.
	/// </summary>
	public class ScoRandom : Random
	{
		#region Properties

		/// <summary>
		/// For assigning random generator ID's
		/// </summary>
		static int IDCounter = 0;

		/// <summary>
		/// Random generator ID
		/// </summary>
		public int ID { get; private set; }

		/// <summary>
		/// The managed thread ID of the current thread
		/// </summary>
		public int ThreadID { get; private set; }
		#endregion

		#region Construction

		/// <summary>
		/// Default constructor
		/// </summary>
		internal ScoRandom()
			: base()
		{
			ThreadID = Thread.CurrentThread.ManagedThreadId;
			ID = ++IDCounter;
		}

		/// <summary>
		/// Constructor with seed.
		/// </summary>
		/// <param name="seed"></param>
		internal ScoRandom(int seed)
			: base(seed)
		{
			ID = ++IDCounter;
			ThreadID = Thread.CurrentThread.ManagedThreadId;
		}

		#endregion

		/// <summary>
		/// Returns a nonnegative random number.
		/// </summary>
		/// <returns></returns>
		public override int Next()
		{
			int rand = base.Next();

#if DEBUG
			if (ThreadID != Thread.CurrentThread.ManagedThreadId)
				throw new Exception("ScoRandom.Next() called on different thread than the one on which it was created");

			if (RandomCreator.DebugOutPutOfRandomNumbers)
			{
				StackTrace stackTrace = new StackTrace();
				MethodBase caller = stackTrace.GetFrame(1).GetMethod();
				Console.WriteLine("ScoRandom.Next() called from " + caller.Name + " produced: " + rand.ToString());
			}
#endif

			return rand;
		}

		/// <summary>
		/// Returns a nonnegative random number less than the specified maximum.
		/// </summary>
		public override int Next(int maxValue)
		{
			int rand = base.Next(maxValue);

#if DEBUG
			if (ThreadID != Thread.CurrentThread.ManagedThreadId)
				throw new Exception("ScoRandom.Next(int maxValue) called on different thread than the one on which it was created");
			
			if (RandomCreator.DebugOutPutOfRandomNumbers)
			{
				StackTrace stackTrace = new StackTrace();
				MethodBase caller = stackTrace.GetFrame(1).GetMethod();
				Console.WriteLine("ScoRandom.Next(" + maxValue.ToString() + ") called from " + caller.Name + " produced: " + rand.ToString());
			}
#endif

			return rand;
		}

		/// <summary>
		/// Returns a random number within a specified range
		/// </summary>
		public override int Next(int minValue, int maxValue)
		{
			int rand = base.Next(minValue, maxValue);

#if DEBUG
			if (ThreadID != Thread.CurrentThread.ManagedThreadId)
				throw new Exception("ScoRandom.Next(int minValue, int maxValue) called on different thread than the one on which it was created");
			
			if (RandomCreator.DebugOutPutOfRandomNumbers)
			{
				StackTrace stackTrace = new StackTrace();
				MethodBase caller = stackTrace.GetFrame(1).GetMethod();
				Console.WriteLine("ScoRandom.Next(" + minValue.ToString() + ", " + maxValue.ToString() + ") called from " + caller.Name + " produced: " + rand.ToString());
			}
#endif

			return rand;
		}
	}


	/// <summary>
	/// Generates random generators (objects of class Random) with seeds chosen
	/// according to the static property SeedToUse. It is used so that a global flag may be set
	/// in only one location, changing the behaviour of random generators from re-producable to more
	/// random behaviour. This is useful when performing statistical testing of software that may
	/// otherwise, for purposes of supportability, used fixed seeds.
	/// Thread safe.
	/// </summary>
	public static class RandomCreator
	{
		/// <summary>
		/// Enum enumerating the types of seed usage that the class supports.
		/// </summary>
		public enum SeedType
		{
			/// <summary>
			/// Use the same seed (42) every time.
			/// </summary>
			FIXED,

			/// <summary>
			/// Seed is based on wall time.
			/// </summary>
			FROM_SYSTEM_CLOCK
		}

		/// <summary>
		/// The current seed type
		/// </summary>
		static SeedType _seedType = SeedType.FROM_SYSTEM_CLOCK;

		/// <summary>
		/// The current generator for seeds for new random generators
		/// </summary>
		static Random _globalGenerator = new Random();

		/// <summary>
		/// A thread static generator. This is created the first time a thread needs it, and
		/// simply returned after that.
		/// In some cases, it is necessary to start
		/// with a new generator (e.g. in connecion with unit testing it is important that the sequence
		/// of tests have no influence on individual test results).
		/// The can be achieved by setting this generator to null, calling ResetGenerator();
		/// </summary>
		[ThreadStatic]
		static Random _localGenerator;

#if DEBUG

		/// <summary>
		/// Global flag for determining whether to output to Console every produced random number.
		/// </summary>
		static public bool DebugOutPutOfRandomNumbers = false;

#endif

		/// <summary>
		/// A global, application wide, flag for what kind of seeds are used when the function 
		/// CreateRandomGenerator is called. It is implemented this way so that this flag may be set
		/// in only one location, changing the behaviour of random generators from re-producible to more
		/// random behaviour. This is useful when performing statistical testing of software that may
		/// otherwise, for purposes of supportability, used fixed seeds.
		/// The default value is to use a seed based on the real time.
		/// </summary>
		static public SeedType GlobalSeedUsageForRandomCreation
		{
			get { return _seedType; }
			set
			{
				switch (value)
				{
					case SeedType.FIXED:

						//TODO remove
						object tt = _globalGenerator;
						if (System.Threading.Monitor.TryEnter(tt))
							System.Threading.Monitor.Exit(tt);
						else
						{
							string s = "Thread wait";
							s += "\n";
						}

						lock (_globalGenerator) _globalGenerator = new Random(42);
						break;
					case SeedType.FROM_SYSTEM_CLOCK:

						//TODO remove
						tt = _globalGenerator;
						if (System.Threading.Monitor.TryEnter(tt))
							System.Threading.Monitor.Exit(tt);
						else
						{
							string s = "Thread wait";
							s += "\n";
						}

						lock (_globalGenerator) _globalGenerator = new Random();
						break;
					default:
						throw new ArgumentException("Illegal seed type");
				}
				_seedType = value;
			}
		}

		/// <summary>
		/// Returns a thread-static random generator. If one has not been created for the thread,
		/// it is. Depending on the value of GlobalSeedUsageForRandomCreation,
		/// the generator will than either behave reproducibly when the application is run more than once with the same
		/// parameters, or have a seed that depends on the time (more truly random).
		/// </summary>
		/// <returns></returns>
		static public Random GetRandomGenerator()
		{
			if (_localGenerator == null)
			{
				int seed;

				//TODO remove
				object t = _globalGenerator;
				if (System.Threading.Monitor.TryEnter(t))
					System.Threading.Monitor.Exit(t);
				else
				{
					string s = "Thread wait";
					s += "\n";
				}

				lock (_globalGenerator) seed = _globalGenerator.Next();
				_localGenerator = new Random(seed);
			}

#if DEBUG
			if(DebugOutPutOfRandomNumbers)
			{
				StackTrace stackTrace = new StackTrace();
				MethodBase caller = stackTrace.GetFrame(1).GetMethod();
				Console.WriteLine("CreateRandomGenerator called from " + caller.Name+". Creating random generator with ID = " + (_localGenerator as ScoRandom).ID.ToString());
			}
#endif

			return _localGenerator;
		}

		/// <summary>
		/// Sets the local, thread static, generator to null. A new generator will then be created
		/// on the next call to GetRandomGenerator. Use this e.g. to ensure that
		/// a unit test starts with a new generator.
		/// </summary>
		static public void ResetGenerator()
		{
			_localGenerator = null;
		}
	}

}
