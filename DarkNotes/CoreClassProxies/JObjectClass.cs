using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DarkNotes.CoreClassProxies
{
	/// <summary>
	/// An incomplete representation of Java's Throwable class.
	/// </summary>
	internal class JObjectClass
	{
		private readonly JniWrapper _vm;
		private readonly IntPtr _class;
		private readonly IntPtr _getClassMethod;
		private readonly IntPtr _toStringMethod;

		public JObjectClass(JniWrapper vm)
		{
			_vm = vm;
			_class = vm.FindClass("java/lang/Object");
			_getClassMethod = _vm.GetMethodID(_class, "getClass", "()Ljava/lang/Class;");
			_toStringMethod = _vm.GetMethodID(_class, "toString", "()Ljava/lang/String;");
		}

		public IntPtr GetClass(IntPtr objectPtr)
		{
			return _vm.CallObjectMethod(objectPtr, _getClassMethod);
		}

		public string ToString(IntPtr objectPtr)
		{
			IntPtr stringPtr = _vm.CallObjectMethod(objectPtr, _toStringMethod);
			return stringPtr == IntPtr.Zero ? "" : _vm.GetString(stringPtr);
		}
	}
}
