using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DarkNotes.JniInterface;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;
using DarkNotes.CoreClassProxies;
using System.Threading;

namespace DarkNotes
{
	/// <summary>
	/// Main wrapper for the low-level JNI functionality.
	/// </summary>
	/// <remarks>
	/// This class is exposed publicly today, but once I create a better entry-point API to Dark Notes I'll make this class internal.
	/// </remarks>
	internal class JniWrapper : IDisposable
	{
		private readonly string _vmDll;
		private readonly IntPtr _envP;
		private readonly IntPtr _vmP;
		private readonly JavaVM _vm;
		private readonly JniEnv _env;
		private IntPtr _vmDllModule;
		private readonly bool _vmReuse;
		private readonly ThreadLocal<IntPtr> _currentThreadEnv;
		private const int _jniVersion = 0x00010006;

		private unsafe delegate int CreateJavaVM(IntPtr* pvm, IntPtr* penv, IntPtr args);
		private unsafe delegate int GetCreatedJavaVMs(IntPtr* pvm, int bufferLength, out int numVms);
		private delegate int GetDefaultJavaVMInitArgs(ref JavaVMInitArgs args);

		[DllImport("kernel32.dll", EntryPoint = "LoadLibraryW")]
		private static extern IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPWStr)] string name);

		[DllImport("kernel32.dll", EntryPoint = "FreeLibrary")]
		private static extern bool FreeLibrary(IntPtr hModule);

		[DllImport("kernel32.dll", EntryPoint = "GetProcAddress")]
		private static extern IntPtr GetProcAddress(IntPtr hModule, [MarshalAs(UnmanagedType.LPStr)] string name);

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
		internal JniWrapper(string jvmDllPath, bool attemptVmReuse, IEnumerable<JavaOption> options)
		{
			_currentThreadEnv = new ThreadLocal<IntPtr>(() =>
				{
					IntPtr ret;
					unsafe
					{
						if (_vm.GetDelegate<GetEnv>()(_vmP, (IntPtr)(&ret), _jniVersion) == -2)
						{
							_vm.GetDelegate<AttachCurrentThread>()(_vmP, (IntPtr)(&ret), IntPtr.Zero);
						}
						return ret;
					}
				});
			if (String.IsNullOrEmpty(jvmDllPath))
			{
				string javaHome = GetJavaInstallationPath();
				_vmDll = Path.Combine(javaHome, "bin", "server", "jvm.dll");
			}
			else
			{
				_vmDll = jvmDllPath;
			}
			if (!File.Exists(_vmDll))
			{
				throw new InvalidOperationException("Couldn't find the runtime library for Java at: " + _vmDll);
			}
			IntPtr envP;
			IntPtr vmP;
			_vmDllModule = LoadLibrary(_vmDll);
			if (_vmDllModule == IntPtr.Zero)
			{
				throw new Win32Exception();
			}
			if (attemptVmReuse)
			{
				_vmReuse = true;
				int numVms;
				unsafe
				{
					ThrowOnBadResult(GetDelegateFromJni<GetCreatedJavaVMs>("JNI_GetCreatedJavaVMs")(&vmP, 1, out numVms), "GetCreatedJavaVMs");
					if (numVms >= 1)
					{
						_vmP = vmP;
						_vm = (JavaVM)Marshal.PtrToStructure(vmP, typeof(JavaVM));
						ThrowOnBadResult(_vm.GetDelegate<AttachCurrentThread>()(_vmP, (IntPtr)(&envP), IntPtr.Zero), "AttachCurrentThread");
						_envP = envP;
						_env = (JniEnv)Marshal.PtrToStructure(envP, typeof(JniEnv));
						return;
					}
				}
			}
			JavaVMInitArgs vmArgs = new JavaVMInitArgs();
			GetDelegateFromJni<GetDefaultJavaVMInitArgs>("JNI_GetDefaultJavaVMInitArgs")(ref vmArgs);
			vmArgs.Version = _jniVersion;
			vmArgs._nOptions = options.Count();
			vmArgs._options = Marshal.AllocHGlobal(vmArgs._nOptions * Marshal.SizeOf(typeof(JavaVMOption)));
			IntPtr vmArgsP = Marshal.AllocHGlobal(Marshal.SizeOf(vmArgs));
			try
			{
				int optionsLocation = 0;
				foreach (JavaOption currentOption in options)
				{
					Marshal.StructureToPtr(currentOption.ActualOption, vmArgs._options + optionsLocation * Marshal.SizeOf(typeof(JavaVMOption)), false);
					optionsLocation++;
				}
				Marshal.StructureToPtr(vmArgs, vmArgsP, false);
				unsafe
				{
					ThrowOnBadResult(GetDelegateFromJni<CreateJavaVM>("JNI_CreateJavaVM")(&vmP, &envP, vmArgsP), "CreateJavaVM");
					_vm = (JavaVM)Marshal.PtrToStructure(vmP, typeof(JavaVM));
					_env = (JniEnv)Marshal.PtrToStructure(envP, typeof(JniEnv));
					_envP = envP;
					_vmP = vmP;
				}
			}
			finally
			{
				Marshal.DestroyStructure(vmArgsP, typeof(JavaVMInitArgs));
				Marshal.FreeHGlobal(vmArgsP);
				Marshal.FreeHGlobal(vmArgs._options);
			}
		}

		private static void ThrowOnBadResult(int ret, string operationName)
		{
			if (ret != 0)
			{
				throw new InvalidOperationException(operationName + " failed - return value obtained: " + ret);
			}
		}

		private T GetDelegateFromJni<T>(string procName)
			where T: class
		{
			return Marshal.GetDelegateForFunctionPointer(GetProcAddress(_vmDllModule, procName), typeof(T)) as T;
		}

		private static string GetJavaInstallationPath()
		{
			string environmentPath = Environment.GetEnvironmentVariable("JAVA_HOME");
			if (!string.IsNullOrEmpty(environmentPath))
			{
				return environmentPath;
			}

			using (RegistryKey javaKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\JavaSoft\Java Runtime Environment"))
			{
				if (javaKey == null)
				{
					throw new InvalidOperationException("Couldn't find Java installed on the machine");
				}
				string currentVersion = javaKey.GetValue("CurrentVersion").ToString();
				using (RegistryKey key = javaKey.OpenSubKey(currentVersion))
				{
					return key.GetValue("JavaHome").ToString();
				}
			}
		}

		public IntPtr GetStaticFieldId(IntPtr classP, string fieldName, string fieldSignature)
		{
			return SafeJniCall(_env.GetDelegate<GetStaticFieldID>().Invoke, classP, fieldName, fieldSignature);
		}
		
		public IntPtr FindClass(string className)
		{
			return SafeJniCall(_env.GetDelegate<FindClass>().Invoke, className);
		}

		public long GetVersion()
		{
			return SafeJniCall(_env.GetDelegate<GetVersion>().Invoke);
		}

		public IntPtr FromReflectedField(IntPtr reflectedField)
		{
			return SafeJniCall(_env.GetDelegate<FromReflectedField>().Invoke, reflectedField);
		}

		public IntPtr FromReflectedMethod(IntPtr reflectedMethod)
		{
			return SafeJniCall( _env.GetDelegate<FromReflectedMethod>().Invoke, reflectedMethod);
		}

		public IntPtr GetStaticObjectField(IntPtr classP, IntPtr fieldId)
		{
			return SafeJniCall(_env.GetDelegate<GetStaticObjectField>().Invoke, classP, fieldId);
		}

		public IntPtr GetObjectField(IntPtr obj, IntPtr fieldId)
		{
			return SafeJniCall(_env.GetDelegate<GetObjectField>().Invoke, obj, fieldId);
		}

		public IntPtr GetMethodID(IntPtr classP, string name, string sig)
		{
			return SafeJniCall(_env.GetDelegate<GetMethodID>().Invoke, classP, name, sig);
		}

		public IntPtr GetStaticMethodID(IntPtr classP, string name, string sig)
		{
			return SafeJniCall(_env.GetDelegate<GetStaticMethodID>().Invoke, classP, name, sig);
		}

		public byte CallByteMethod(IntPtr obj, IntPtr methodId, params JValue[] args)
		{
			return SafeJniCall(_env.GetDelegate<CallByteMethodA>().Invoke, obj, methodId, args);
		}

		public short CallShortMethod(IntPtr obj, IntPtr methodId, params JValue[] args)
		{
			return SafeJniCall(_env.GetDelegate<CallShortMethodA>().Invoke, obj, methodId, args);
		}

		public int CallIntMethod(IntPtr obj, IntPtr methodId, params JValue[] args)
		{
			return SafeJniCall(_env.GetDelegate<CallIntMethodA>().Invoke, obj, methodId, args);
		}

		public long CallLongMethod(IntPtr obj, IntPtr methodId, params JValue[] args)
		{
			return SafeJniCall(_env.GetDelegate<CallLongMethodA>().Invoke, obj, methodId, args);
		}

		public char CallCharMethod(IntPtr obj, IntPtr methodId, params JValue[] args)
		{
			return SafeJniCall(_env.GetDelegate<CallCharMethodA>().Invoke, obj, methodId, args);
		}

		public float CallFloatMethod(IntPtr obj, IntPtr methodId, params JValue[] args)
		{
			return SafeJniCall(_env.GetDelegate<CallFloatMethodA>().Invoke, obj, methodId, args);
		}

		public double CallDoubleMethod(IntPtr obj, IntPtr methodId, params JValue[] args)
		{
			return SafeJniCall(_env.GetDelegate<CallDoubleMethodA>().Invoke, obj, methodId, args);
		}

		public bool CallBooleanMethod(IntPtr obj, IntPtr methodId, params JValue[] args)
		{
			return SafeJniCall(_env.GetDelegate<CallBooleanMethodA>().Invoke, obj, methodId, args);
		}

		public IntPtr CallObjectMethod(IntPtr obj, IntPtr methodId, params JValue[] args)
		{
			return SafeJniCall(_env.GetDelegate<CallObjectMethodA>().Invoke, obj, methodId, args);
		}

		public void CallVoidMethod(IntPtr obj, IntPtr methodId, params JValue[] args)
		{
			SafeJniCallA(_env.GetDelegate<CallVoidMethodA>().Invoke, obj, methodId, args);
		}

		public IntPtr NewObject(IntPtr classP, IntPtr methodId, params JValue[] args)
		{
			return SafeJniCall(_env.GetDelegate<NewObjectA>().Invoke, classP, methodId, args);
		}

		public IntPtr NewStringUTF(string utf)
		{
			return SafeJniCall(_env.GetDelegate<NewStringUTF>().Invoke, utf);
		}

		public IntPtr NewString(string s)
		{
			byte[] bytes = Encoding.Unicode.GetBytes(s);
			unsafe
			{
				fixed (byte* buf = bytes)
				{
					// The NewString len parameter expects the length of an array of 2-byte characters, which
					// is half the length of an array of bytes.
					return SafeJniCall(_env.GetDelegate<NewString>().Invoke, new IntPtr(buf), bytes.Length / 2);
				}
			}
		}

		public byte CallStaticByteMethod(IntPtr classP, IntPtr methodId, params JValue[] args)
		{
			return SafeJniCall(_env.GetDelegate<CallStaticByteMethodA>().Invoke, classP, methodId, args);
		}

		public short CallStaticShortMethod(IntPtr classP, IntPtr methodId, params JValue[] args)
		{
			return SafeJniCall(_env.GetDelegate<CallStaticShortMethodA>().Invoke, classP, methodId, args);
		}

		public int CallStaticIntMethod(IntPtr classP, IntPtr methodId, params JValue[] args)
		{
			return SafeJniCall(_env.GetDelegate<CallStaticIntMethodA>().Invoke, classP, methodId, args);
		}

		public long CallStaticLongMethod(IntPtr classP, IntPtr methodId, params JValue[] args)
		{
			return SafeJniCall(_env.GetDelegate<CallStaticLongMethodA>().Invoke, classP, methodId, args);
		}

		public char CallStaticCharMethod(IntPtr classP, IntPtr methodId, params JValue[] args)
		{
			return SafeJniCall(_env.GetDelegate<CallStaticCharMethodA>().Invoke, classP, methodId, args);
		}

		public float CallStaticFloatMethod(IntPtr classP, IntPtr methodId, params JValue[] args)
		{
			return SafeJniCall(_env.GetDelegate<CallStaticFloatMethodA>().Invoke, classP, methodId, args);
		}

		public double CallStaticDoubleMethod(IntPtr classP, IntPtr methodId, params JValue[] args)
		{
			return SafeJniCall(_env.GetDelegate<CallStaticDoubleMethodA>().Invoke, classP, methodId, args);
		}

		public bool CallStaticBooleanMethod(IntPtr classP, IntPtr methodId, params JValue[] args)
		{
			return SafeJniCall(_env.GetDelegate<CallStaticBooleanMethodA>().Invoke, classP, methodId, args);
		}

		public void CallStaticVoidMethod(IntPtr classP, IntPtr methodId, params JValue[] args)
		{
			SafeJniCallA(_env.GetDelegate<CallStaticVoidMethodA>().Invoke, classP, methodId, args);
		}

		public IntPtr CallStaticObjectMethod(IntPtr classP, IntPtr methodId, params JValue[] args)
		{
			return SafeJniCall(_env.GetDelegate<CallStaticObjectMethodA>().Invoke, classP, methodId, args);
		}

		public IntPtr GetObjectClass(IntPtr obj)
		{
			return SafeJniCall( _env.GetDelegate<GetObjectClass>().Invoke, obj);
		}

		public string GetString(IntPtr stringPointer)
		{
			IntPtr stringChars = SafeJniCall(_env.GetDelegate<GetStringChars>().Invoke, stringPointer, IntPtr.Zero);
			try
			{
				return Marshal.PtrToStringUni(stringChars);
			}
			finally
			{
				SafeJniCallA(_env.GetDelegate<ReleaseStringChars>().Invoke, stringPointer, stringChars);
			}
		}

		private T[] GetPrimitiveArray<T>(IntPtr arrayPtr, Action<IntPtr, IntPtr, int, int, T[]> func)
		{
			int length = SafeJniCall(_env.GetDelegate<GetArrayLength>().Invoke, arrayPtr);
			T[] ret = new T[length];
			SafeJniCallA(func, arrayPtr, 0, length, ret);
			return ret;
		}

		public int GetArrayLength(IntPtr arrayPtr)
		{
			return SafeJniCall(_env.GetDelegate<GetArrayLength>().Invoke, arrayPtr);
		}

		public bool[] GetBooleanArray(IntPtr arrayPtr)
		{
			return GetPrimitiveArray<byte>(arrayPtr, _env.GetDelegate<GetBooleanArrayRegion>().Invoke).Select(b => b != 0).ToArray();
		}

		public byte[] GetByteArray(IntPtr arrayPtr)
		{
			return GetPrimitiveArray<byte>(arrayPtr, _env.GetDelegate<GetByteArrayRegion>().Invoke);
		}

		public char[] GetCharArray(IntPtr arrayPtr)
		{
			return GetPrimitiveArray<ushort>(arrayPtr, _env.GetDelegate<GetCharArrayRegion>().Invoke).Select(s => (char)s).ToArray();
		}

		public short[] GetShortArray(IntPtr arrayPtr)
		{
			return GetPrimitiveArray<short>(arrayPtr, _env.GetDelegate<GetShortArrayRegion>().Invoke);
		}

		public int[] GetIntArray(IntPtr arrayPtr)
		{
			return GetPrimitiveArray<int>(arrayPtr, _env.GetDelegate<GetIntArrayRegion>().Invoke);
		}

		public long[] GetLongArray(IntPtr arrayPtr)
		{
			return GetPrimitiveArray<long>(arrayPtr, _env.GetDelegate<GetLongArrayRegion>().Invoke);
		}

		public float[] GetFloatArray(IntPtr arrayPtr)
		{
			return GetPrimitiveArray<float>(arrayPtr, _env.GetDelegate<GetFloatArrayRegion>().Invoke);
		}

		public double[] GetDoubleArray(IntPtr arrayPtr)
		{
			return GetPrimitiveArray<double>(arrayPtr, _env.GetDelegate<GetDoubleArrayRegion>().Invoke);
		}

		public IntPtr[] GetArray(IntPtr arrayPtr)
		{
			int length = SafeJniCall(_env.GetDelegate<GetArrayLength>().Invoke, arrayPtr);
			IntPtr[] ret = new IntPtr[length];
			for (int i = 0; i < length; i++)
			{
				ret[i] = SafeJniCall(_env.GetDelegate<GetObjectArrayElement>().Invoke, arrayPtr, i);
			}
			return ret;
		}

		public IntPtr NewBooleanArray(bool[] elements)
		{
			IntPtr ret = SafeJniCall(_env.GetDelegate<NewBooleanArray>().Invoke, elements.Length);
			SafeJniCallA(_env.GetDelegate<SetBooleanArrayRegion>().Invoke, ret, 0, elements.Length, elements.Select(b => b ? (byte)1 : (byte)0).ToArray());
			return ret;
		}

		public IntPtr NewByteArray(byte[] elements)
		{
			IntPtr ret = SafeJniCall(_env.GetDelegate<NewByteArray>().Invoke, elements.Length);
			SafeJniCallA(_env.GetDelegate<SetByteArrayRegion>().Invoke, ret, 0, elements.Length, elements);
			return ret;
		}

		public IntPtr NewCharArray(char[] elements)
		{
			IntPtr ret = SafeJniCall(_env.GetDelegate<NewCharArray>().Invoke, elements.Length);
			SafeJniCallA(_env.GetDelegate<SetCharArrayRegion>().Invoke, ret, 0, elements.Length, elements.Select(c => (ushort)c).ToArray());
			return ret;
		}

		public IntPtr NewShortArray(short[] elements)
		{
			IntPtr ret = SafeJniCall(_env.GetDelegate<NewShortArray>().Invoke, elements.Length);
			SafeJniCallA(_env.GetDelegate<SetShortArrayRegion>().Invoke, ret, 0, elements.Length, elements);
			return ret;
		}

		public IntPtr NewIntArray(int[] elements)
		{
			IntPtr ret = SafeJniCall(_env.GetDelegate<NewIntArray>().Invoke, elements.Length);
			SafeJniCallA(_env.GetDelegate<SetIntArrayRegion>().Invoke, ret, 0, elements.Length, elements);
			return ret;
		}

		public IntPtr NewLongArray(long[] elements)
		{
			IntPtr ret = SafeJniCall(_env.GetDelegate<NewLongArray>().Invoke, elements.Length);
			SafeJniCallA(_env.GetDelegate<SetLongArrayRegion>().Invoke, ret, 0, elements.Length, elements);
			return ret;
		}

		public IntPtr NewFloatArray(float[] elements)
		{
			IntPtr ret = SafeJniCall(_env.GetDelegate<NewFloatArray>().Invoke, elements.Length);
			SafeJniCallA(_env.GetDelegate<SetFloatArrayRegion>().Invoke, ret, 0, elements.Length, elements);
			return ret;
		}

		public IntPtr NewDoubleArray(double[] elements)
		{
			IntPtr ret = SafeJniCall(_env.GetDelegate<NewDoubleArray>().Invoke, elements.Length);
			SafeJniCallA(_env.GetDelegate<SetDoubleArrayRegion>().Invoke, ret, 0, elements.Length, elements);
			return ret;
		}

		public IntPtr NewArray(IntPtr classP, IntPtr[] elements)
		{
			IntPtr ret = SafeJniCall(_env.GetDelegate<NewObjectArray>().Invoke, elements.Length, classP, IntPtr.Zero);
			for (int i = 0; i < elements.Length; i++)
			{
				SafeJniCallA(_env.GetDelegate<SetObjectArrayElement>().Invoke, ret, i, elements[i]);
			}
			return ret;
		}

		private A SafeJniCall<A>(Func<IntPtr, A> func)
		{
			return SafeJniCall(() => func(_currentThreadEnv.Value));
		}

		private B SafeJniCall<A, B>(Func<IntPtr, A, B> func, A arg1)
		{
			return SafeJniCall(() => func(_currentThreadEnv.Value, arg1));
		}

		private C SafeJniCall<A, B, C>(Func<IntPtr, A, B, C> func, A arg1, B arg2)
		{
			return SafeJniCall(() => func(_currentThreadEnv.Value, arg1, arg2));
		}

		private D SafeJniCall<A, B, C, D>(Func<IntPtr, A, B, C, D> func, A arg1, B arg2, C arg3)
		{
			return SafeJniCall(() => func(_currentThreadEnv.Value, arg1, arg2, arg3));
		}

		private void SafeJniCallA(Action<IntPtr> func)
		{
			SafeJniCall<object>(() => { func(_currentThreadEnv.Value); return null; });
		}

		private void SafeJniCallA<A>(Action<IntPtr, A> func, A arg1)
		{
			SafeJniCall<object>(() => { func(_currentThreadEnv.Value, arg1); return null; });
		}

		private void SafeJniCallA<A, B>(Action<IntPtr, A, B> func, A arg1, B arg2)
		{
			SafeJniCall<object>(() => { func(_currentThreadEnv.Value, arg1, arg2); return null; });
		}

		private void SafeJniCallA<A, B, C>(Action<IntPtr, A, B, C> func, A arg1, B arg2, C arg3)
		{
			SafeJniCall<object>(() => { func(_currentThreadEnv.Value, arg1, arg2, arg3); return null; });
		}

		private void SafeJniCallA<A, B, C, D>(Action<IntPtr, A, B, C, D> func, A arg1, B arg2, C arg3, D arg4)
		{
			SafeJniCall<object>(() => { func(_currentThreadEnv.Value, arg1, arg2, arg3, arg4); return null; });
		}

		private T SafeJniCall<T>(Func<T> func)
		{
			_env.GetDelegate<ExceptionClear>()(_currentThreadEnv.Value);
			T ret = func();
			if (_env.GetDelegate<ExceptionCheck>()(_currentThreadEnv.Value))
			{
				IntPtr exception = _env.GetDelegate<ExceptionOccurred>()(_currentThreadEnv.Value);
				_env.GetDelegate<ExceptionClear>()(_currentThreadEnv.Value);
				string exceptionType = GetExceptionType(exception);
				JThrowableClass throwableClass = new JThrowableClass(this);
				string exceptionMessage = throwableClass.GetMessage(exception);
				exceptionMessage = AppendCause(exceptionMessage, exception, throwableClass);
				throw new JavaException(exceptionType + (String.IsNullOrEmpty(exceptionMessage) ? "" : ": " + exceptionMessage));
			}
			return ret;
		}

		private string GetExceptionType(IntPtr exception)
		{
			IntPtr exceptionTypePtr = new JObjectClass(this).GetClass(exception);
			return new JClassClass(this).GetName(exceptionTypePtr);
		}

		private string AppendCause(string exceptionMessage, IntPtr exception, JThrowableClass throwableClass)
		{
			IntPtr cause = throwableClass.GetCause(exception);
			if (cause == IntPtr.Zero)
			{
				return exceptionMessage;
			}
			else
			{
				string causeType = GetExceptionType(cause);
				string causeMessage = throwableClass.GetMessage(exception);
				if (causeMessage != null)
				{
					exceptionMessage = (exceptionMessage ?? "") + ". Caused by: " + causeType + ": " + causeMessage;
				}
				return AppendCause(exceptionMessage, cause, throwableClass);
			}
		}

		public void DetachThread()
		{
			_vm.GetDelegate<DetachCurrentThread>()(_vmP);
			_currentThreadEnv.Value = IntPtr.Zero;
		}

		public void Dispose()
		{
			if (_vmDllModule != IntPtr.Zero && !_vmReuse)
			{
				int ret = _vm.GetDelegate<DestroyJavaVM>()(_vmP);
				if (ret != 0)
				{
					throw new InvalidOperationException("JavaVM destruction failed - return value obtained: " + ret);
				}
				if (!FreeLibrary(_vmDllModule))
				{
					throw new Win32Exception();
				}
				_vmDllModule = IntPtr.Zero;
			}
		}
	}
}
