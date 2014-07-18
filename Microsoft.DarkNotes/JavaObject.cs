using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;
using DarkNotes.CoreClassProxies;
using DarkNotes.JniInterface;

namespace DarkNotes
{
	/// <summary>
	/// A dynamic representation of a Java object.
	/// </summary>
	internal sealed class JavaObject : DynamicObject
	{
		private readonly JniWrapper _vm;
		private readonly IntPtr _objectPointer;
		private readonly JavaClass _class;
		private readonly JMethodClass _methodClass;
		private readonly JClassClass _classClass;
		private readonly JObjectClass _objectClass;

		public JavaObject(JniWrapper vm, IntPtr objectPointer, JavaClass javaClass)
		{
			_vm = vm;
			_objectPointer = objectPointer;
			_methodClass = new JMethodClass(_vm);
			_classClass = new JClassClass(_vm);
			_objectClass = new JObjectClass(_vm);
			if (javaClass == null && _objectPointer != IntPtr.Zero)
			{
				_class = new JavaClass(vm, jniClass: vm.GetObjectClass(_objectPointer), reflectedClass: _objectClass.GetClass(_objectPointer), name: null);
			}
			else
			{
				_class = javaClass;
			}
		}

		public IntPtr Pointer
		{
			get { return _objectPointer; }
		}

		public JavaClass Class
		{
			get { return _class; }
		}

		public override string ToString()
		{
			dynamic t = this;
			return _vm.GetString(((JavaObject)t.toString())._objectPointer);
		}

		public override bool TryConvert(ConvertBinder binder, out object result)
		{
			if (binder.Type.Equals(typeof(string)))
			{
				result = ToString();
				return true;
			}
			const string javaDotLang = "java.lang.";
			if (_class != null && _class.Name.StartsWith(javaDotLang))
			{
				bool converted;
				switch (_class.Name.Substring(javaDotLang.Length))
				{
					case "Boolean": converted = ConvertToPrimitive(binder, "booleanValue", 'Z', (vm, o, m) => vm.CallBooleanMethod(o, m), out result); break;
					case "Byte": converted = ConvertToPrimitive(binder, "byteValue", 'B', (vm, o, m) => vm.CallBooleanMethod(o, m), out result); break;
					case "Character": converted = ConvertToPrimitive(binder, "charValue", 'C', (vm, o, m) => vm.CallCharMethod(o, m), out result); break;
					case "Short": converted = ConvertToPrimitive(binder, "shortValue", 'S', (vm, o, m) => vm.CallShortMethod(o, m), out result); break;
					case "Integer": converted = ConvertToPrimitive(binder, "intValue", 'I', (vm, o, m) => vm.CallIntMethod(o, m), out result); break;
					case "Long": converted = ConvertToPrimitive(binder, "longValue", 'J', (vm, o, m) => vm.CallLongMethod(o, m), out result); break;
					case "Float": converted = ConvertToPrimitive(binder, "floatValue", 'F', (vm, o, m) => vm.CallFloatMethod(o, m), out result); break;
					case "Double": converted = ConvertToPrimitive(binder, "doubleValue", 'D', (vm, o, m) => vm.CallDoubleMethod(o, m), out result); break;
					default: converted = false; result = null; break;
				}
				if (converted)
				{
					return true;
				}
			}
			return base.TryConvert(binder, out result);
		}

		public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
		{
			ParameterInfo[] parameterInfo = ParameterInfo.GetParameterInfo(_vm, args);
			bool[] needBoxing;
			IntPtr method = _class.GetMethod(binder.Name, parameterInfo.Select(p => p.Type), out needBoxing);
			if (method != IntPtr.Zero)
			{
				result = InvokeMethod(method, parameterInfo, needBoxing);
				return true;
			}
			return base.TryInvokeMember(binder, args, out result);
		}

		public override bool TryGetMember(GetMemberBinder binder, out object result)
		{
			result = _class.TryGetObjectFieldValue(_objectPointer, binder.Name);
			if (result != null)
			{
				return true;
			}
			return base.TryGetMember(binder, out result);
		}

		private bool ConvertToPrimitive<T>(ConvertBinder binder, string unboxMethodName, char primitiveTypeName, Func<JniWrapper, IntPtr, IntPtr, T> callMethodFunc, out object result)
		{
			if (binder.Type.IsAssignableFrom(typeof(T)))
			{
				result = Unbox(unboxMethodName, primitiveTypeName, callMethodFunc);
				return true;
			}
			else
			{
				result = null;
				return false;
			}
		}

		private T Unbox<T>(string unboxMethodName, char primitiveTypeName, Func<JniWrapper, IntPtr, IntPtr, T> callMethodFunc)
		{
			IntPtr methodId = _class.GetJniMethodId(unboxMethodName, "()" + primitiveTypeName);//"booleanValue", "()Z");
			return callMethodFunc(_vm, _objectPointer, methodId);//_vm.CallBooleanMethod(_objectPointer, methodId);
		}

		private object InvokeMethod(IntPtr method, ParameterInfo[] parameterInfo, bool[] needBoxing)
		{
			object result;
			IntPtr methodReturn = _methodClass.GetReturnType(method);
			IntPtr jniMethodPtr = _vm.FromReflectedMethod(method);
			string returnTypeName = _classClass.GetName(methodReturn);
			JValue[] parameters = parameterInfo.Select((p, i) => p.Value(_vm, needBoxing[i])).ToArray();
			PrimitiveType returnTypeAsPrimitive = PrimitiveType.FromString(_vm, returnTypeName);
			if (methodReturn == IntPtr.Zero || returnTypeName == "void")
			{
				result = null;
				_vm.CallVoidMethod(_objectPointer, jniMethodPtr, parameters);
			}
			else if (returnTypeAsPrimitive != null)
			{
				switch (returnTypeAsPrimitive.Kind)
				{
					case PrimitiveTypeKind.Boolean:
						result = _vm.CallBooleanMethod(_objectPointer, jniMethodPtr, parameters);
						break;
					case PrimitiveTypeKind.Byte:
						result = _vm.CallByteMethod(_objectPointer, jniMethodPtr, parameters);
						break;
					case PrimitiveTypeKind.Char:
						result = _vm.CallCharMethod(_objectPointer, jniMethodPtr, parameters);
						break;
					case PrimitiveTypeKind.Double:
						result = _vm.CallDoubleMethod(_objectPointer, jniMethodPtr, parameters);
						break;
					case PrimitiveTypeKind.Float:
						result = _vm.CallFloatMethod(_objectPointer, jniMethodPtr, parameters);
						break;
					case PrimitiveTypeKind.Int:
						result = _vm.CallIntMethod(_objectPointer, jniMethodPtr, parameters);
						break;
					case PrimitiveTypeKind.Long:
						result = _vm.CallLongMethod(_objectPointer, jniMethodPtr, parameters);
						break;
					case PrimitiveTypeKind.Short:
						result = _vm.CallShortMethod(_objectPointer, jniMethodPtr, parameters);
						break;
					default:
						throw new InvalidOperationException("Unknown primitive type: " + returnTypeAsPrimitive.Kind);
				}
			}
			else
			{
				IntPtr methodResult = _vm.CallObjectMethod(_objectPointer, jniMethodPtr, parameters);
				if (methodResult != IntPtr.Zero)
				{
					if (returnTypeName.StartsWith("["))
					{
						result = new JavaArray(_vm, methodResult, (ArrayType)JavaType.FromReflectedType(_vm, methodReturn));
					}
					else
					{
						result = new JavaObject(_vm, methodResult, null);
					}
				}
				else
				{
					result = null;
				}
			}
			return result;
		}
	}
}
