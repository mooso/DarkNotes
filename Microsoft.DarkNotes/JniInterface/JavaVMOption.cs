using System;
using System.Runtime.InteropServices;

namespace Microsoft.Experimental.DarkNotes.JniInterface
{
	/// <summary>
	/// JNI representation of a JVM creation option.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	internal struct JavaVMOption
	{
		[MarshalAs(UnmanagedType.LPStr)]
		private string _optionString;
		private IntPtr _extraInfo;

		public JavaVMOption(string optionString)
		{
			_optionString = optionString;
			_extraInfo = IntPtr.Zero;
		}
	}
}
