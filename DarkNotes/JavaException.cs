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
		private readonly string[] _javaStackTrace = new string[]{};

		public JavaException() { }
		public JavaException(string message) : base(message) { }
		public JavaException(string message, string[] javaStackTrace) : base(message) { _javaStackTrace = javaStackTrace; }
		public JavaException(string message, Exception inner) : base(message, inner) { }

		public string[] JavaStackTrace { get { return _javaStackTrace; } }
		protected JavaException(
		System.Runtime.Serialization.SerializationInfo info,
		System.Runtime.Serialization.StreamingContext context)
			: base(info, context) { }
	}
}
