using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DarkNotes
{
	/// <summary>
	/// Encapsulates exceptions from Java.
	/// </summary>
	[Serializable]
	public class JavaException : Exception
	{
		public JavaException() { }
		public JavaException(string message) : base(message) { }
		public JavaException(string message, Exception inner) : base(message, inner) { }
		protected JavaException(
		System.Runtime.Serialization.SerializationInfo info,
		System.Runtime.Serialization.StreamingContext context)
			: base(info, context) { }
	}
}
