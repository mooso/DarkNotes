using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.DarkNotes.JniInterface;

namespace Microsoft.DarkNotes
{
	internal sealed class ParameterInfo
	{
		private readonly JValue _value;
		private readonly JavaType _type;

		private ParameterInfo(JValue value, JavaType type)
		{
			_value = value;
			_type = type;
		}

		public JavaType Type
		{
			get { return _type; }
		}

		public JValue Value(JniWrapper vm, bool box)
		{
			if (!box)
			{
				return _value;
			}
			else
			{
				ArrayType asArray;
				if ((asArray = _type as ArrayType) != null)
				{
					JavaArray arrayObject = new JavaArray(vm, _value.ToIntPtr(), asArray);
					return arrayObject.Box();
				}
				else
				{
					PrimitiveType primitiveType = (PrimitiveType)_type;
					return primitiveType.GetBoxClass().Box(_value, primitiveType);
				}
			}
		}

		private class JavaObjectFactory
		{
			private readonly JavaType _javaType;
			private readonly Func<object, JValue> _factory;

			private JavaObjectFactory(Func<object, JValue> factory, JavaType javaType)
			{
				_javaType = javaType;
				_factory = factory;
			}

			public JavaType JavaType { get { return _javaType; } }

			public JValue Create(object dotNetObject)
			{
				return _factory(dotNetObject);
			}

			public static JavaObjectFactory Array(JniWrapper vm, Type dotNetType, Array prototype)
			{
				object innerPrototype = null;
				if (prototype != null && prototype.Length > 0)
				{
					innerPrototype = prototype.GetValue(0);
				}
				JavaObjectFactory innerFactory = GetJavaType(vm, dotNetType.GetElementType(), innerPrototype);
				ArrayType javaArrayType = new ArrayType(innerFactory.JavaType);
				return new JavaObjectFactory(o =>
					{
						Array asArray = (Array)o;
						PrimitiveType asPrimitive = javaArrayType.MemberType as PrimitiveType;
						if (asPrimitive != null)
						{
							switch (asPrimitive.Kind)
							{
								case PrimitiveTypeKind.Boolean: return vm.NewBooleanArray((bool[])o);
								case PrimitiveTypeKind.Byte: return vm.NewByteArray((byte[])o);
								case PrimitiveTypeKind.Char: return vm.NewCharArray((char[])o);
								case PrimitiveTypeKind.Double: return vm.NewDoubleArray((double[])o);
								case PrimitiveTypeKind.Float: return vm.NewFloatArray((float[])o);
								case PrimitiveTypeKind.Int: return vm.NewIntArray((int[])o);
								case PrimitiveTypeKind.Long: return vm.NewLongArray((long[])o);
								case PrimitiveTypeKind.Short: return vm.NewShortArray((short[])o);
								default: throw new InvalidOperationException("Unknown primitive kind: " + asPrimitive.Kind);
							}
						}
						else
						{
							IntPtr[] elements = asArray.Cast<object>().Select(innerFactory._factory).Select(v => v.ToIntPtr()).ToArray();
							return vm.NewArray(vm.FindClass(innerFactory.JavaType.JniClassName), elements);
						}
					}, javaArrayType);
			}

			public static JavaObjectFactory String(JniWrapper vm)
			{
				return new JavaObjectFactory(o => vm.NewString((string)o), new JavaClass(vm, "java/lang/String"));
			}

			public static JavaObjectFactory Byte(JniWrapper vm)
			{
				return new JavaObjectFactory(o => new JValue((byte)o), PrimitiveType.Byte(vm));
			}

			public static JavaObjectFactory Short(JniWrapper vm)
			{
				return new JavaObjectFactory(o => new JValue((short)o), PrimitiveType.Short(vm));
			}

			public static JavaObjectFactory Int(JniWrapper vm)
			{
				return new JavaObjectFactory(o => new JValue((int)o), PrimitiveType.Int(vm));
			}

			public static JavaObjectFactory Long(JniWrapper vm)
			{
				return new JavaObjectFactory(o => new JValue((long)o), PrimitiveType.Long(vm));
			}

