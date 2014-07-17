using System;
using System.Runtime.InteropServices;

namespace Microsoft.Experimental.DarkNotes.JniInterface
{
	/// <summary>
	/// Helper class for creating a pointer to a struct by copying it into HGlobal memory and deallocating that on dispose.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	internal class StructPtr<T> : IDisposable
		where T: struct
	{
		private IntPtr _pointer;

		/// <summary>
		/// Copies the struct to memory and creates the pointer.
		/// </summary>
		/// <param name="s"></param>
		public StructPtr(T s)
		{
			_pointer = Marshal.AllocHGlobal(Marshal.SizeOf(s));
			Marshal.StructureToPtr(s, _pointer, false);
		}

		/// <summary>
		/// The pointer.
		/// </summary>
		public IntPtr Pointer
		{
			get { return _pointer; }
		}

		/// <summary>
		/// Convenience implicit conversion to <see cref="IntPtr"/>.
		/// </summary>
		/// <param name="p"></param>
		/// <returns></returns>
		static public implicit operator IntPtr(StructPtr<T> p)
		{
			return p.Pointer;
		}

		/// <summary>
		/// Deallocates the memory used.
		/// </summary>
		public void Dispose()
		{
			Marshal.DestroyStructure(_pointer, typeof(T));
			Marshal.FreeHGlobal(_pointer);
		}
	}

	/// <summary>
	/// Factory class for <see cref="StructPtr{T}"/>.
	/// </summary>
	internal static class StructPtr
	{
		/// <summary>
		/// Creates a pointer to the given struct.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="s"></param>
		/// <returns></returns>
		public static StructPtr<T> Create<T>(T s)
			where T: struct
		{
			return new StructPtr<T>(s);
		}
	}
}
