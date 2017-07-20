using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HookTest {
	class Program {

		[DllImport("user32.dll")]
		static extern uint GetWindowThreadProcessId(IntPtr hWnd, IntPtr ProcessId);

		[DllImport("user32.dll", SetLastError = true)]
		static extern IntPtr SetWindowsHookEx(HookType hookType, HookProc lpfn, IntPtr hMod, uint dwThreadId);
		delegate IntPtr HookProc(int code, IntPtr wParam, IntPtr lParam);

		[StructLayout(LayoutKind.Sequential)]
		public struct CWPSTRUCT {
			public IntPtr lParam;
			public IntPtr wParam;
			public int msg;
			public IntPtr hwnd;
		}

		[DllImport("user32.dll")]
		static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

		[DllImport("user32.dll")]
		static extern int GetMessage(out IntPtr lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);
		[DllImport("user32.dll")]
		static extern bool TranslateMessage([In] ref IntPtr lpMsg);
		[DllImport("user32.dll")]
		static extern IntPtr DispatchMessage([In] ref IntPtr lpmsg);

		public enum HookType : int {
			WH_JOURNALRECORD = 0,
			WH_JOURNALPLAYBACK = 1,
			WH_KEYBOARD = 2,
			WH_GETMESSAGE = 3,
			WH_CALLWNDPROC = 4,
			WH_CBT = 5,
			WH_SYSMSGFILTER = 6,
			WH_MOUSE = 7,
			WH_HARDWARE = 8,
			WH_DEBUG = 9,
			WH_SHELL = 10,
			WH_FOREGROUNDIDLE = 11,
			WH_CALLWNDPROCRET = 12,
			WH_KEYBOARD_LL = 13,
			WH_MOUSE_LL = 14
		}

		//private static IntPtr hook;
		//private static HookProc hookProc;

		static void Main(string[] args) {
			Console.SetWindowSize(Console.BufferWidth * 2, Console.WindowHeight);
			IntPtr hWnd = Process.GetCurrentProcess().MainWindowHandle;
			/*
			uint threadId = GetWindowThreadProcessId(hWnd, IntPtr.Zero);
			Console.WriteLine("hooking WndProc... hWnd=0x{0:X8}, threadId=0x{1}", hWnd.ToInt64(), threadId);

			hookProc = new HookProc(CallWndProc);
			hook = SetWindowsHookEx(HookType.WH_CALLWNDPROC, hookProc, IntPtr.Zero, threadId);
			if (hook == IntPtr.Zero)
				throw new Win32Exception();
			Console.WriteLine("HHOOK hook = 0x{0:X8}", hook);
			*/

			//int x = SetWindowsHookRedirect(hWnd, /*hWnd*/new IntPtr(0x000A26A0));
			//Console.WriteLine("native call returned: {0}", x);

			//IntPtr msg;
			//while (GetMessage(out msg, IntPtr.Zero, 0, 0) > 0) {
			//	Console.WriteLine("pump");
			//	TranslateMessage(ref msg);
			//	DispatchMessage(ref msg);
			//}

			Application.Run(new Form1());

			//int gtfo = GTFO();
			//Console.WriteLine("GTFO'ed -> {0}", gtfo);
			//Console.ReadKey();
		}

		//private static IntPtr CallWndProc(int nCode, IntPtr wParam, IntPtr lParam) {
		//	//try {
		//	//	Console.WriteLine("parentHWnd_WndProc WM_{0} {1:X8} {2:X8}\t viewportHWnd={3:X8} parentHWnd={4:X8} hwnd={5:X8} {6}", ((WM)args.msg).ToString(), args.wParam.ToInt64(), args.lParam.ToInt64(), viewportHWnd.ToInt64(), parentHWnd.ToInt64(), args.hwnd.ToInt64(), code);
		//	//} catch (Exception) { }

		//	Console.WriteLine("CallWndProc: {0} {1:X8} {2:X8}", nCode, wParam.ToInt64(), lParam.ToInt64());

		//	if (nCode >= 0)
		//		return CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
		//	return IntPtr.Zero;
		//}
	}
}
