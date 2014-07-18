using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DarkNotes
{
	internal class NullType : JavaType
	{
		public override bool IsAssignableFrom(JavaType other)
		{
			return true; // Anything can be assigned to null. That's not the normal case though.
		}

		public override string JniClassName
		{
			get { return null; }
		}
	}
}
