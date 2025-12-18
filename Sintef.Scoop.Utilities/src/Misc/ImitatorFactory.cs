//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

#if NETFRAMEWORK || NET5_0_OR_GREATER

namespace Sintef.Scoop.Utilities
{
	/// <summary>
	/// A factory for imitators.
	/// 
	/// An imitator is defined in terms of two other objects: the source object and the helper object.
	/// The imitator's class is a (dynamically generated) subclass of the source object class. The imitator is
	/// created using the source copy constructor, and will therefore behave like the source object.
	/// In addition, the imitator class implements all interfaces that are implemented by the helper class and
	/// forwards calls to these methods to the helper object (unless the source class already implements the
	/// interface). 
	/// 
	/// In short, the imitator mimics the source object, while using the helper to also support all of
	/// the helper's interfaces.
	/// 
	/// The intended use of this functionality is when you want to create a wrapper object that modifies some of the
	/// functionality of a contained object. As the wrapper cannot foresee all the interfaces that a contained object may implement,
	/// the wrapper can behave differently or not at all in some contexts.
	/// By creating an imitator, using the wrapper as the source and the contained
	/// object as the helper, you can produce an object that wraps the contained object in the correct way, while
	/// also exposing all of its functionality.
	/// </summary>
	public static class ImitatorFactory
	{
		struct ImitatorClassParameters
		{
			public Type _sourceType;
			public Type _helperType;
			public bool _requireAllInterfaces;
		}

		/// <summary>
		/// The imitator classes we have created so far.
		/// </summary>
		static readonly Dictionary<ImitatorClassParameters, Type> _imitatorClasses = new();

		/// <summary>
		/// The dynamic module in which we create imitator classes
		/// </summary>
		static ModuleBuilder _moduleBuilder;

		/// <summary>
		/// Creates and returns an imitator.
		/// 
		/// All interfaces implemented by the helper class (and not by the source class) must be publicly visible
		/// </summary>
		/// <param name="source">The source object. The class must be publicly visible and have a publicly visible
		///   copy constructor</param>
		/// <param name="helper">The helper object</param>
		/// <returns>The new imitator</returns>
		public static object GetImitator(object source, object helper)
		{
			return GetImitator(source, helper, true);
		}

		/// <summary>
		/// Creates and returns an imitator
		/// </summary>
		/// <param name="source">The source object. The class must be publicly visible and have a publicly visible
		///   copy constructor</param>
		/// <param name="helper">The helper object</param>
		/// <param name="requireAllInterfaces">If true, all interfaces implemented by the helper class 
		/// (and not by the source class) 
		/// must be publicly visible. If false, nonpublic interfaces are allowed, but are not
		/// implemented by the imitator.</param>
		/// <returns>The new imitator</returns>
		public static object GetImitator(object source, object helper, bool requireAllInterfaces)
		{
			Type sourceType = source.GetType();
			Type helperType = helper.GetType();

			ImitatorClassParameters parms = new()
			{
				_sourceType = sourceType,
				_helperType = helperType,
				_requireAllInterfaces = requireAllInterfaces
			};

			return GetImitator(source, helper, parms);
		}

		/// <summary>
		/// Crates and returns an imitator based on the supplied parameters
		/// </summary>
		private static object GetImitator(object source, object helper, ImitatorClassParameters parms)
		{
			Type imitatorClass = _imitatorClasses.ItemOrAdd(parms, () => CreateImitatorClass(parms));

			//builder.Save(modName);

			ConstructorInfo constructor = imitatorClass.GetConstructor(new Type[] { parms._sourceType, parms._helperType });

			return constructor.Invoke(new object[] { source, helper });
		}

		/// <summary>
		/// Creates an imitator class for the given source and helper classes
		/// </summary>
		private static Type CreateImitatorClass(ImitatorClassParameters parms)
		{
			Type sourceType = parms._sourceType;
			Type helperType = parms._helperType;

			try
			{
				if (!sourceType.IsVisible)
					throw new Exception("Imitation source type " + sourceType + " is not publicly visible");

				string typeName = "Imitator_" + sourceType.AssemblyQualifiedName + "_" + helperType.AssemblyQualifiedName;

				if (_moduleBuilder == null)
				{
					string assemblyName = "ImitatorsAssembly";
					string modName = "ImitatorsModule";
					AssemblyName name = new(assemblyName);

#if NET5_0_OR_GREATER
					var builder = AssemblyBuilder.DefineDynamicAssembly(name, AssemblyBuilderAccess.Run);
					_moduleBuilder = builder.DefineDynamicModule(modName);
#else
					AppDomain domain = AppDomain.CurrentDomain;
					var builder = domain.DefineDynamicAssembly(
						name, AssemblyBuilderAccess.RunAndSave);
					_moduleBuilder = builder.DefineDynamicModule(modName, true);
#endif

				}

				Type[] helperInterfaces = helperType.GetInterfaces();
				Type[] srcInterfaces = sourceType.GetInterfaces();

				Type[] newInterfaces = helperInterfaces.Except(srcInterfaces).ToArray();

				if (!parms._requireAllInterfaces)
				{
					// Implement just the publicly visible interfaces
					newInterfaces = newInterfaces.Where(x => x.IsVisible).ToArray();
				}

				if (!helperType.IsVisible)
				{
					// Treat helper as general object instead.
					// The cost is slightly more expensive typecasts
					helperType = typeof(object);
				}

				// Create type
				TypeBuilder imitatorBuilder = _moduleBuilder.DefineType(typeName,
					TypeAttributes.Public | TypeAttributes.Class, sourceType, newInterfaces);

				// Define a field that holds the helper
				FieldBuilder helperField = imitatorBuilder.DefineField("_helper", helperType, FieldAttributes.Private);

				// Create constructor
				ConstructorInfo constructor = CreateConstructor(imitatorBuilder, sourceType, helperType, helperField);

				// Implement each interface
				foreach (Type interfc in newInterfaces)
					ImplementInterface(interfc, imitatorBuilder, helperField);

				// Finish type
				return imitatorBuilder.CreateType();
			}
			catch (Exception ex)
			{
				throw new Exception(string.Format("Problem creating an imitator class with {0} as source and {1} as helper", sourceType.Name, helperType.Name), ex);
			}
		}

