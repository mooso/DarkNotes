using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;
using System.Globalization;

namespace Microsoft.Experimental.DarkNotes
{
	/// <summary>
	/// The main entry point API for Dark Notes.
	/// </summary>
	public sealed class DarkJava : DynamicObject, IDisposable
	{
		private readonly JniWrapper _jniWrapper;
		private readonly JavaPackage _defaultPackage;
		private readonly List<JavaPackage> _importedPackages = new List<JavaPackage>();

		private DarkJava(string jvmDllPath, bool attemptVmReuse, IEnumerable<JavaOption> options)
		{
			_jniWrapper = new JniWrapper(jvmDllPath: jvmDllPath, attemptVmReuse: attemptVmReuse, options: options ?? Enumerable.Empty<JavaOption>());
			_defaultPackage = new JavaPackage(_jniWrapper, "");
		}

		/// <summary>
		/// Create the Java VM and wrap it.
		/// </summary>
		/// <param name="jvmDllPath">(Optional) Path to jvm.dll where we'll load the JNI library. If not provided we'll try to infer its location from the Java install path.</param>
		/// <param name="attemptVmReuse">
		/// This is an annoying workaround to a fundamental JNI problem: you can't host a JVM within the process again once you've hosted it once
		/// (ever, even if you destroy the first one). So if this flag is <c>true</c>, we'll attempt to find an existing JVM in the process and use that 
		/// if available, and we won't destroy the JVM on dispose so subsequent constructions of this object will succeed.
		/// The downside of reuse of course is that if we're reusing it then we won't be able to construct it with fresh arguments.
		/// </param>
		/// <param name="options">(Optional) The options to pass to the Java VM upon creation.</param>
		public static DarkJava CreateVm(string jvmDllPath = null, bool attemptVmReuse = false, IEnumerable<JavaOption> options = null)
		{
			return new DarkJava(jvmDllPath, attemptVmReuse, options);
		}

		public void ImportPackage(string packageName)
		{
			_importedPackages.Add(new JavaPackage(_jniWrapper, packageName));
		}

		/// <summary>
		/// Detaches the current thread - should be called in any thread outside of the main thread where this object is used after it's done with it.
		/// </summary>
		public void DetachThread()
		{
			_jniWrapper.DetachThread();
		}

		public override bool TryGetMember(GetMemberBinder binder, out object result)
		{
			object classResult = null;
			if (_importedPackages.Any(p => p.TryGetClass(binder, out classResult)))
			{
				result = classResult;
				return true;
			}
			return _defaultPackage.TryGetMember(binder, out result);
		}

		private JavaArray CreateArray(Array dotNetArray)
		{
			bool[] asBool;
			byte[] asByte;
			char[] asChar;
			double[] asDouble;
			float[] asFloat;
			int[] asInt;
			long[] asLong;
			short[] asShort;
			JavaObject[] asObject;
			if ((asBool = dotNetArray as bool[]) != null)
			{
				return new JavaArray(_jniWrapper, _jniWrapper.NewBooleanArray(asBool), new ArrayType(PrimitiveType.Boolean(_jniWrapper)));
			}
			else if ((asByte = dotNetArray as byte[]) != null)
			{
				return new JavaArray(_jniWrapper, _jniWrapper.NewByteArray(asByte), new ArrayType(PrimitiveType.Byte(_jniWrapper)));
			}
			else if ((asChar = dotNetArray as char[]) != null)
			{
				return new JavaArray(_jniWrapper, _jniWrapper.NewCharArray(asChar), new ArrayType(PrimitiveType.Char(_jniWrapper)));
			}
			else if ((asDouble = dotNetArray as double[]) != null)
			{
				return new JavaArray(_jniWrapper, _jniWrapper.NewDoubleArray(asDouble), new ArrayType(PrimitiveType.Double(_jniWrapper)));
			}
			else if ((asFloat = dotNetArray as float[]) != null)
			{
				return new JavaArray(_jniWrapper, _jniWrapper.NewFloatArray(asFloat), new ArrayType(PrimitiveType.Float(_jniWrapper)));
			}
			else if ((asInt = dotNetArray as int[]) != null)
			{
				return new JavaArray(_jniWrapper, _jniWrapper.NewIntArray(asInt), new ArrayType(PrimitiveType.Int(_jniWrapper)));
			}
			else if ((asLong = dotNetArray as long[]) != null)
			{
				return new JavaArray(_jniWrapper, _jniWrapper.NewLongArray(asLong), new ArrayType(PrimitiveType.Long(_jniWrapper)));
			}
			else if ((asShort = dotNetArray as short[]) != null)
			{
				return new JavaArray(_jniWrapper, _jniWrapper.NewShortArray(asShort), new ArrayType(PrimitiveType.Short(_jniWrapper)));
			}
			else if ((asObject = dotNetArray as JavaObject[]) != null)
			{
				JavaObject prototype = asObject.FirstOrDefault(o => o != null);
				if (prototype == null)
				{
					throw new InvalidOperationException("Can't construct a Java array without at least one non-null object in there.");
				}
				return new JavaArray(_jniWrapper, prototype.Class.NewArray(asObject.Select(o => o.Pointer).ToArray()), new ArrayType(prototype.Class));
			}
			else
			{
				throw new InvalidOperationException("Can't construct a Java array from the given array of the given type: " + dotNetArray.GetType());
			}
		}

		public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
		{
			if (String.Equals(binder.Name, "newArray", StringComparison.OrdinalIgnoreCase) && args.Length == 1 && args[0] is Array)
			{
				result = CreateArray((Array)args[0]);
				return true;
			}
			return base.TryInvokeMember(binder, args, out result);
		}

		public void Dispose()
		{
			_jniWrapper.Dispose();
		}
	}
}
