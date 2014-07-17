using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Collections.ObjectModel;

namespace Microsoft.DarkNotes.JniInterface
{
	/// <summary>
	/// JNI struct for representing the arguments for creating the Java VM.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	internal struct JavaVMInitArgs
	{
		private int _version;

		public int _nOptions;
		public IntPtr _options;
		[MarshalAs(UnmanagedType.U1)]
		private bool _ignoreUnrecognized;

		public int Version
		{
			get { return _version; }
			set { _version = value; }
		}
	}
}