		/// <summary>
		/// Adds implementation of the given interface to the imitator class
		/// </summary>
		/// <param name="interfc">Interface to implement</param>
		/// <param name="imitatorBuilder">Type builder for imitator class</param>
		/// <param name="helperField">Field in imitator class that holds helper</param>
		private static void ImplementInterface(Type interfc, TypeBuilder imitatorBuilder, FieldBuilder helperField)
		{
			if (!interfc.IsVisible)
				throw new Exception("Imitation helper interface " + interfc + " is not publicly visible");

			foreach (var method in interfc.GetMethods())
				ImplementMethod(imitatorBuilder, interfc, method, helperField);
		}

		/// <summary>
		/// Imlpements a method that forwards the call to the helper
		/// </summary>
		/// <param name="imitatorBuilder">Type builder for the imitator class</param>
		/// <param name="interfc">Iterface that method belongs to</param>
		/// <param name="interfaceMethod">Method to implement</param>
		/// <param name="helperField">Field in imitator class that holds helper</param>
		private static void ImplementMethod(TypeBuilder imitatorBuilder, Type interfc, MethodInfo interfaceMethod, FieldBuilder helperField)
		{
			Type[] parameters = interfaceMethod.GetParameters().Select(x => x.ParameterType).ToArray();

			// Create method
			MethodBuilder method = imitatorBuilder.DefineMethod(interfaceMethod.Name + "_imitation", MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual,
				interfaceMethod.ReturnType, parameters);

			// Declare interface implementation
			imitatorBuilder.DefineMethodOverride(method, interfaceMethod);

			var gen = method.GetILGenerator();

			// Create code:

			// Load helper
			gen.Emit(OpCodes.Ldarg_0);
			gen.Emit(OpCodes.Ldfld, helperField);
			// Cast to interface type
			gen.Emit(OpCodes.Castclass, interfc);
			// Load all method parameters
			byte i, n = (byte)parameters.Length;
			for (i = 0; i < n; ++i)
			{
				gen.Emit(OpCodes.Ldarg_S, (byte)(i + 1));
			}
			// Forward call
			gen.Emit(OpCodes.Call, interfaceMethod);
			gen.Emit(OpCodes.Ret);
		}

		/// <summary>
		/// Creates a constructor for the imitator class. The constructor takes two
		/// arguments: the source and the helper
		/// </summary>
		/// <param name="imitatorBuilder">Type builder for the imitator class</param>
		/// <param name="sourceType">Source class</param>
		/// <param name="helperType">Helper class</param>
		/// <param name="helperField">Field in imitator class that holds the helper</param>
		/// <returns></returns>
		private static ConstructorInfo CreateConstructor(TypeBuilder imitatorBuilder, Type sourceType, Type helperType, FieldBuilder helperField)
		{
			ConstructorBuilder constructor =
				imitatorBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, new Type[] { sourceType, helperType });

			// Find copy constructor in source class
			ConstructorInfo sourceCopyConstructor = sourceType.GetConstructor(BindingFlags.ExactBinding | BindingFlags.Public | BindingFlags.Instance, System.Type.DefaultBinder, new Type[] { sourceType }, null);
			if (sourceCopyConstructor == null)
				throw new Exception(String.Format("Imitation source type {0} does not define a public copy constructor", sourceType));

			var gen = constructor.GetILGenerator();

			// Create code:

			// Call source class copy constructor
			gen.Emit(OpCodes.Ldarg_0);
			gen.Emit(OpCodes.Ldarg_1);
			gen.Emit(OpCodes.Call, sourceCopyConstructor);
			// Store helper in helper field
			gen.Emit(OpCodes.Ldarg_0);
			gen.Emit(OpCodes.Ldarg_2);
			gen.Emit(OpCodes.Stfld, helperField);
			gen.Emit(OpCodes.Ret);

			return constructor;
		}
	}
}

#endif