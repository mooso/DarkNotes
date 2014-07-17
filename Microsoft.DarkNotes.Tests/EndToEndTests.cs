using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace Microsoft.DarkNotes.Tests
{
	[TestClass]
	public class EndToEndTests
	{
		[TestMethod]
		public void ToStringTest()
		{
			using (dynamic java = CreateVm())
			{
				dynamic s = java.String.@new("Hello world");
				Assert.AreEqual("hello world", (string)s.toLowerCase());
			}
		}

		[TestMethod]
		public void StaticMethodTest()
		{
			using (dynamic java = CreateVm())
			{
				Assert.AreEqual("X.Y", (string)java.String.valueOf("X.Y"));
				Assert.AreEqual(java.System.@out.hashCode(), java.System.identityHashCode(java.System.@out));
			}
		}

		[TestMethod]
		public void PrimitiveValuesTest()
		{
			using (dynamic java = CreateVm())
			{
				Assert.AreEqual(3, java.Byte.@new((byte)3).byteValue());
				Assert.AreEqual(3, java.Short.@new((short)3).shortValue());
				Assert.AreEqual(3, java.Integer.@new(3).intValue());
				Assert.AreEqual(3, java.Long.@new((long)3).longValue());
				Assert.AreEqual(3.2f, java.Float.@new(3.2f).floatValue());
				Assert.AreEqual(3.2, java.Double.@new(3.2).doubleValue());
				Assert.AreEqual(true, java.Boolean.@new(true).booleanValue());
				Assert.AreEqual(false, java.Boolean.@new(false).booleanValue());
				Assert.AreEqual('c', java.Character.@new('c').charValue());
			}
		}

		[TestMethod]
		public void NullTest()
		{
			using (dynamic java = CreateVm())
			{
				Assert.AreEqual("java.lang.String", (string)java.String.@class.getName());
				Assert.IsTrue(java.String.@class.isInstance("x"));
				Assert.IsFalse(java.String.@class.isInstance(null));
				dynamic map = java.HashMap.@new();
				map.put(null, "x");
				Assert.AreEqual("x", (string)map.get(null));
			}
		}

		[TestMethod]
		public void ExceptionTest()
		{
			using (dynamic java = CreateVm())
			{
				try
				{
					java.System.getProperty(null);
					Assert.Fail("Should've thrown.");
				}
				catch (JavaException ex)
				{
					StringAssert.Contains(ex.Message, "NullPointerException");
				}
			}
		}

		[TestMethod]
		public void FileIOTest()
		{
			using (dynamic java = CreateVm())
			{
				string fileName = Path.GetTempFileName();
				dynamic writer = java.PrintWriter.@new(java.FileWriter.@new(fileName));
				try
				{
					writer.println("a.b");
				}
				finally
				{
					if (writer != null)
					{
						writer.close();
					}
				}
				try
				{
					Assert.AreEqual("a.b\r\n", File.ReadAllText(fileName));
				}
				finally
				{
					if (File.Exists(fileName))
					{
						File.Delete(fileName);
					}
				}
			}
		}

		[TestMethod]
		public void OptionsTest()
		{
			using (dynamic java = CreateVm())
			{
				Assert.AreEqual("rocks", (string)java.System.getProperty("dark.notes"));
				Assert.AreEqual("high", (string)java.System.getProperty("dark.notes.rocking"));
			}
		}

		[TestMethod]
		public void MultiThreadedTest()
		{
			using (dynamic java = CreateVm())
			{
				List<Exception> allExceptions = new List<Exception>();
				var threads = Enumerable.Range(1, 10).Select(i => new Thread(() =>
					{
						try
						{
							Assert.AreEqual(3, java.Integer.@new(3).intValue());
						}
						catch (Exception ex)
						{
							lock (allExceptions)
							{
								allExceptions.Add(ex);
							}
						}
						finally
						{
							java.DetachThread();
						}
					})).ToList();
				threads.ForEach(t => t.Start());
				threads.ForEach(t => t.Join());
				if (allExceptions.Any())
				{
					throw new AggregateException(allExceptions.ToArray());
				}
			}
		}

		[TestMethod]
		public void ArrayParameter()
		{
			using (dynamic java = CreateVm())
			{
				Assert.AreEqual("10", (string)java.Arrays.asList(new[] { "5", "10", "20" }).get(1));
				var split = java.String.@new("foo:bar").split(":");
				Assert.AreEqual("bar", (string)split[1]);
				Assert.AreEqual("bar", (string)java.Arrays.asList(split).get(1));
			}
		}
		
		[TestMethod]
		public void ArrayPrimitives()
		{
			using (dynamic java = CreateVm())
			{
				Assert.AreEqual("abc", (string)java.String.copyValueOf(new[] { 'a', 'b', 'c' }));
				Assert.AreEqual(true, (bool)java.Arrays.copyOf(new[] { false, true }, 2)[1]);
				Assert.AreEqual((byte)1, (byte)java.Arrays.copyOf(new byte[] { 20, 1 }, 2)[1]);
				Assert.AreEqual((short)1, (short)java.Arrays.copyOf(new short[] { 20, 1 }, 2)[1]);
				Assert.AreEqual((int)1, (int)java.Arrays.copyOf(new int[] { 20, 1 }, 2)[1]);
				Assert.AreEqual((long)1, (long)java.Arrays.copyOf(new long[] { 20, 1 }, 2)[1]);
				Assert.AreEqual(1.1f, (float)java.Arrays.copyOf(new float[] { 20, 1.1f }, 2)[1]);
				Assert.AreEqual(1.1, (double)java.Arrays.copyOf(new double[] { 20, 1.1 }, 2)[1]);
				Assert.AreEqual('c', (char)java.Arrays.copyOf(new char[] { 'a', 'c' }, 2)[1]);
			}
		}

		[TestMethod]
		public void BoxingOfPrimitives()
		{
			using (dynamic java = CreateVm())
			{
				var list = java.ArrayList.@new();
				list.add(1);
				Assert.AreEqual(1, (int)list.get(0));
			}
		}

		[TestMethod]
		public void SimpleArrays()
		{
			using (dynamic java = CreateVm())
			{
				var array = java.newArray(new[] { 1, 2 });
				Assert.AreEqual(2, array.length);
				Assert.AreEqual(1, array[0]);
			}
		}

		private dynamic CreateVm()
		{
			dynamic ret = DarkJava.CreateVm(attemptVmReuse: true,
				options: new[] { JavaOption.DefineProperty("dark.notes", "rocks"), JavaOption.DefineProperty("dark.notes.rocking", "high") }
			);
			ret.ImportPackage("java.io");
			ret.ImportPackage("java.lang");
			ret.ImportPackage("java.text");
			ret.ImportPackage("java.util");
			return ret;
		}
	}
}
