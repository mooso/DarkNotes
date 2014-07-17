using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Experimental.DarkNotes.CoreClassProxies
{
	/// <summary>
	/// An incomplete representation of Java's Throwable class.
	/// </summary>
	internal class JObjectClass
	{
		private readonly JniWrapper _vm;
		private readonly IntPtr _class;
		private readonly IntPtr _getClassMethod;

		public JObjectClass(JniWrapper vm)
		{
			_vm = vm;
			_class = vm.FindClass("java/lang/Object");
			_getClassMethod = _vm.GetMethodID(_class, "getClass", "()Ljava/lang/Class;");
		}

		public IntPtr GetClass(IntPtr objectPtr)
		{
			return _vm.CallObjectMethod(objectPtr, _getClassMethod);
		}
	}
}
