//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sintef.Scoop.Utilities.Functions;

namespace Sintef.Scoop.Utilities.Tests
{
	[TestClass]
	public class TestingPiecewiseLinearFunctions
	{
		[TestMethod]
		public void TestSums()
		{
			PiecewiseLinearFunction<FunctionPoint> a = new PiecewiseLinearFunction<FunctionPoint>();
			PiecewiseLinearFunction<FunctionPoint> b = new PiecewiseLinearFunction<FunctionPoint>();
			for (int i = 0; i < 10; i++)
			{
				a.AddPoint(i, 3 + i);
				b.AddPoint(1.5 * i, 5 + 0.5 * i);
			}

			PiecewiseLinearFunction<FunctionPoint> sum = a.SumWith(b);
			

			for (int x = 0; x < 20; x++)
			{
				double leftValue = 0;
				bool leftUndefined = false;
				if (a.GetFirstPoint().X < x)
					leftValue += a.GetLeftValue(x);//Do not include left value if not  well defined
				else 
					leftUndefined = true;

				if (b.GetFirstPoint().X < x)
					leftValue += b.GetLeftValue(x);
				else 
					leftUndefined = true;

				if(leftUndefined)
					Assert.IsTrue(sum.GetFirstPoint().X == x || double.IsNegativeInfinity(sum.GetLeftValue(x)));
				else
					Assert.IsTrue(sum.GetLeftValue(x) == leftValue);

				double rightValue = 0;
				bool rightUndefined = false;
				if (a.GetLastPoint().X > x)
					rightValue += a.GetValue(x); //Do not include right value if not  well defined
				else
					rightUndefined = true;

				if (b.GetLastPoint().X > x)
					rightValue += b.GetValue(x);
				else
					rightUndefined = true;

				if(rightUndefined)
					Assert.IsTrue(sum.GetLastPoint().X == x || double.IsNegativeInfinity(sum.GetValue(x)));
				else
					Assert.IsTrue(sum.GetValue(x) == rightValue);
			}
		}

		[TestMethod]
		public void TestPieceWizeConstFunction()
		{
			//Build a function, gradually increasing:
			PiecewiseConstFunction pcf = new PiecewiseConstFunction();
			int step = 10;
			for (int i = 0; i < 10; i++)
			{
				pcf.AddInOpenInterval(i * step, (i + 1) * step, i + 1);
			}

			//Test some integration functions
			Assert.AreEqual(55 * step, pcf.Integral());
			IntInterval inclusiveInt = new IntInterval(5*step, 7*step);
			int fasit = (6 + 7) * step;
			Assert.AreEqual(fasit, pcf.Integral(inclusiveInt));
			Assert.AreEqual(fasit, pcf.IntegralIgnoringNegativeValues(inclusiveInt));
			Assert.AreEqual(0, pcf.IntegralIgnoringPositiveValues(inclusiveInt));

			//Min/Max values
			Assert.AreEqual(2, pcf.MinValue(new IntInterval(2 * step, 9 * step)));
			Assert.AreEqual(10, pcf.MaxValue());

			//Check that negative values are subtracted in sum
			PiecewiseConstFunction copy = new PiecewiseConstFunction(pcf);
			copy.AddInOpenInterval(0, step, -11);
			Assert.AreEqual(-10, copy.MinValue());

			//Test some integration functions on the result
			Assert.AreEqual(44 * step, copy.Integral());
			IntInterval inclusiveIntNeg = new IntInterval(0, 2*step);
			Assert.AreEqual((-10 + 2) * step, copy.Integral(inclusiveIntNeg));
			Assert.AreEqual(2 * step, copy.IntegralIgnoringNegativeValues(inclusiveIntNeg));
			Assert.AreEqual(-10 * step, copy.IntegralIgnoringPositiveValues(inclusiveIntNeg));


			//Subtract a function without lower limit
			PiecewiseConstFunction tosubtract = new PiecewiseConstFunction();
			copy = new PiecewiseConstFunction(pcf);
			tosubtract.AddInOpenInterval(5 * step, 6 * step, 100);
			copy.Subtract(0, tosubtract);
			Assert.AreEqual(-94, copy.MinValue());
			Assert.AreEqual(-45 * step, copy.Integral());

			//Subtract a function WITH lower limit
			copy = new PiecewiseConstFunction(pcf);
			copy.Subtract(0, tosubtract, 1);
			Assert.AreEqual(1, copy.MinValue(new IntInterval(4 * step, 7 * step)));
			Assert.AreEqual(50 * step, copy.Integral());

			//Test GetFirstIntervalWhereLargerThan and GetFirstFreeInterval
			PiecewiseConstFunction toFit = new PiecewiseConstFunction();
			toFit.AddInOpenInterval(0, 2*step, 9);
			IntInterval firstPossible = pcf.GetFirstIntervalWhereLargerThan(new IntInterval(pcf.FirstXValue, pcf.LastXValue), toFit);
			Assert.AreEqual(8*step, firstPossible.Lower);
			Assert.AreEqual(10*step, firstPossible.Upper);
					
			firstPossible = pcf.GetFirstIntervalWhereLargerThan(new IntInterval(pcf.LastXValue - 1, pcf.LastXValue), toFit);//Should not fit
			Assert.AreEqual(null, firstPossible);

			//Build a new function, gradually decreasing
			PiecewiseConstFunction pcfDec = new PiecewiseConstFunction();
			for (int i = 0; i < 10; i++)
			{
				pcfDec.AddInOpenInterval(i * step, (i + 1) * step, (10 - i));
			}

			firstPossible = pcfDec.GetFirstFreeInterval(new IntInterval(pcfDec.FirstXValue, pcfDec.LastXValue), 2 * step, 9);
			Assert.AreEqual(step, firstPossible.Lower);

			firstPossible = pcfDec.GetFirstFreeInterval(new IntInterval(pcfDec.FirstXValue, pcfDec.FirstXValue+1), 2 * step, 9);//Should not fit
			Assert.AreEqual(null, firstPossible);

		}

