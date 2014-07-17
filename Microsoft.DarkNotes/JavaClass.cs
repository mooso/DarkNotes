using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.Experimental.DarkNotes.CoreClassProxies;
using Microsoft.Experimental.DarkNotes.JniInterface;

namespace Microsoft.Experimental.DarkNotes
{
	/// <summary>
	/// A dynamic representation of a Java class.
	/// </summary>
	internal sealed class JavaClass : JavaType
	{
		/// <summary>
		/// The JniWrapper to use.
		/// </summary>
		private readonly JniWrapper _vm;
		/// <summary>
		/// The pointer to the JNI class.
		/// </summary>
		private readonly IntPtr _jniClass;
		/// <summary>
		/// The pointer to the reflected class in Java (so it's a pointer to an object as far as JNI is concerned).
		/// </summary>
		private readonly IntPtr _reflectedClass;
		private readonly string _name;
		private readonly JConstructorClass _constructorClass;
		private readonly JMethodClass _methodClass;
		private readonly JClassClass _classClass;
		private readonly JFieldClass _fieldClass;
		private readonly JClassLoaderClass _classLoaderClass;
		private readonly ReadOnlyCollection<IntPtr> _allMethods;
		private readonly ReadOnlyCollection<IntPtr> _allConstructors;

		/// <summary>
		/// Constructs the wrapper given the fully qualified class name.
		/// </summary>
		/// <param name="vm"></param>
		/// <param name="name"></param>
		public JavaClass(JniWrapper vm, string name)
			: this(vm, jniClass: IntPtr.Zero, reflectedClass: IntPtr.Zero, name: name)
		{
		}

		private static void ThrowClassNotFoundException(string name)
		{
			throw new InvalidOperationException("Class not found: " + name);
		}

		internal JavaClass(JniWrapper vm, IntPtr jniClass, IntPtr reflectedClass, string name)
		{
			_vm = vm;
			_classClass = new JClassClass(vm);
			_methodClass = new JMethodClass(vm);
			_fieldClass = new JFieldClass(vm);
			_constructorClass = new JConstructorClass(vm);
			_classLoaderClass = new JClassLoaderClass(vm);
			_jniClass = jniClass;
			_reflectedClass = reflectedClass;
			if (_reflectedClass == IntPtr.Zero)
			{
				if (name == null)
				{
					throw new ArgumentNullException("I can't infer the name from just the JNI class (I should be able to, but I have no idea how).");
				}
				_reflectedClass = _classClass.ForName(name.Replace('/', '.'), true, _classLoaderClass.GetSystemClassLoader());
				if (_reflectedClass == IntPtr.Zero)
				{
					ThrowClassNotFoundException(name);
				}
			}
			if (name == null)
			{
				name = _classClass.GetName(_reflectedClass);
			}
			if (_jniClass == IntPtr.Zero)
			{
				_jniClass = vm.FindClass(name.Replace('.', '/'));
				if (_jniClass == IntPtr.Zero)
				{
					ThrowClassNotFoundException(name);
				}
			}
			_name = name.Replace('/', '.');
			_allMethods = _classClass.GetMethods(_reflectedClass).ToList().AsReadOnly();
			_allConstructors = _classClass.GetConstructors(_reflectedClass).ToList().AsReadOnly();
		}

		public string Name
		{
			get { return _name; }
		}

		public override string JniClassName
		{
			get { return Name.Replace('.', '/'); }
		}

		internal IntPtr NewArray(IntPtr[] members)
		{
			return _vm.NewArray(_jniClass, members);
		}

		private Tuple<IntPtr, JavaClass> GetField(string fieldName)
		{
			IntPtr reflectedField = _classClass.GetField(_reflectedClass, fieldName);
			if (reflectedField == IntPtr.Zero)
			{
				return null;
			}
			IntPtr reflectedFieldClass = _fieldClass.GetType(reflectedField);
			JavaClass javaClass = new JavaClass(_vm, jniClass: IntPtr.Zero, reflectedClass: reflectedFieldClass, name: null);
			return Tuple.Create(_vm.FromReflectedField(reflectedField), javaClass);
		}

		internal JavaObject TryGetObjectFieldValue(IntPtr objectPtr, string name)
		{
			Tuple<IntPtr, JavaClass> field = GetField(name);
			if (field == null)
			{
				return null;
			}
			IntPtr value = _vm.GetObjectField(_jniClass, field.Item1);
			return value == IntPtr.Zero ? null : new JavaObject(_vm, value, field.Item2);
		}

		private class MethodSpeficityInfo : IComparable<MethodSpeficityInfo>
		{
			private readonly IntPtr _methodPointer;
			private readonly bool[] _needBoxing;
			private readonly JavaType[] _parameterTypes;

			public MethodSpeficityInfo(IntPtr methodPointer, bool[] needBoxing, JavaType[] parameterTypes)
			{
				_methodPointer = methodPointer;
				_needBoxing = needBoxing;
				_parameterTypes = parameterTypes;
			}

			public IntPtr MethodPointer
			{
				get { return _methodPointer; }
			}

