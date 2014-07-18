using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace DarkNotes
{
	internal class PrimitiveType : JavaType
	{
		private readonly PrimitiveTypeKind _kind;
		private readonly JniWrapper _vm;

		private PrimitiveType(JniWrapper vm, PrimitiveTypeKind kind)
		{
			_kind = kind;
			_vm = vm;
		}

		public static PrimitiveType Byte(JniWrapper vm) { return new PrimitiveType(vm, PrimitiveTypeKind.Byte); }
		public static PrimitiveType Short(JniWrapper vm) { return new PrimitiveType(vm, PrimitiveTypeKind.Short); }
		public static PrimitiveType Int(JniWrapper vm) { return new PrimitiveType(vm, PrimitiveTypeKind.Int); }
		public static PrimitiveType Long(JniWrapper vm) { return new PrimitiveType(vm, PrimitiveTypeKind.Long); }
		public static PrimitiveType Char(JniWrapper vm) { return new PrimitiveType(vm, PrimitiveTypeKind.Char); }
		public static PrimitiveType Float(JniWrapper vm) { return new PrimitiveType(vm, PrimitiveTypeKind.Float); }
		public static PrimitiveType Double(JniWrapper vm) { return new PrimitiveType(vm, PrimitiveTypeKind.Double); }
		public static PrimitiveType Boolean(JniWrapper vm) { return new PrimitiveType(vm, PrimitiveTypeKind.Boolean); }

		public PrimitiveTypeKind Kind
		{
			get { return _kind; }
		}

		private static readonly Dictionary<PrimitiveTypeKind, PrimitiveTypeKind[]> _wideningConversionTable = new Dictionary<PrimitiveTypeKind, PrimitiveTypeKind[]>()
			{
				{ PrimitiveTypeKind.Byte, new [] { PrimitiveTypeKind.Short, PrimitiveTypeKind.Int, PrimitiveTypeKind.Long, PrimitiveTypeKind.Float, PrimitiveTypeKind.Double } },
				{ PrimitiveTypeKind.Short, new [] { PrimitiveTypeKind.Int, PrimitiveTypeKind.Long, PrimitiveTypeKind.Float, PrimitiveTypeKind.Double } },
				{ PrimitiveTypeKind.Char, new [] { PrimitiveTypeKind.Int, PrimitiveTypeKind.Long, PrimitiveTypeKind.Float, PrimitiveTypeKind.Double } },
				{ PrimitiveTypeKind.Int, new [] { PrimitiveTypeKind.Long, PrimitiveTypeKind.Float, PrimitiveTypeKind.Double } },
				{ PrimitiveTypeKind.Long, new [] { PrimitiveTypeKind.Float, PrimitiveTypeKind.Double } },
				{ PrimitiveTypeKind.Float, new [] { PrimitiveTypeKind.Double } },
			};
			

		public bool IsWideningOrIdentityConvertibleTo(PrimitiveType conversionTarget)
		{
			if (_kind == conversionTarget._kind)
			{
				return true;
			}
			PrimitiveTypeKind[] acceptableTargets;
			if (!_wideningConversionTable.TryGetValue(_kind, out acceptableTargets))
			{
				return false;
			}
			return acceptableTargets.Contains(conversionTarget._kind);
		}

		public override bool IsAssignableFrom(JavaType other)
		{
			if (other is NullType)
			{
				return false; // You can't use null's with primitives.
			}
			PrimitiveType asPrimitive = other as PrimitiveType;
			if (asPrimitive != null)
			{
				return asPrimitive.IsWideningOrIdentityConvertibleTo(this);
			}
			JavaClass asClass = (JavaClass)other;
			JavaClass boxed = GetBoxClass();
			return boxed.IsAssignableFrom(asClass);
		}

		public JavaClass GetBoxClass()
		{
			string myName;
			switch (_kind)
			{
				case PrimitiveTypeKind.Int:
					myName = "Integer";
					break;
				case PrimitiveTypeKind.Char:
					myName = "Character";
					break;
				default:
					myName = _kind.ToString();
					break;
			}
			return new JavaClass(_vm, "java.lang." + myName);
		}

		public static PrimitiveType FromString(JniWrapper vm, string name)
		{
			switch (name)
			{
				case "byte": return new PrimitiveType(vm, PrimitiveTypeKind.Byte);
				case "short": return new PrimitiveType(vm, PrimitiveTypeKind.Short);
				case "int": return new PrimitiveType(vm, PrimitiveTypeKind.Int);
				case "long": return new PrimitiveType(vm, PrimitiveTypeKind.Long);
				case "char": return new PrimitiveType(vm, PrimitiveTypeKind.Char);
				case "float": return new PrimitiveType(vm, PrimitiveTypeKind.Float);
				case "double": return new PrimitiveType(vm, PrimitiveTypeKind.Double);
				case "boolean": return new PrimitiveType(vm, PrimitiveTypeKind.Boolean);
				default: return null;
			}
		}

		public override string JniClassName
		{
			get
			{
				switch (_kind)
				{
					case PrimitiveTypeKind.Boolean: return "Z";
					case PrimitiveTypeKind.Byte: return "B";
					case PrimitiveTypeKind.Char: return "C";
					case PrimitiveTypeKind.Double: return "D";
					case PrimitiveTypeKind.Float: return "F";
					case PrimitiveTypeKind.Int: return "I";
					case PrimitiveTypeKind.Long: return "J";
					case PrimitiveTypeKind.Short: return "S";
					default:
						throw new InvalidOperationException("Uknown primite type kind: " + _kind);
				}
			}
		}
	}
}
