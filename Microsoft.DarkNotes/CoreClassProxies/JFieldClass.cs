using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.DarkNotes.CoreClassProxies
{
	/// <summary>
	/// An incomplete representation of Java's Field class.
	/// </summary>
	internal class JFieldClass
	{
		private readonly JniWrapper _vm;
		private readonly IntPtr _getTypeMethod;

		public JFieldClass(JniWrapper vm)
		{
			_vm = vm;
			_getTypeMethod = _vm.GetMethodID(vm.FindClass("java/lang/reflect/Field"), "getType", "()Ljava/lang/Class;");
		}

		public IntPtr GetType(IntPtr fieldPtr)
		{
			return _vm.CallObjectMethod(fieldPtr, _getTypeMethod);
		}
	}
}
