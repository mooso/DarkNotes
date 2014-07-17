using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.DarkNotes.CoreClassProxies
{
	/// <summary>
	/// An incomplete representation of Java's ClassLoader class.
	/// </summary>
	internal class JClassLoaderClass
	{
		private readonly JniWrapper _vm;
		private readonly IntPtr _class;
		private readonly IntPtr _getSystemClassLoaderMethod;

		public JClassLoaderClass(JniWrapper vm)
		{
			_vm = vm;
			_class = vm.FindClass("java/lang/ClassLoader");
			_getSystemClassLoaderMethod = _vm.GetStaticMethodID(_class, "getSystemClassLoader", "()Ljava/lang/ClassLoader;");
		}

		public IntPtr GetSystemClassLoader()
		{
			return _vm.CallStaticObjectMethod(_class, _getSystemClassLoaderMethod);
		}
	}
}
