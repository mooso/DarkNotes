using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;

namespace Microsoft.DarkNotes
{
	internal sealed class JavaArray : DynamicObject
	{
		private readonly JniWrapper _vm;
		private readonly IntPtr _arrayPtr;
		private readonly ArrayType _arrayType;

		public JavaArray(JniWrapper vm, IntPtr arrayPtr, ArrayType arrayType)
		{
			_vm = vm;
			_arrayPtr = arrayPtr;
			_arrayType = arrayType;
		}

		public IntPtr ArrayPointer
		{
			get { return _arrayPtr; }
		}

		public ArrayType ArrayType
		{
			get { return _arrayType; }
		}

		internal IntPtr Box()
		{
			PrimitiveType memberAsPrimitive = (PrimitiveType)_arrayType.MemberType;
			JavaClass boxClass = memberAsPrimitive.GetBoxClass();
			IntPtr[] members;
			switch (memberAsPrimitive.Kind)
			{
				case PrimitiveTypeKind.Boolean:
					members = _vm.GetBooleanArray(_arrayPtr).Select(b => boxClass.Box(b, memberAsPrimitive).ToIntPtr()).ToArray();
					break;
				case PrimitiveTypeKind.Byte:
					members = _vm.GetByteArray(_arrayPtr).Select(b => boxClass.Box(b, memberAsPrimitive).ToIntPtr()).ToArray();
					break;
				case PrimitiveTypeKind.Char:
					members = _vm.GetCharArray(_arrayPtr).Select(b => boxClass.Box(b, memberAsPrimitive).ToIntPtr()).ToArray();
					break;
				case PrimitiveTypeKind.Double:
					members = _vm.GetDoubleArray(_arrayPtr).Select(b => boxClass.Box(b, memberAsPrimitive).ToIntPtr()).ToArray();
					break;
				case PrimitiveTypeKind.Float:
					members = _vm.GetFloatArray(_arrayPtr).Select(b => boxClass.Box(b, memberAsPrimitive).ToIntPtr()).ToArray();
					break;
				case PrimitiveTypeKind.Int:
					members = _vm.GetIntArray(_arrayPtr).Select(b => boxClass.Box(b, memberAsPrimitive).ToIntPtr()).ToArray();
					break;
				case PrimitiveTypeKind.Long:
					members = _vm.GetLongArray(_arrayPtr).Select(b => boxClass.Box(b, memberAsPrimitive).ToIntPtr()).ToArray();
					break;
				case PrimitiveTypeKind.Short:
					members = _vm.GetShortArray(_arrayPtr).Select(b => boxClass.Box(b, memberAsPrimitive).ToIntPtr()).ToArray();
					break;
				default: throw new InvalidOperationException("Unknown primitive kind: " + memberAsPrimitive.Kind);
			}
			return boxClass.NewArray(members);
		}

		public override bool TryGetMember(GetMemberBinder binder, out object result)
		{
			if (String.Equals(binder.Name, "length", StringComparison.OrdinalIgnoreCase))
			{
				result = _vm.GetArrayLength(_arrayPtr);
				return true;
			}
			return base.TryGetMember(binder, out result);
		}

		public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
		{
			int index = (int)indexes[0];

			PrimitiveType memberAsPrimitive;
			ArrayType memberAsArray;
			if ((memberAsPrimitive = _arrayType.MemberType as PrimitiveType) != null)
			{
				switch (memberAsPrimitive.Kind)
				{
					case PrimitiveTypeKind.Boolean: result = _vm.GetBooleanArray(_arrayPtr)[index]; break;
					case PrimitiveTypeKind.Byte: result = _vm.GetByteArray(_arrayPtr)[index]; break;
					case PrimitiveTypeKind.Char: result = _vm.GetCharArray(_arrayPtr)[index]; break;
					case PrimitiveTypeKind.Double: result = _vm.GetDoubleArray(_arrayPtr)[index]; break;
					case PrimitiveTypeKind.Float: result = _vm.GetFloatArray(_arrayPtr)[index]; break;
					case PrimitiveTypeKind.Int: result = _vm.GetIntArray(_arrayPtr)[index]; break;
					case PrimitiveTypeKind.Long: result = _vm.GetLongArray(_arrayPtr)[index]; break;
					case PrimitiveTypeKind.Short: result = _vm.GetShortArray(_arrayPtr)[index]; break;
					default: throw new InvalidOperationException("Unknown primitive kind: " + memberAsPrimitive.Kind);
				}
				return true;
			}
			else if ((memberAsArray = _arrayType.MemberType as ArrayType) != null)
			{
				result = new JavaArray(_vm, _vm.GetArray(_arrayPtr)[index], memberAsArray);
				return true;
			}
			else
			{
				IntPtr objectPointer = _vm.GetArray(_arrayPtr)[index];
				// I'll intentionally pass the actual type as null if we have a non-null object, so we can discover
				// the precise type instead of just assuming it's of the base class type.
				result = new JavaObject(_vm, objectPointer, objectPointer == IntPtr.Zero ? (JavaClass)_arrayType.MemberType : null);
				return true;
			}
		}
	}
}
