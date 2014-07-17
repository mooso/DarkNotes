using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.DarkNotes.CoreClassProxies
{
	/// <summary>
	/// An incomplete representation of Java's Constructor class.
	/// </summary>
	internal class JConstructorClass
	{
		private readonly JniWrapper _vm;
		private readonly IntPtr _getParameterTypes;

		public JConstructorClass(JniWrapper vm)
		{
			_vm = vm;
			IntPtr methodClass = vm.FindClass("java/lang/reflect/Constructor");
			_getParameterTypes = vm.GetMethodID(methodClass, "getParameterTypes", "()[Ljava/lang/Class;");
		}

		public IntPtr[] GetParameterTypes(IntPtr methodPtr)
		{
			return _vm.GetArray(_vm.CallObjectMethod(methodPtr, _getParameterTypes));
		}
	}
}
