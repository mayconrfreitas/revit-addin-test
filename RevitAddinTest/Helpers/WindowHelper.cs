using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace RevitAddinTest.Helpers
{
	public static class WindowHelper
	{
		public static void SetRevitAsOwner(Window window)
		{
			IntPtr revitHandle = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;
			WindowInteropHelper helper = new WindowInteropHelper(window)
			{
				Owner = revitHandle
			};
		}
	}
}