			public bool[] NeedBoxing
			{
				get { return _needBoxing; }
			}

			public int CompareTo(MethodSpeficityInfo other)
			{
				if (_parameterTypes.Any(t => t == null) || other._parameterTypes.Any(t => t == null))
				{
					return 0;
				}
				// See http://docs.oracle.com/javase/specs/jls/se5.0/html/expressions.html#15.12
				bool myParamsAllAssignableFromOther = _parameterTypes.Zip(other._parameterTypes, (t1, t2) => t1.IsAssignableFrom(t2)).All(b => b);
				bool otherParamsAllAssignableFromMine = _parameterTypes.Zip(other._parameterTypes, (t1, t2) => t2.IsAssignableFrom(t1)).All(b => b);
				if (myParamsAllAssignableFromOther == otherParamsAllAssignableFromMine)
				{
					return 0;
				}
				else if (myParamsAllAssignableFromOther)
				{
					return 1;
				}
				else
				{
					return -1;
				}
			}
		}

		internal IntPtr GetMethod(string methodName, IEnumerable<JavaType> parameterTypes, out bool[] needBoxing)
		{
			List<MethodSpeficityInfo> qualifyingMethods = new List<MethodSpeficityInfo>();
			foreach (IntPtr m in _allMethods)
			{
				string currentMethodName = _methodClass.GetMethodName(m);
				if (currentMethodName == methodName)
				{
					IntPtr[] methodParams = _methodClass.GetParameterTypes(m);
					bool[] methodNeedBoxing;
					JavaType[] methodParameterInfo;
					if (ParameterTypesMatch(parameterTypes, methodParams, out methodNeedBoxing, out methodParameterInfo))
					{
						qualifyingMethods.Add(new MethodSpeficityInfo(m, methodNeedBoxing, methodParameterInfo));
					}
				}
			}
			if (qualifyingMethods.Count == 0)
			{
				needBoxing = null;
				return IntPtr.Zero;
			}
			qualifyingMethods.Sort();
			needBoxing = qualifyingMethods[0].NeedBoxing;
			return qualifyingMethods[0].MethodPointer;
		}

		private bool ParameterTypesMatch(IEnumerable<JavaType> actualParameterTypes, IntPtr[] formalParameterTypes, out bool[] needBoxing,
			out JavaType[] formalParameterTypeInfo)
		{
			// See http://docs.oracle.com/javase/specs/jls/se5.0/html/expressions.html#15.12
			// 1. Accepts the same number of parameters as given and
			// 2. The parameter types are assignable (I forgive unknown parametery types now and just assume they're equivalent to anything).

			if (formalParameterTypes.Length != actualParameterTypes.Count())
			{
				needBoxing = null;
				formalParameterTypeInfo = null;
				return false;
			}
			int currentIndex = 0;
			needBoxing = new bool[formalParameterTypes.Length];
			formalParameterTypeInfo = new JavaType[formalParameterTypes.Length];
			foreach (JavaType actualParameter in actualParameterTypes)
			{
				if (actualParameter != null)
				{
					formalParameterTypeInfo[currentIndex] = JavaType.FromReflectedType(_vm, formalParameterTypes[currentIndex]);
					if (!formalParameterTypeInfo[currentIndex].IsAssignableFrom(actualParameter))
					{
						needBoxing = null;
						return false;
					}
					ArrayType formalAsArray, actualAsArray;
					needBoxing[currentIndex] = (actualParameter is PrimitiveType && !(formalParameterTypeInfo[currentIndex] is PrimitiveType)) ||
						((actualAsArray = actualParameter as ArrayType) != null && (formalAsArray = formalParameterTypeInfo[currentIndex] as ArrayType) != null &&
							actualAsArray.MemberType is PrimitiveType && !(formalAsArray.MemberType is PrimitiveType));
				}
				else
				{
					needBoxing[currentIndex] = false;
				}
				currentIndex++;
			}
			return true;
		}

		internal IntPtr GetConstructor(IEnumerable<JavaType> parameterTypes, out bool[] needBoxing)
		{
			List<MethodSpeficityInfo> qualifyingMethods = new List<MethodSpeficityInfo>();
			foreach (var c in _allConstructors)
			{
				IntPtr[] methodParams = _constructorClass.GetParameterTypes(c);
				bool[] methodNeedBoxing;
				JavaType[] methodParameterInfo;
				if (ParameterTypesMatch(parameterTypes, methodParams, out methodNeedBoxing, out methodParameterInfo))
				{
					qualifyingMethods.Add(new MethodSpeficityInfo(c, methodNeedBoxing, methodParameterInfo));
				}
			}
			if (qualifyingMethods.Count == 0)
			{
				needBoxing = null;
				return IntPtr.Zero;
			}
			qualifyingMethods.Sort();
			needBoxing = qualifyingMethods[0].NeedBoxing;
			return qualifyingMethods[0].MethodPointer;
		}

