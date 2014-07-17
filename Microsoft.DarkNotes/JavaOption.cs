using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.DarkNotes.JniInterface;
using System.Globalization;

namespace Microsoft.DarkNotes
{
	/// <summary>
	/// An option to pass to the Java VM while creating it.
	/// </summary>
	public class JavaOption
	{
		private readonly JavaVMOption _actualOption;

		private JavaOption(string optionString)
		{
			_actualOption = new JavaVMOption(optionString);
		}

		/// <summary>
		/// Gets an options that defines a Java system property.
		/// </summary>
		/// <param name="propertyName">The name of the property, e.g. java.class.path</param>
		/// <param name="propertyValue">The value of the property, e.g. c:\myJars\*</param>
		/// <returns>The option.</returns>
		public static JavaOption DefineProperty(string propertyName, string propertyValue)
		{
			return new JavaOption(String.Format(CultureInfo.InvariantCulture, "-D{0}={1}", propertyName, propertyValue));
		}

		internal JavaVMOption ActualOption
		{
			get { return _actualOption; }
		}
	}
}