		[TestMethod]
		public void TestPieceWizeConstFunctionDouble()
		{
			//Build a function, gradually increasing:
			PiecewiseConstFunctionDouble pcf = new PiecewiseConstFunctionDouble();
			int step = 10;
			for (int i = 0; i < 10; i++)
			{
				pcf.AddInOpenInterval(i * step, (i + 1) * step, i + 1);
			}

			//Test some integration functions
			Assert.AreEqual(55 * step, pcf.Integral());
			IntInterval inclusiveInt = new IntInterval(5 * step, 7 * step);
			int fasit = (6 + 7) * step;
			Assert.AreEqual(fasit, pcf.Integral(inclusiveInt));
			Assert.AreEqual(fasit, pcf.IntegralIgnoringNegativeValues(inclusiveInt));
			Assert.AreEqual(0, pcf.IntegralIgnoringPositiveValues(inclusiveInt));

			//Min/Max values
			Assert.AreEqual(2, pcf.MinValue(new IntInterval(2 * step, 9 * step)));
			Assert.AreEqual(10, pcf.MaxValue());

			//Check that negative values are subtracted in sum
			PiecewiseConstFunctionDouble copy = new PiecewiseConstFunctionDouble(pcf);
			copy.AddInOpenInterval(0, step, -11);
			Assert.AreEqual(-10, copy.MinValue());

			//Test some integration functions on the result
			Assert.AreEqual(44 * step, copy.Integral());
			IntInterval inclusiveIntNeg = new IntInterval(0, 2 * step);
			Assert.AreEqual((-10 + 2) * step, copy.Integral(inclusiveIntNeg));
			Assert.AreEqual(2 * step, copy.IntegralIgnoringNegativeValues(inclusiveIntNeg));
			Assert.AreEqual(-10 * step, copy.IntegralIgnoringPositiveValues(inclusiveIntNeg));


			//Subtract a function without lower limit
			PiecewiseConstFunctionDouble tosubtract = new PiecewiseConstFunctionDouble();
			copy = new PiecewiseConstFunctionDouble(pcf);
			tosubtract.AddInOpenInterval(5 * step, 6 * step, 100);
			copy.Subtract(0, tosubtract);
			Assert.AreEqual(-94, copy.MinValue());
			Assert.AreEqual(-45 * step, copy.Integral());

			//Subtract a function WITH lower limit
			copy = new PiecewiseConstFunctionDouble(pcf);
			copy.Subtract(0, tosubtract, 1);
			Assert.AreEqual(1, copy.MinValue(new IntInterval(4 * step, 7 * step)));
			Assert.AreEqual(50 * step, copy.Integral());

			//Test GetFirstIntervalWhereLargerThan and GetFirstFreeInterval
			PiecewiseConstFunctionDouble toFit = new PiecewiseConstFunctionDouble();
			toFit.AddInOpenInterval(0, 2 * step, 9);
			IntInterval firstPossible = pcf.GetFirstIntervalWhereLargerThan(new IntInterval(pcf.FirstXValue, pcf.LastXValue), toFit);
			Assert.AreEqual(8 * step, firstPossible.Lower);
			Assert.AreEqual(10 * step, firstPossible.Upper);

			firstPossible = pcf.GetFirstIntervalWhereLargerThan(new IntInterval(pcf.LastXValue - 1, pcf.LastXValue), toFit);//Should not fit
			Assert.AreEqual(null, firstPossible);

			firstPossible = pcf.GetFirstIntervalWhereLargerThan(new IntInterval(7*step, pcf.LastXValue), toFit);//Should not fit
			Assert.AreEqual(8 * step, firstPossible.Lower);

			firstPossible = pcf.GetFirstIntervalWhereLargerThan(new IntInterval(8 * step, pcf.LastXValue), toFit);//Should not fit
			Assert.AreEqual(8 * step, firstPossible.Lower);

			PiecewiseConstFunctionDouble toFitsmall = new PiecewiseConstFunctionDouble();
			toFitsmall.AddInOpenInterval(0, 2 * step, 1);
			firstPossible = pcf.GetFirstIntervalWhereLargerThan(new IntInterval(0, pcf.LastXValue), toFitsmall);//Should not fit
			Assert.AreEqual(0, firstPossible.Lower);
			firstPossible = pcf.GetFirstIntervalWhereLargerThan(new IntInterval(-1000, pcf.LastXValue), toFitsmall);//Should not fit
			Assert.AreEqual(0, firstPossible.Lower);

			//Build a new function, gradually decreasing
			PiecewiseConstFunctionDouble pcfDec = new PiecewiseConstFunctionDouble();
			for (int i = 0; i < 10; i++)
			{
				pcfDec.AddInOpenInterval(i * step, (i + 1) * step, (10 - i));
			}

			firstPossible = pcfDec.GetFirsIntervalNeverHigherThan(new IntInterval(pcfDec.FirstXValue, pcfDec.LastXValue), 2 * step, 9);
			Assert.AreEqual(step, firstPossible.Lower);

			firstPossible = pcfDec.GetFirsIntervalNeverHigherThan(new IntInterval(pcfDec.FirstXValue, pcfDec.FirstXValue + 1), 2 * step, 9);//Should not fit
			Assert.AreEqual(null, firstPossible);

		}

	}
}
