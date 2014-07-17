using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;
using System.Globalization;

namespace Microsoft.DarkNotes
{
	internal sealed class JavaPackage : DynamicObject
	{
		private readonly string _name;
		private readonly JniWrapper _vm;

		public JavaPackage(JniWrapper vm, string name)
		{
			_vm = vm;
			_name = name.Replace('.', '/');
		}

		public override string ToString()
		{
			return String.Format(CultureInfo.InvariantCulture, "(Package: {0})", _name);
		}

		public bool TryGetClass(GetMemberBinder binder, out object result)
		{
			string qualifiedName = GetQualifiedName(binder.Name);
			// Try to find the class with this name and fail if not found. I really tried to find anyway
			// in java to check for class existence without exceptions but couldn't find any.
			try
			{
				IntPtr classPtr = _vm.FindClass(qualifiedName);
				result = new JavaClass(_vm, classPtr, IntPtr.Zero, qualifiedName);
				return true;
			}
			catch (JavaException)
			{
				result = null;
				return false;
			}
		}

		public override bool TryGetMember(GetMemberBinder binder, out object result)
		{
			if (!TryGetClass(binder, out result))
			{
				result = new JavaPackage(_vm, GetQualifiedName(binder.Name));
			}
			return true;
		}

		private string GetQualifiedName(string memberName)
		{
			return String.IsNullOrEmpty(_name) ? memberName : _name + "/" + memberName;
		}
	}
}