		public dynamic New(params object[] args)
		{
			ParameterInfo[] info = ParameterInfo.GetParameterInfo(_vm, args);
			bool[] needBoxing;
			IntPtr method = GetConstructor(info.Select(i => i.Type), out needBoxing);
			if (method == IntPtr.Zero)
			{
				throw new InvalidOperationException("No appropriate constructor found.");
			}
			return new JavaObject(_vm, _vm.NewObject(_jniClass, _vm.FromReflectedMethod(method), info.Select((p, i) => p.Value(_vm, needBoxing[i])).ToArray()), this);
		}

		internal JValue Box(JValue primitiveValue, PrimitiveType primitiveType)
		{
			IntPtr constructorId = _vm.GetMethodID(_jniClass, "<init>", String.Format(CultureInfo.InvariantCulture, "({0})V", primitiveType.JniClassName));
			if (constructorId == IntPtr.Zero)
			{
				throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture,
					"No appropriate boxing constructor found for class {0} for primitive type {1}", _name, primitiveType.Kind));
			}
			return _vm.NewObject(_jniClass, constructorId, primitiveValue);
		}

		internal IntPtr GetJniMethodId(string name, string signature)
		{
			return _vm.GetMethodID(_jniClass, name, signature);
		}

		public override bool IsAssignableFrom(JavaType other)
		{
			if (other is NullType)
			{
				return true; // Reference types should accept null.
			}
			if (other is ArrayType)
			{
				return false;
			}
			JavaClass asClass = other as JavaClass;
			if (asClass == null)
			{
				asClass = ((PrimitiveType)other).GetBoxClass();
			}
			return _classClass.IsAssignableFrom(_reflectedClass, asClass._reflectedClass);
		}

		public override bool TryGetMember(GetMemberBinder binder, out object result)
		{
			if (binder.Name == "class")
			{
				result = new JavaObject(_vm, _reflectedClass, new JavaClass(_vm, "java/lang/Class"));
				return true;
			}
			Tuple<IntPtr, JavaClass> field = GetField(binder.Name);
			if (field != null)
			{
				IntPtr value = _vm.GetStaticObjectField(_jniClass, field.Item1);
				result = value == IntPtr.Zero ? null : new JavaObject(_vm, value, field.Item2);
				return true;
			}
			return base.TryGetMember(binder, out result);
		}

		public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
		{
			if (binder.Name == "new")
			{
				result = New(args);
				return true;
			}
			ParameterInfo[] parameterInfo = ParameterInfo.GetParameterInfo(_vm, args);
			bool[] needBoxing;
			IntPtr method = GetMethod(binder.Name, parameterInfo.Select(p => p.Type), out needBoxing);
			if (method != IntPtr.Zero)
			{
				result = InvokeMethod(method, parameterInfo, needBoxing);
				return true;
			}
			return base.TryInvokeMember(binder, args, out result);
		}

		private object InvokeMethod(IntPtr method, ParameterInfo[] parameterInfo, bool[] needBoxing)
		{
			object result;
			IntPtr methodReturn = _methodClass.GetReturnType(method);
			IntPtr jniMethodPtr = _vm.FromReflectedMethod(method);
			string returnTypeName = _classClass.GetName(methodReturn);
			JValue[] parameters = parameterInfo.Select((p, i) => p.Value(_vm, needBoxing[i])).ToArray();
			PrimitiveType returnTypeAsPrimitive = PrimitiveType.FromString(_vm, returnTypeName);
			if (methodReturn == IntPtr.Zero || _classClass.GetName(methodReturn) == "void")
			{
				result = null;
				_vm.CallStaticVoidMethod(_jniClass, jniMethodPtr, parameters);
			}
			else if (returnTypeAsPrimitive != null)
			{
				switch (returnTypeAsPrimitive.Kind)
				{
					case PrimitiveTypeKind.Boolean:
						result = _vm.CallStaticBooleanMethod(_jniClass, jniMethodPtr, parameters);
						break;
					case PrimitiveTypeKind.Byte:
						result = _vm.CallStaticByteMethod(_jniClass, jniMethodPtr, parameters);
						break;
					case PrimitiveTypeKind.Char:
						result = _vm.CallStaticCharMethod(_jniClass, jniMethodPtr, parameters);
						break;
					case PrimitiveTypeKind.Double:
						result = _vm.CallStaticDoubleMethod(_jniClass, jniMethodPtr, parameters);
						break;
					case PrimitiveTypeKind.Float:
						result = _vm.CallStaticFloatMethod(_jniClass, jniMethodPtr, parameters);
						break;
					case PrimitiveTypeKind.Int:
						result = _vm.CallStaticIntMethod(_jniClass, jniMethodPtr, parameters);
						break;
					case PrimitiveTypeKind.Long:
						result = _vm.CallStaticLongMethod(_jniClass, jniMethodPtr, parameters);
						break;
					case PrimitiveTypeKind.Short:
						result = _vm.CallStaticShortMethod(_jniClass, jniMethodPtr, parameters);
						break;
					default:
						throw new InvalidOperationException("Unknown primitive type: " + returnTypeAsPrimitive.Kind);
				}
			}
			else
			{
				IntPtr methodResult = _vm.CallStaticObjectMethod(_jniClass, jniMethodPtr, parameters);
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
