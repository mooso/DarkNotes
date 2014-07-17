using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.DarkNotes.CoreClassProxies
{
	/// <summary>
	/// An incomplete representation of Java's Throwable class.
	/// </summary>
	internal class JThrowableClass
	{
		private readonly JniWrapper _vm;
		private readonly IntPtr _class;
		private readonly IntPtr _getMessageMethod;
		private readonly IntPtr _getCauseMethod;

		public JThrowableClass(JniWrapper vm)
		{
			_vm = vm;
			_class = vm.FindClass("java/lang/Throwable");
			_getMessageMethod = _vm.GetMethodID(_class, "getMessage", "()Ljava/lang/String;");
			_getCauseMethod = _vm.GetMethodID(_class, "getCause", "()Ljava/lang/Throwable;");
		}

		public string GetMessage(IntPtr objectPtr)
		{
			IntPtr messagePtr = _vm.CallObjectMethod(objectPtr, _getMessageMethod);
			return messagePtr == IntPtr.Zero ? "" : _vm.GetString(messagePtr);
		}

		public IntPtr GetCause(IntPtr objectPtr)
		{
			return _vm.CallObjectMethod(objectPtr, _getCauseMethod);
		}
	}
}
