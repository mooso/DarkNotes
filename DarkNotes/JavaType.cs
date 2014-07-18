using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;
using DarkNotes.CoreClassProxies;

namespace DarkNotes
{
	internal abstract class JavaType : DynamicObject
	{
		public abstract bool IsAssignableFrom(JavaType other);

		public abstract string JniClassName { get; }

		public static JavaType FromReflectedType(JniWrapper vm, IntPtr reflectedType)
		{
			string name = new JClassClass(vm).GetName(reflectedType);
			if (name.StartsWith("["))
			{
				return TypeByJniName(vm, name);
			}
			PrimitiveType primitiveType = PrimitiveType.FromString(vm, name);
			if (primitiveType != null)
			{
				return primitiveType;
			}
			return new JavaClass(vm, IntPtr.Zero, reflectedType, name);
		}

		private static JavaType TypeByJniName(JniWrapper vm, string name)
		{
			if (String.IsNullOrEmpty(name))
			{
				throw new ArgumentNullException("name");
			}
			switch (name[0])
			{
				case 'Z': return PrimitiveType.Boolean(vm);
				case 'B': return PrimitiveType.Byte(vm);
				case 'C': return PrimitiveType.Char(vm);
				case 'S': return PrimitiveType.Short(vm);
				case 'I': return PrimitiveType.Int(vm);
				case 'J': return PrimitiveType.Long(vm);
				case 'F': return PrimitiveType.Float(vm);
				case 'D': return PrimitiveType.Double(vm);
				case 'L': return new JavaClass(vm, name.TrimStart('L').TrimEnd(';').Replace('/', '.'));
				case '[': return new ArrayType(TypeByJniName(vm, name.Substring(1)));
				default:
					throw new InvalidOperationException("Can't decipher this JNI type name: " + name);
			}
		}
	}
}
