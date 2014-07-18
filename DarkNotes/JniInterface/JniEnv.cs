using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace DarkNotes.JniInterface
{
	/// <summary>
	/// JNI struct for representing most of the actual operations with the Java VM such as finding and invoking methods.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	internal partial struct JniEnv
	{
		private IntPtr _functions;

		public T GetDelegate<T>()
			where T: class
		{
			int location = (int)(JniInterfaceTable)Enum.Parse(typeof(JniInterfaceTable), typeof(T).Name);
			return Marshal.GetDelegateForFunctionPointer(Marshal.ReadIntPtr(_functions + Marshal.SizeOf(typeof(IntPtr)) * (4 + location)), typeof(T)) as T;
		}
	}

}
