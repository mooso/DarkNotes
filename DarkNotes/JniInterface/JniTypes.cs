using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace DarkNotes.JniInterface
{
	[StructLayout(LayoutKind.Sequential)]
	internal struct JNINativeMethod
	{
		[MarshalAs(UnmanagedType.LPStr)]
		string name;
		[MarshalAs(UnmanagedType.LPStr)]
		string signature;
		IntPtr fnPtr;
	};

	internal enum jobjectRefType
	{
		JNIInvalidRefType = 0,
		JNILocalRefType = 1,
		JNIGlobalRefType = 2,
		JNIWeakGlobalRefType = 3
	};
}
