using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace DarkNotes.JniInterface
{
	/// <summary>
	/// JNI struct for representing the higher level operations on the JVM: destroying it, attaching/detaching a thread.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	internal partial struct JavaVM
	{
		private IntPtr _functions;

		public T GetDelegate<T>()
			where T: class
		{
			int location = (int)(JniInvokeInterfaceTable)Enum.Parse(typeof(JniInvokeInterfaceTable), typeof(T).Name);
			return Marshal.GetDelegateForFunctionPointer(Marshal.ReadIntPtr(_functions + Marshal.SizeOf(typeof(IntPtr)) * (3 + location)), typeof(T)) as T;
		}
	}
}
