using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#if NETFRAMEWORK

namespace Sintef.Scoop.Utilities
{
	/// <summary>
	/// Helper for testing numerous input variations in WCF API calls.
	/// Checks that valid data for API calls are accepted and invalid data are rejected.
	/// </summary>
	public class WcfVariationTester : VariationTester
	{
		/// <summary>
		/// Initializes the tester
		/// </summary>
		public WcfVariationTester()
		{
			Fail = (message) => Assert.Fail(message);

			ClassifyException = ClassifyWcfException;
		}

		/// <summary>
		/// Classifies the exception by detecting WCF-specific exception types
		/// </summary>
		private (ExceptionLocation type, Exception ex) ClassifyWcfException(Exception ex)
		{
			if (ex is System.ServiceModel.CommunicationException)
			{
				// Exception thrown during service invocation. Unpack inner exception to get at serialization exceptions
				if (ex.InnerException != null)
					ex = ex.InnerException;
			}

			ExceptionLocation type = ExceptionLocation.InClient;

			if (ex is System.ServiceModel.FaultException)
			{
				type = ExceptionLocation.InService;
			}
			else if (ex is System.Runtime.Serialization.SerializationException)
			{
				type = ExceptionLocation.InSerialization;
			}

			return (type, ex);
		}

	}
}

#endif