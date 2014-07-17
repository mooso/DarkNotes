using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Experimental.DarkNotes.CoreClassProxies
{
	/// <summary>
	/// An incomplete representation of Java's Method class.
	/// </summary>
	internal class JMethodClass
	{
		private readonly JniWrapper _vm;
		private readonly IntPtr _methodGetNameMethod;
		private readonly IntPtr _methodGetParameterTypes;
		private readonly IntPtr _methodGetReturnTypeMethod;

		public JMethodClass(JniWrapper vm)
		{
			_vm = vm;
			IntPtr methodClass = vm.FindClass("java/lang/reflect/Method");
			_methodGetNameMethod = vm.GetMethodID(methodClass, "getName", "()Ljava/lang/String;");
			_methodGetParameterTypes = vm.GetMethodID(methodClass, "getParameterTypes", "()[Ljava/lang/Class;");
			_methodGetReturnTypeMethod = vm.GetMethodID(methodClass, "getReturnType", "()Ljava/lang/Class;");
		}

		public string GetMethodName(IntPtr methodPtr)
		{
			return _vm.GetString(_vm.CallObjectMethod(methodPtr, _methodGetNameMethod));
		}

		public IntPtr[] GetParameterTypes(IntPtr methodPtr)
		{
			return _vm.GetArray(_vm.CallObjectMethod(methodPtr, _methodGetParameterTypes));
		}

		public IntPtr GetReturnType(IntPtr methodPtr)
		{
			return _vm.CallObjectMethod(methodPtr, _methodGetReturnTypeMethod);
		}
	}
}
