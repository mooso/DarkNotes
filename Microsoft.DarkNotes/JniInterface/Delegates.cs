using System;
using System.Runtime.InteropServices;

namespace DarkNotes.JniInterface
{
	delegate int DestroyJavaVM(IntPtr vm);
	delegate int AttachCurrentThread(IntPtr vm, IntPtr penv, IntPtr args);
	delegate int DetachCurrentThread(IntPtr vm);
	delegate int GetEnv(IntPtr vm, IntPtr penv, int version);
	delegate int AttachCurrentThreadAsDaemon(IntPtr vm, IntPtr penv, IntPtr args);


	delegate int GetVersion(IntPtr env);

	delegate IntPtr DefineClass
			(IntPtr env, [MarshalAs(UnmanagedType.LPStr)] string name, IntPtr loader, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 4)] byte[] buf,
		int len);
	delegate IntPtr FindClass
			(IntPtr env, [MarshalAs(UnmanagedType.LPStr)] string name);

	delegate IntPtr FromReflectedMethod
			(IntPtr env, IntPtr method);
	delegate IntPtr FromReflectedField
			(IntPtr env, IntPtr field);

	delegate IntPtr ToReflectedMethod
			(IntPtr env, IntPtr cls, IntPtr methodID, [MarshalAs(UnmanagedType.U1)] bool isStatic);

	delegate IntPtr GetSuperclass
			(IntPtr env, IntPtr sub);

	[return: MarshalAs(UnmanagedType.U1)]
	delegate bool IsAssignableFrom
			(IntPtr env, IntPtr sub, IntPtr sup);

	delegate IntPtr ToReflectedField
			(IntPtr env, IntPtr cls, IntPtr fieldID, [MarshalAs(UnmanagedType.U1)] bool isStatic);

	delegate int Throw
			(IntPtr env, IntPtr obj);
	delegate int ThrowNew
			(IntPtr env, IntPtr clazz, [MarshalAs(UnmanagedType.LPStr)] string msg);
	delegate IntPtr ExceptionOccurred
			(IntPtr env);
	delegate void ExceptionDescribe
			(IntPtr env);
	delegate void ExceptionClear
			(IntPtr env);
	delegate void FatalError
			(IntPtr env, [MarshalAs(UnmanagedType.LPStr)] string msg);

	delegate int PushLocalFrame
			(IntPtr env, int capacity);
	delegate IntPtr PopLocalFrame
			(IntPtr env, IntPtr result);

	delegate IntPtr NewGlobalRef
			(IntPtr env, IntPtr lobj);
	delegate void DeleteGlobalRef
			(IntPtr env, IntPtr gref);
	delegate void DeleteLocalRef
			(IntPtr env, IntPtr obj);
	[return: MarshalAs(UnmanagedType.U1)]
	delegate bool IsSameObject
			(IntPtr env, IntPtr obj1, IntPtr obj2);
	delegate IntPtr NewLocalRef
			(IntPtr env, IntPtr reference);
	delegate int EnsureLocalCapacity
			(IntPtr env, int capacity);

	delegate IntPtr AllocObject
			(IntPtr env, IntPtr clazz);
	delegate IntPtr NewObject
			(IntPtr env, IntPtr clazz, IntPtr methodID, params JValue[] args);
	delegate IntPtr NewObjectV
			(IntPtr env, IntPtr clazz, IntPtr methodID, IntPtr args);
	delegate IntPtr NewObjectA
			(IntPtr env, IntPtr clazz, IntPtr methodID, JValue[] args);

	delegate IntPtr GetObjectClass
			(IntPtr env, IntPtr obj);
	[return: MarshalAs(UnmanagedType.U1)]
	delegate bool IsInstanceOf
			(IntPtr env, IntPtr obj, IntPtr clazz);

	delegate IntPtr GetMethodID
			(IntPtr env, IntPtr clazz, [MarshalAs(UnmanagedType.LPStr)] string name, [MarshalAs(UnmanagedType.LPStr)] string sig);

	delegate IntPtr CallObjectMethod
			(IntPtr env, IntPtr obj, IntPtr methodID, params JValue[] args);
	delegate IntPtr CallObjectMethodV
			(IntPtr env, IntPtr obj, IntPtr methodID, IntPtr args);
	delegate IntPtr CallObjectMethodA
			(IntPtr env, IntPtr obj, IntPtr methodID, JValue[] args);

	[return: MarshalAs(UnmanagedType.U1)]
	delegate bool CallBooleanMethod
			(IntPtr env, IntPtr obj, IntPtr methodID, params JValue[] args);
	[return: MarshalAs(UnmanagedType.U1)]
	delegate bool CallBooleanMethodV
			(IntPtr env, IntPtr obj, IntPtr methodID, IntPtr args);
	[return: MarshalAs(UnmanagedType.U1)]
	delegate bool CallBooleanMethodA
			(IntPtr env, IntPtr obj, IntPtr methodID, JValue[] args);

	delegate byte CallByteMethod
			(IntPtr env, IntPtr obj, IntPtr methodID, params JValue[] args);
	delegate byte CallByteMethodV
			(IntPtr env, IntPtr obj, IntPtr methodID, IntPtr args);
	delegate byte CallByteMethodA
			(IntPtr env, IntPtr obj, IntPtr methodID, JValue[] args);

	delegate char CallCharMethod
			(IntPtr env, IntPtr obj, IntPtr methodID, params JValue[] args);
	delegate char CallCharMethodV
			(IntPtr env, IntPtr obj, IntPtr methodID, IntPtr args);
	delegate char CallCharMethodA
			(IntPtr env, IntPtr obj, IntPtr methodID, JValue[] args);

	delegate short CallShortMethod
			(IntPtr env, IntPtr obj, IntPtr methodID, params JValue[] args);
	delegate short CallShortMethodV
			(IntPtr env, IntPtr obj, IntPtr methodID, IntPtr args);
	delegate short CallShortMethodA
			(IntPtr env, IntPtr obj, IntPtr methodID, JValue[] args);

	delegate int CallIntMethod
			(IntPtr env, IntPtr obj, IntPtr methodID, params JValue[] args);
	delegate int CallIntMethodV
			(IntPtr env, IntPtr obj, IntPtr methodID, IntPtr args);
	delegate int CallIntMethodA
			(IntPtr env, IntPtr obj, IntPtr methodID, JValue[] args);

	delegate long CallLongMethod
			(IntPtr env, IntPtr obj, IntPtr methodID, params JValue[] args);
	delegate long CallLongMethodV
			(IntPtr env, IntPtr obj, IntPtr methodID, IntPtr args);
	delegate long CallLongMethodA
			(IntPtr env, IntPtr obj, IntPtr methodID, JValue[] args);

	delegate float CallFloatMethod
			(IntPtr env, IntPtr obj, IntPtr methodID, params JValue[] args);
	delegate float CallFloatMethodV
			(IntPtr env, IntPtr obj, IntPtr methodID, IntPtr args);
	delegate float CallFloatMethodA
			(IntPtr env, IntPtr obj, IntPtr methodID, JValue[] args);

	delegate double CallDoubleMethod
			(IntPtr env, IntPtr obj, IntPtr methodID, params JValue[] args);
	delegate double CallDoubleMethodV
			(IntPtr env, IntPtr obj, IntPtr methodID, IntPtr args);
	delegate double CallDoubleMethodA
			(IntPtr env, IntPtr obj, IntPtr methodID, JValue[] args);

	delegate void CallVoidMethod
			(IntPtr env, IntPtr obj, IntPtr methodID, params JValue[] args);
	delegate void CallVoidMethodV
			(IntPtr env, IntPtr obj, IntPtr methodID, IntPtr args);
	delegate void CallVoidMethodA
			(IntPtr env, IntPtr obj, IntPtr methodID, JValue[] args);

	delegate IntPtr CallNonvirtualObjectMethod
			(IntPtr env, IntPtr obj, IntPtr clazz, IntPtr methodID, params JValue[] args);
	delegate IntPtr CallNonvirtualObjectMethodV
			(IntPtr env, IntPtr obj, IntPtr clazz, IntPtr methodID,
		IntPtr args);
	delegate IntPtr CallNonvirtualObjectMethodA
			(IntPtr env, IntPtr obj, IntPtr clazz, IntPtr methodID,
		JValue[] args);

	[return: MarshalAs(UnmanagedType.U1)]
	delegate bool CallNonvirtualBooleanMethod
			(IntPtr env, IntPtr obj, IntPtr clazz, IntPtr methodID, params JValue[] args);
	[return: MarshalAs(UnmanagedType.U1)]
	delegate bool CallNonvirtualBooleanMethodV
			(IntPtr env, IntPtr obj, IntPtr clazz, IntPtr methodID,
		IntPtr args);
	[return: MarshalAs(UnmanagedType.U1)]
	delegate bool CallNonvirtualBooleanMethodA
			(IntPtr env, IntPtr obj, IntPtr clazz, IntPtr methodID,
		JValue[] args);

	delegate byte CallNonvirtualByteMethod
			(IntPtr env, IntPtr obj, IntPtr clazz, IntPtr methodID, params JValue[] args);
	delegate byte CallNonvirtualByteMethodV
			(IntPtr env, IntPtr obj, IntPtr clazz, IntPtr methodID,
		IntPtr args);
	delegate byte CallNonvirtualByteMethodA
			(IntPtr env, IntPtr obj, IntPtr clazz, IntPtr methodID,
		JValue[] args);

	delegate ushort CallNonvirtualCharMethod
			(IntPtr env, IntPtr obj, IntPtr clazz, IntPtr methodID, params JValue[] args);
	delegate ushort CallNonvirtualCharMethodV
			(IntPtr env, IntPtr obj, IntPtr clazz, IntPtr methodID,
		IntPtr args);
	delegate ushort CallNonvirtualCharMethodA
			(IntPtr env, IntPtr obj, IntPtr clazz, IntPtr methodID,
		JValue[] args);

	delegate short CallNonvirtualShortMethod
			(IntPtr env, IntPtr obj, IntPtr clazz, IntPtr methodID, params JValue[] args);
	delegate short CallNonvirtualShortMethodV
			(IntPtr env, IntPtr obj, IntPtr clazz, IntPtr methodID,
		IntPtr args);
	delegate short CallNonvirtualShortMethodA
			(IntPtr env, IntPtr obj, IntPtr clazz, IntPtr methodID,
		JValue[] args);

	delegate int CallNonvirtualIntMethod
			(IntPtr env, IntPtr obj, IntPtr clazz, IntPtr methodID, params JValue[] args);
	delegate int CallNonvirtualIntMethodV
			(IntPtr env, IntPtr obj, IntPtr clazz, IntPtr methodID,
		IntPtr args);
	delegate int CallNonvirtualIntMethodA
			(IntPtr env, IntPtr obj, IntPtr clazz, IntPtr methodID,
		JValue[] args);

	delegate long CallNonvirtualLongMethod
			(IntPtr env, IntPtr obj, IntPtr clazz, IntPtr methodID, params JValue[] args);
	delegate long CallNonvirtualLongMethodV
			(IntPtr env, IntPtr obj, IntPtr clazz, IntPtr methodID,
		IntPtr args);
	delegate long CallNonvirtualLongMethodA
			(IntPtr env, IntPtr obj, IntPtr clazz, IntPtr methodID,
		JValue[] args);

	delegate float CallNonvirtualFloatMethod
			(IntPtr env, IntPtr obj, IntPtr clazz, IntPtr methodID, params JValue[] args);
	delegate float CallNonvirtualFloatMethodV
			(IntPtr env, IntPtr obj, IntPtr clazz, IntPtr methodID,
		IntPtr args);
	delegate float CallNonvirtualFloatMethodA
			(IntPtr env, IntPtr obj, IntPtr clazz, IntPtr methodID,
		JValue[] args);

	delegate double CallNonvirtualDoubleMethod
			(IntPtr env, IntPtr obj, IntPtr clazz, IntPtr methodID, params JValue[] args);
	delegate double CallNonvirtualDoubleMethodV
			(IntPtr env, IntPtr obj, IntPtr clazz, IntPtr methodID,
		IntPtr args);
	delegate double CallNonvirtualDoubleMethodA
			(IntPtr env, IntPtr obj, IntPtr clazz, IntPtr methodID,
		JValue[] args);

	delegate void CallNonvirtualVoidMethod
			(IntPtr env, IntPtr obj, IntPtr clazz, IntPtr methodID, params JValue[] args);
	delegate void CallNonvirtualVoidMethodV
			(IntPtr env, IntPtr obj, IntPtr clazz, IntPtr methodID,
		IntPtr args);
	delegate void CallNonvirtualVoidMethodA
			(IntPtr env, IntPtr obj, IntPtr clazz, IntPtr methodID,
		JValue[] args);

	delegate IntPtr GetFieldID
			(IntPtr env, IntPtr clazz, [MarshalAs(UnmanagedType.LPStr)] string name, [MarshalAs(UnmanagedType.LPStr)] string sig);

	delegate IntPtr GetObjectField
			(IntPtr env, IntPtr obj, IntPtr fieldID);
	[return: MarshalAs(UnmanagedType.U1)]
	delegate bool GetBooleanField
			(IntPtr env, IntPtr obj, IntPtr fieldID);
	delegate byte GetByteField
			(IntPtr env, IntPtr obj, IntPtr fieldID);
	delegate char GetCharField
			(IntPtr env, IntPtr obj, IntPtr fieldID);
	delegate short GetShortField
			(IntPtr env, IntPtr obj, IntPtr fieldID);
	delegate int GetIntField
			(IntPtr env, IntPtr obj, IntPtr fieldID);
	delegate long GetLongField
			(IntPtr env, IntPtr obj, IntPtr fieldID);
	delegate float GetFloatField
			(IntPtr env, IntPtr obj, IntPtr fieldID);
	delegate double GetDoubleField
			(IntPtr env, IntPtr obj, IntPtr fieldID);

	delegate void SetObjectField
			(IntPtr env, IntPtr obj, IntPtr fieldID, IntPtr val);
	delegate void SetBooleanField
			(IntPtr env, IntPtr obj, IntPtr fieldID, [MarshalAs(UnmanagedType.U1)] bool val);
	delegate void SetByteField
			(IntPtr env, IntPtr obj, IntPtr fieldID, byte val);
	delegate void SetCharField
			(IntPtr env, IntPtr obj, IntPtr fieldID, char val);
	delegate void SetShortField
			(IntPtr env, IntPtr obj, IntPtr fieldID, short val);
	delegate void SetIntField
			(IntPtr env, IntPtr obj, IntPtr fieldID, int val);
	delegate void SetLongField
			(IntPtr env, IntPtr obj, IntPtr fieldID, long val);
	delegate void SetFloatField
			(IntPtr env, IntPtr obj, IntPtr fieldID, float val);
	delegate void SetDoubleField
			(IntPtr env, IntPtr obj, IntPtr fieldID, double val);

	delegate IntPtr GetStaticMethodID
			(IntPtr env, IntPtr clazz, [MarshalAs(UnmanagedType.LPStr)] string name, [MarshalAs(UnmanagedType.LPStr)] string sig);

	delegate IntPtr CallStaticObjectMethod
			(IntPtr env, IntPtr clazz, IntPtr methodID, params JValue[] args);
	delegate IntPtr CallStaticObjectMethodV
			(IntPtr env, IntPtr clazz, IntPtr methodID, IntPtr args);
	delegate IntPtr CallStaticObjectMethodA
			(IntPtr env, IntPtr clazz, IntPtr methodID, JValue[] args);

	[return: MarshalAs(UnmanagedType.U1)]
	delegate bool CallStaticBooleanMethod
			(IntPtr env, IntPtr clazz, IntPtr methodID, params JValue[] args);
	[return: MarshalAs(UnmanagedType.U1)]
	delegate bool CallStaticBooleanMethodV
			(IntPtr env, IntPtr clazz, IntPtr methodID, IntPtr args);
	[return: MarshalAs(UnmanagedType.U1)]
	delegate bool CallStaticBooleanMethodA
			(IntPtr env, IntPtr clazz, IntPtr methodID, JValue[] args);

	delegate byte CallStaticByteMethod
			(IntPtr env, IntPtr clazz, IntPtr methodID, params JValue[] args);
	delegate byte CallStaticByteMethodV
			(IntPtr env, IntPtr clazz, IntPtr methodID, IntPtr args);
	delegate byte CallStaticByteMethodA
			(IntPtr env, IntPtr clazz, IntPtr methodID, JValue[] args);

	delegate char CallStaticCharMethod
			(IntPtr env, IntPtr clazz, IntPtr methodID, params JValue[] args);
	delegate char CallStaticCharMethodV
			(IntPtr env, IntPtr clazz, IntPtr methodID, IntPtr args);
	delegate char CallStaticCharMethodA
			(IntPtr env, IntPtr clazz, IntPtr methodID, JValue[] args);

	delegate short CallStaticShortMethod
			(IntPtr env, IntPtr clazz, IntPtr methodID, params JValue[] args);
	delegate short CallStaticShortMethodV
			(IntPtr env, IntPtr clazz, IntPtr methodID, IntPtr args);
	delegate short CallStaticShortMethodA
			(IntPtr env, IntPtr clazz, IntPtr methodID, JValue[] args);

	delegate int CallStaticIntMethod
			(IntPtr env, IntPtr clazz, IntPtr methodID, params JValue[] args);
	delegate int CallStaticIntMethodV
			(IntPtr env, IntPtr clazz, IntPtr methodID, IntPtr args);
	delegate int CallStaticIntMethodA
			(IntPtr env, IntPtr clazz, IntPtr methodID, JValue[] args);

	delegate long CallStaticLongMethod
			(IntPtr env, IntPtr clazz, IntPtr methodID, params JValue[] args);
	delegate long CallStaticLongMethodV
			(IntPtr env, IntPtr clazz, IntPtr methodID, IntPtr args);
	delegate long CallStaticLongMethodA
			(IntPtr env, IntPtr clazz, IntPtr methodID, JValue[] args);

	delegate float CallStaticFloatMethod
			(IntPtr env, IntPtr clazz, IntPtr methodID, params JValue[] args);
	delegate float CallStaticFloatMethodV
			(IntPtr env, IntPtr clazz, IntPtr methodID, IntPtr args);
	delegate float CallStaticFloatMethodA
			(IntPtr env, IntPtr clazz, IntPtr methodID, JValue[] args);

	delegate double CallStaticDoubleMethod
			(IntPtr env, IntPtr clazz, IntPtr methodID, params JValue[] args);
	delegate double CallStaticDoubleMethodV
			(IntPtr env, IntPtr clazz, IntPtr methodID, IntPtr args);
	delegate double CallStaticDoubleMethodA
			(IntPtr env, IntPtr clazz, IntPtr methodID, JValue[] args);

	delegate void CallStaticVoidMethod
			(IntPtr env, IntPtr cls, IntPtr methodID, params JValue[] args);
	delegate void CallStaticVoidMethodV
			(IntPtr env, IntPtr cls, IntPtr methodID, IntPtr args);
	delegate void CallStaticVoidMethodA
			(IntPtr env, IntPtr cls, IntPtr methodID, JValue[] args);

	delegate IntPtr GetStaticFieldID
			(IntPtr env, IntPtr clazz, [MarshalAs(UnmanagedType.LPStr)] string name, [MarshalAs(UnmanagedType.LPStr)] string sig);
	delegate IntPtr GetStaticObjectField
			(IntPtr env, IntPtr clazz, IntPtr fieldID);
	[return: MarshalAs(UnmanagedType.U1)]
	delegate bool GetStaticBooleanField
			(IntPtr env, IntPtr clazz, IntPtr fieldID);
	delegate byte GetStaticByteField
			(IntPtr env, IntPtr clazz, IntPtr fieldID);
	delegate char GetStaticCharField
			(IntPtr env, IntPtr clazz, IntPtr fieldID);
	delegate short GetStaticShortField
			(IntPtr env, IntPtr clazz, IntPtr fieldID);
	delegate int GetStaticIntField
			(IntPtr env, IntPtr clazz, IntPtr fieldID);
	delegate long GetStaticLongField
			(IntPtr env, IntPtr clazz, IntPtr fieldID);
	delegate float GetStaticFloatField
			(IntPtr env, IntPtr clazz, IntPtr fieldID);
	delegate double GetStaticDoubleField
			(IntPtr env, IntPtr clazz, IntPtr fieldID);

	delegate void SetStaticObjectField
			(IntPtr env, IntPtr clazz, IntPtr fieldID, IntPtr value);
	delegate void SetStaticBooleanField
			(IntPtr env, IntPtr clazz, IntPtr fieldID, [MarshalAs(UnmanagedType.U1)] bool value);
	delegate void SetStaticByteField
			(IntPtr env, IntPtr clazz, IntPtr fieldID, byte value);
	delegate void SetStaticCharField
			(IntPtr env, IntPtr clazz, IntPtr fieldID, char value);
	delegate void SetStaticShortField
			(IntPtr env, IntPtr clazz, IntPtr fieldID, short value);
	delegate void SetStaticIntField
			(IntPtr env, IntPtr clazz, IntPtr fieldID, int value);
	delegate void SetStaticLongField
			(IntPtr env, IntPtr clazz, IntPtr fieldID, long value);
	delegate void SetStaticFloatField
			(IntPtr env, IntPtr clazz, IntPtr fieldID, float value);
	delegate void SetStaticDoubleField
			(IntPtr env, IntPtr clazz, IntPtr fieldID, double value);

	delegate IntPtr NewString
			(IntPtr env, IntPtr unicode, int len);
	delegate int GetStringLength
			(IntPtr env, IntPtr str);
	delegate IntPtr GetStringChars
			(IntPtr env, IntPtr str, IntPtr isCopy);
	delegate void ReleaseStringChars
			(IntPtr env, IntPtr str, IntPtr chars);

	delegate IntPtr NewStringUTF
			(IntPtr env, [MarshalAs(UnmanagedType.LPStr)] string utf);
	delegate int GetStringUTFLength
			(IntPtr env, IntPtr str);
	delegate IntPtr GetStringUTFChars
			(IntPtr env, IntPtr str, [MarshalAs(UnmanagedType.U1)] ref bool isCopy);
	delegate void ReleaseStringUTFChars
			(IntPtr env, IntPtr str, [MarshalAs(UnmanagedType.LPArray)] ushort[] chars);


	delegate int GetArrayLength
			(IntPtr env, IntPtr array);

	delegate IntPtr NewObjectArray
			(IntPtr env, int len, IntPtr clazz, IntPtr init);
	delegate IntPtr GetObjectArrayElement
			(IntPtr env, IntPtr array, int index);
	delegate void SetObjectArrayElement
			(IntPtr env, IntPtr array, int index, IntPtr val);

	delegate IntPtr NewBooleanArray
			(IntPtr env, int len);
	delegate IntPtr NewByteArray
			(IntPtr env, int len);
	delegate IntPtr NewCharArray
			(IntPtr env, int len);
	delegate IntPtr NewShortArray
			(IntPtr env, int len);
	delegate IntPtr NewIntArray
			(IntPtr env, int len);
	delegate IntPtr NewLongArray
			(IntPtr env, int len);
	delegate IntPtr NewFloatArray
			(IntPtr env, int len);
	delegate IntPtr NewDoubleArray
			(IntPtr env, int len);

	delegate IntPtr GetBooleanArrayElements
			(IntPtr env, IntPtr array, [MarshalAs(UnmanagedType.U1)] ref bool isCopy);
	delegate IntPtr GetByteArrayElements
			(IntPtr env, IntPtr array, [MarshalAs(UnmanagedType.U1)] ref bool isCopy);
	delegate IntPtr GetCharArrayElements
			(IntPtr env, IntPtr array, [MarshalAs(UnmanagedType.U1)] ref bool isCopy);
	delegate IntPtr GetShortArrayElements
			(IntPtr env, IntPtr array, [MarshalAs(UnmanagedType.U1)] ref bool isCopy);
	delegate IntPtr GetIntArrayElements
			(IntPtr env, IntPtr array, [MarshalAs(UnmanagedType.U1)] ref bool isCopy);
	delegate IntPtr GetLongArrayElements
			(IntPtr env, IntPtr array, [MarshalAs(UnmanagedType.U1)] ref bool isCopy);
	delegate IntPtr GetFloatArrayElements
			(IntPtr env, IntPtr array, [MarshalAs(UnmanagedType.U1)] ref bool isCopy);
	delegate IntPtr GetDoubleArrayElements
			(IntPtr env, IntPtr array, [MarshalAs(UnmanagedType.U1)] ref bool isCopy);

	delegate void ReleaseBooleanArrayElements
			(IntPtr env, IntPtr array, [MarshalAs(UnmanagedType.U1)] ref bool elems, int mode);
	delegate void ReleaseByteArrayElements
			(IntPtr env, IntPtr array, [MarshalAs(UnmanagedType.LPArray)] byte[] elems, int mode);
	delegate void ReleaseCharArrayElements
			(IntPtr env, IntPtr array, [MarshalAs(UnmanagedType.LPArray)] ushort[] elems, int mode);
	delegate void ReleaseShortArrayElements
			(IntPtr env, IntPtr array, [MarshalAs(UnmanagedType.LPArray)] short[] elems, int mode);
	delegate void ReleaseIntArrayElements
			(IntPtr env, IntPtr array, [MarshalAs(UnmanagedType.LPArray)] int[] elems, int mode);
	delegate void ReleaseLongArrayElements
			(IntPtr env, IntPtr array, [MarshalAs(UnmanagedType.LPArray)] long[] elems, int mode);
	delegate void ReleaseFloatArrayElements
			(IntPtr env, IntPtr array, [MarshalAs(UnmanagedType.LPArray)] float[] elems, int mode);
	delegate void ReleaseDoubleArrayElements
			(IntPtr env, IntPtr array, [MarshalAs(UnmanagedType.LPArray)] double[] elems, int mode);

	delegate void GetBooleanArrayRegion
			(IntPtr env, IntPtr array, int start, int l, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] byte[] buf);
	delegate void GetByteArrayRegion
			(IntPtr env, IntPtr array, int start, int len, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] byte[] buf);
	delegate void GetCharArrayRegion
			(IntPtr env, IntPtr array, int start, int len, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] ushort[] buf);
	delegate void GetShortArrayRegion
			(IntPtr env, IntPtr array, int start, int len, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] short[] buf);
	delegate void GetIntArrayRegion
			(IntPtr env, IntPtr array, int start, int len, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] int[] buf);
	delegate void GetLongArrayRegion
			(IntPtr env, IntPtr array, int start, int len, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] long[] buf);
	delegate void GetFloatArrayRegion
			(IntPtr env, IntPtr array, int start, int len, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] float[] buf);
	delegate void GetDoubleArrayRegion
			(IntPtr env, IntPtr array, int start, int len, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] double[] buf);

	delegate void SetBooleanArrayRegion
			(IntPtr env, IntPtr array, int start, int l, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] byte[] buf);
	delegate void SetByteArrayRegion
			(IntPtr env, IntPtr array, int start, int len, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] byte[] buf);
	delegate void SetCharArrayRegion
			(IntPtr env, IntPtr array, int start, int len, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] ushort[] buf);
	delegate void SetShortArrayRegion
			(IntPtr env, IntPtr array, int start, int len, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] short[] buf);
	delegate void SetIntArrayRegion
			(IntPtr env, IntPtr array, int start, int len, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] int[] buf);
	delegate void SetLongArrayRegion
			(IntPtr env, IntPtr array, int start, int len, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] long[] buf);
	delegate void SetFloatArrayRegion
			(IntPtr env, IntPtr array, int start, int len, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] float[] buf);
	delegate void SetDoubleArrayRegion
			(IntPtr env, IntPtr array, int start, int len, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] double[] buf);

	delegate int RegisterNatives
			(IntPtr env, IntPtr clazz, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] JNINativeMethod[] methods,
		int nMethods);
	delegate int UnregisterNatives
			(IntPtr env, IntPtr clazz);

	delegate int MonitorEnter
			(IntPtr env, IntPtr obj);
	delegate int MonitorExit
			(IntPtr env, IntPtr obj);

	delegate int GetJavaVM
			(IntPtr env, out JavaVM vm);

	delegate void GetStringRegion
			(IntPtr env, IntPtr str, int start, int len, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] ushort[] buf);
	delegate void GetStringUTFRegion
			(IntPtr env, IntPtr str, int start, int len, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] char[] buf);

	delegate IntPtr GetPrimitiveArrayCritical
			(IntPtr env, IntPtr array, [MarshalAs(UnmanagedType.U1)] ref bool isCopy);
	delegate void ReleasePrimitiveArrayCritical
			(IntPtr env, IntPtr array, IntPtr carray, int mode);

	[return: MarshalAs(UnmanagedType.LPStr)]
	delegate string GetStringCritical
			(IntPtr env, IntPtr str, [MarshalAs(UnmanagedType.U1)] ref bool isCopy);
	delegate void ReleaseStringCritical
			(IntPtr env, IntPtr str, [MarshalAs(UnmanagedType.LPStr)] string cstring);

	delegate IntPtr NewWeakGlobalRef
				(IntPtr env, IntPtr obj);
	delegate void DeleteWeakGlobalRef
				(IntPtr env, IntPtr r);

	[return: MarshalAs(UnmanagedType.U1)]
	delegate bool ExceptionCheck
				(IntPtr env);

	delegate IntPtr NewDirectByteBuffer
				(ref IntPtr env, IntPtr address, long capacity);
	delegate IntPtr GetDirectBufferAddress
				(ref IntPtr env, IntPtr buf);
	delegate long GetDirectBufferCapacity
				(ref IntPtr env, IntPtr buf);

	/* New JNI 1.6 Features */

	delegate jobjectRefType GetObjectRefType
				(IntPtr env, IntPtr obj);
}
