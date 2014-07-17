using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Experimental.DarkNotes.CoreClassProxies
{
	/// <summary>
	/// An incomplete representation of Java's Class class.
	/// </summary>
	internal class JClassClass
	{
		private readonly JniWrapper _vm;
		private readonly IntPtr _classClass;
		private readonly IntPtr _getFieldMethod;
		private readonly IntPtr _getNameMethod;
		private readonly IntPtr _isAssignableFromMethod;
		private readonly IntPtr _forNameMethod;
		private readonly IntPtr _getMethodMethod;
		private readonly IntPtr _getConstructorMethod;
		private readonly IntPtr _getConstructorsMethod;
		private readonly IntPtr _getMethodsMethod;

		public JClassClass(JniWrapper vm)
		{
			_vm = vm;
			_classClass = vm.FindClass("java/lang/Class");
			_getNameMethod = _vm.GetMethodID(_classClass, "getName", "()Ljava/lang/String;");
			_getFieldMethod = _vm.GetMethodID(_classClass, "getField", "(Ljava/lang/String;)Ljava/lang/reflect/Field;");
			_forNameMethod = vm.GetStaticMethodID(_classClass, "forName", "(Ljava/lang/String;ZLjava/lang/ClassLoader;)Ljava/lang/Class;");
			_isAssignableFromMethod = vm.GetMethodID(_classClass, "isAssignableFrom", "(Ljava/lang/Class;)Z");
			_getMethodMethod = vm.GetMethodID(_classClass, "getMethod", "(Ljava/lang/String;[Ljava/lang/Class;)Ljava/lang/reflect/Method;");
			_getMethodsMethod = vm.GetMethodID(_classClass, "getMethods", "()[Ljava/lang/reflect/Method;");
			_getConstructorMethod = vm.GetMethodID(_classClass, "getConstructor", "([Ljava/lang/Class;)Ljava/lang/reflect/Constructor;");
			_getConstructorsMethod = vm.GetMethodID(_classClass, "getConstructors", "()[Ljava/lang/reflect/Constructor;");
		}

		public IntPtr ForName(string className, bool initialize, IntPtr classLoader)
		{
			return _vm.CallStaticObjectMethod(_classClass, _forNameMethod, _vm.NewStringUTF(className), initialize, classLoader);
		}

		public string GetName(IntPtr classPtr)
		{
			return _vm.GetString(_vm.CallObjectMethod(classPtr, _getNameMethod));
		}

		public IntPtr GetMethod(IntPtr classPtr, string methodName, IntPtr[] parameterTypes)
		{
			return _vm.CallObjectMethod(classPtr, _getMethodMethod, _vm.NewString(methodName), _vm.NewArray(_vm.FindClass("java/lang/Class"), parameterTypes));
		}

		public IntPtr[] GetMethods(IntPtr classPtr)
		{
			return _vm.GetArray(_vm.CallObjectMethod(classPtr, _getMethodsMethod));
		}

		public IntPtr GetConstructor(IntPtr classPtr, IntPtr[] parameterTypes)
		{
			return _vm.CallObjectMethod(classPtr, _getConstructorMethod, _vm.NewArray(_vm.FindClass("java/lang/Class"), parameterTypes));
		}

		public IntPtr[] GetConstructors(IntPtr classPtr)
		{
			return _vm.GetArray(_vm.CallObjectMethod(classPtr, _getConstructorsMethod));
		}

		public IntPtr GetField(IntPtr classPtr, string fieldName)
		{
			return _vm.CallObjectMethod(classPtr, _getFieldMethod, _vm.NewStringUTF(fieldName));
		}

		public bool IsAssignableFrom(IntPtr classPtr, IntPtr otherClassPtr)
		{
			return _vm.CallBooleanMethod(classPtr, _isAssignableFromMethod, otherClassPtr);
		}
	}
}
