using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Microsoft.DarkNotes.JniInterface
{
	[StructLayout(LayoutKind.Sequential)]
	struct JValue
	{
		private Int64 _value;

		public JValue(IntPtr ptr)
		{
			_value = ptr.ToInt64();
		}

		public JValue(byte value)
		{
			_value = value;
		}

		public JValue(short value)
		{
			_value = value;
		}

		public JValue(int value)
		{
			_value = value;
		}

		public JValue(long value)
		{
			_value = value;
		}

		public JValue(char value)
		{
			_value = value;
		}

		public JValue(float value)
		{
			_value = BitConverter.ToInt64(BitConverter.GetBytes(value).Concat(new byte[] { 0, 0, 0, 0 }).ToArray(), 0);
		}

		public JValue(double value)
		{
			_value = BitConverter.DoubleToInt64Bits(value);
		}

		public JValue(bool value)
		{
			_value = value ? 1 : 0;
		}

		public byte ToByte()
		{
			return (byte)_value;
		}

		public short ToInt16()
		{
			return (short)_value;
		}

		public int ToInt32()
		{
			return (int)_value;
		}

		public long ToInt64()
		{
			return _value;
		}

		public float ToFloat()
		{
			return BitConverter.ToSingle(BitConverter.GetBytes(_value), 0);
		}

		public double ToDouble()
		{
			return BitConverter.Int64BitsToDouble(_value);
		}

		public bool ToBoolean()
		{
			return _value != 0;
		}

		public IntPtr ToIntPtr()
		{
			return new IntPtr(_value);
		}

		public static implicit operator JValue(IntPtr value)
		{
			return new JValue(value);
		}

		public static implicit operator JValue(bool value)
		{
			return new JValue(value);
		}

		public static implicit operator JValue(byte value)
		{
			return new JValue(value);
		}

		public static implicit operator JValue(short value)
		{
			return new JValue(value);
		}

		public static implicit operator JValue(int value)
		{
			return new JValue(value);
		}

		public static implicit operator JValue(long value)
		{
			return new JValue(value);
		}

		public static implicit operator JValue(float value)
		{
			return new JValue(value);
		}

		public static implicit operator JValue(double value)
		{
			return new JValue(value);
		}

		public static implicit operator JValue(char value)
		{
			return new JValue(value);
		}
	}
}