			public static JavaObjectFactory Float(JniWrapper vm)
			{
				return new JavaObjectFactory(o => new JValue((float)o), PrimitiveType.Float(vm));
			}

			public static JavaObjectFactory Double(JniWrapper vm)
			{
				return new JavaObjectFactory(o => new JValue((double)o), PrimitiveType.Double(vm));
			}

			public static JavaObjectFactory Char(JniWrapper vm)
			{
				return new JavaObjectFactory(o => new JValue((char)o), PrimitiveType.Char(vm));
			}

			public static JavaObjectFactory Boolean(JniWrapper vm)
			{
				return new JavaObjectFactory(o => new JValue((bool)o), PrimitiveType.Boolean(vm));
			}

			public static JavaObjectFactory Object(JniWrapper vm, JavaClass javaClass)
			{
				return new JavaObjectFactory(o => new JValue(((JavaObject)o).Pointer), javaClass);
			}

			public static JavaObjectFactory Array(JniWrapper vm, ArrayType arrayType)
			{
				return new JavaObjectFactory(o => new JValue(((JavaArray)o).ArrayPointer), arrayType);
			}
		}

		private static JavaObjectFactory GetJavaType(JniWrapper vm, Type dotNetType, object prototype)
		{
			if (dotNetType.IsArray)
			{
				return JavaObjectFactory.Array(vm, dotNetType, (Array)prototype);
			}
			else if (dotNetType.IsAssignableFrom(typeof(string)))
			{
				return JavaObjectFactory.String(vm);
			}
			else if (dotNetType.IsAssignableFrom(typeof(byte)))
			{
				return JavaObjectFactory.Byte(vm);
			}
			else if (dotNetType.IsAssignableFrom(typeof(short)))
			{
				return JavaObjectFactory.Short(vm);
			}
			else if (dotNetType.IsAssignableFrom(typeof(int)))
			{
				return JavaObjectFactory.Int(vm);
			}
			else if (dotNetType.IsAssignableFrom(typeof(long)))
			{
				return JavaObjectFactory.Long(vm);
			}
			else if (dotNetType.IsAssignableFrom(typeof(float)))
			{
				return JavaObjectFactory.Float(vm);
			}
			else if (dotNetType.IsAssignableFrom(typeof(double)))
			{
				return JavaObjectFactory.Double(vm);
			}
			else if (dotNetType.IsAssignableFrom(typeof(char)))
			{
				return JavaObjectFactory.Char(vm);
			}
			else if (dotNetType.IsAssignableFrom(typeof(bool)))
			{
				return JavaObjectFactory.Boolean(vm);
			}
			else if (dotNetType.IsAssignableFrom(typeof(JavaObject)))
			{
				if (prototype == null)
				{
					throw new InvalidOperationException("Can't infer the type of the Java object without an instance.");
				}
				return JavaObjectFactory.Object(vm, ((JavaObject)prototype).Class);
			}
			else if (dotNetType.IsAssignableFrom(typeof(JavaArray)))
			{
				if (prototype == null)
				{
					throw new InvalidOperationException("Can't infer the type of the Java array without an instance.");
				}
				return JavaObjectFactory.Array(vm, ((JavaArray)prototype).ArrayType);
			}
			else
			{
				throw new InvalidOperationException("Can't infer the Java type of " + dotNetType);
			}
		}

		public static ParameterInfo[] GetParameterInfo(JniWrapper vm, object[] args)
		{
			ParameterInfo[] ret = new ParameterInfo[args.Length];
			for (int i = 0; i < args.Length; i++)
			{
				if (args[i] == null)
				{
					ret[i] = new ParameterInfo(new JValue(IntPtr.Zero), new NullType());
				}
				else
				{
					JavaObjectFactory objectFactory = GetJavaType(vm, args[i].GetType(), args[i]);
					ret[i] = new ParameterInfo(objectFactory.Create(args[i]), objectFactory.JavaType);
				}
			}
			return ret;
		}
	}
}
