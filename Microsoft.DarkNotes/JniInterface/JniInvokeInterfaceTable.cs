using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DarkNotes.JniInterface
{
	partial struct JavaVM
	{
		/// <summary>
		/// The interface table for all the function pointers in <see cref="JavaVM"/>.
		/// </summary>
		private enum JniInvokeInterfaceTable
		{
			DestroyJavaVM,
			AttachCurrentThread,
			DetachCurrentThread,
			GetEnv,
			AttachCurrentThreadAsDaemon,
		}
	}
}
