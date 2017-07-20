using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HookTest {
	public partial class Form1 : Form {

		[DllImport("HookMeUp.dll", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
		static extern int SetWindowsHookRedirect(IntPtr hWndReceiver, IntPtr hWndTarget);

		[DllImport("HookMeUp.dll", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
		static extern int GTFO();

		[StructLayout(LayoutKind.Sequential)]
		public struct CWPSTRUCT {
			public IntPtr lParam;
			public IntPtr wParam;
			public uint message;
			public IntPtr hwnd;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct COPYDATASTRUCT {
			public IntPtr dwData;
			public int cbData;
			public IntPtr lpData;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct MSG {
			public IntPtr hwnd;
			public uint message;
			public IntPtr lParam;
			public IntPtr wParam;
			public uint time;
			public uint x;
			public uint y;
		}

		const int WM_COPYDATA = 0x004A;

		public Form1() {
			InitializeComponent();

			this.Load += Form1_Load;
			this.FormClosing += Form1_FormClosing;
		}

		private void Form1_FormClosing(object sender, FormClosingEventArgs e) {
			int gtfo = GTFO();
			MessageBox.Show("GTFO'ed: " + gtfo);
		}

		private void Form1_Load(object sender, EventArgs e) {
			IntPtr hWnd = Process.GetProcessesByName("vncviewer").First().MainWindowHandle;
			hWnd = new IntPtr(0x000E2F4E);
			int x = SetWindowsHookRedirect(this.Handle, hWnd/*new IntPtr(0x000727A6)*/);
			Text += " ret=" + x;
			Console.WriteLine("native call returned: {0}", x);
		}

		protected /*override */void WndProc1(ref Message data) {
			if (data.Msg == WM_COPYDATA) {
				IntPtr hwnd = data.WParam;
				COPYDATASTRUCT copyData = Marshal.PtrToStructure<COPYDATASTRUCT>(data.LParam);

				if (copyData.cbData != Marshal.SizeOf(typeof(CWPSTRUCT)))
					Console.WriteLine("Payload length missmatched! COPYDATASTRUCT.cbData != sizeof(CWPSTRUCT)");

				CWPSTRUCT msg = Marshal.PtrToStructure<CWPSTRUCT>(copyData.lpData);

				WM message = (WM)msg.message;
				//if (message == WM.PAINT || message == WM.NCPAINT || message == WM.GETICON || message == WM.PRINT || message == WM.PRINTCLIENT
				//	|| message == WM.NCHITTEST || message == WM.SETCURSOR
				//	|| (msg.message >= 0x0132 && msg.message <= 0x0138)
				//	|| message == WM.ERASEBKGND)
				//	return;

				//if (message != WM.LBUTTONDBLCLK && message != WM.LBUTTONDOWN && message != WM.LBUTTONUP)
				//	return;

				string type = Enum.IsDefined(typeof(WM), message) ? "WM_" + Enum.GetName(typeof(WM), message) : "0x" + msg.message.ToString("X8");
				Console.WriteLine("WINFORMS hwnd={0:X8} msg={1}\tlParam={2:X8} wParam={3:X8}", msg.hwnd, type, msg.lParam, msg.wParam);
			} else {
				base.WndProc(ref data);
			}
		}

		protected override void WndProc(ref Message data) {
			if (data.Msg == WM_COPYDATA) {
				IntPtr hwnd = data.WParam;
				COPYDATASTRUCT copyData = Marshal.PtrToStructure<COPYDATASTRUCT>(data.LParam);

				if (copyData.cbData != Marshal.SizeOf(typeof(MSG)))
					Console.WriteLine("Payload length missmatched! COPYDATASTRUCT.cbData != sizeof(MSG)");

				MSG msg = Marshal.PtrToStructure<MSG>(copyData.lpData);

				WM message = (WM)msg.message;
				if (message == WM.TIMER)
					return;

				string type = Enum.IsDefined(typeof(WM), message) ? "WM_" + Enum.GetName(typeof(WM), message) : "0x" + msg.message.ToString("X8");
				Console.WriteLine("WINFORMS hwnd={0:X8} msg={1}\tlParam={2:X8} wParam={3:X8}", msg.hwnd, type, msg.lParam, msg.wParam);
			} else {
				base.WndProc(ref data);
			}
		}

		private void button1_Click(object sender, EventArgs e) {
			try {
				var proc = Process.GetCurrentProcess();
				Injector.Inject(proc, "HookMeUp.dll");
				Console.WriteLine("Injerktion performanced");
			} catch (Exception exc) {
				Console.WriteLine(exc);
				MessageBox.Show("Shit's mangled");
			}
		}
	}
}
