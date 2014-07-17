using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Experimental.DarkNotes
{
	/// <summary>
	/// An array in Java.
	/// </summary>
	internal class ArrayType : JavaType
	{
		private readonly JavaType _memberType;

		public ArrayType(JavaType memberType)
		{
			if (memberType == null)
			{
				throw new ArgumentNullException("memberType");
			}

			_memberType = memberType;
		}

		/// <summary>
		/// The type of members in the array.
		/// </summary>
		public JavaType MemberType
		{
			get { return _memberType; }
		}

		public override string JniClassName
		{
			get { return "[" + _memberType.JniClassName; }
		}

		public override bool IsAssignableFrom(JavaType other)
		{
			if (other is NullType)
			{
				return true;
			}

			ArrayType asArray = other as ArrayType;
			
			if (asArray == null)
			{
				return false;
			}

			return _memberType.IsAssignableFrom(asArray._memberType) &&
				(_memberType is JavaClass || asArray._memberType.IsAssignableFrom(_memberType));
		}
	}
}
