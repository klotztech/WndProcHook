using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace HookTest {
	public class Injector {
		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern IntPtr OpenProcess(ProcessAccessFlags dwDesiredAccess, bool bInheritHandle, int dwProcessId);
		public static IntPtr OpenProcess(Process proc, ProcessAccessFlags flags) {
			return OpenProcess(flags, false, proc.Id);
		}

		[DllImport("kernel32.dll", SetLastError = true)]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		[SuppressUnmanagedCodeSecurity]
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern bool CloseHandle(IntPtr hObject);

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern IntPtr GetModuleHandle(string lpModuleName);

		[DllImport("kernel32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
		static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

		[DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
		static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, IntPtr dwSize, AllocationType flAllocationType, MemoryProtection flProtect);

		[DllImport("kernel32.dll", SetLastError = true)]
		static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, IntPtr nSize, out IntPtr lpNumberOfBytesWritten);

		[DllImport("kernel32.dll", SetLastError = true)]
		static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, out IntPtr lpThreadId);

		[Flags]
		public enum ProcessAccessFlags : uint {
			All = 0x001F0FFF,
			Terminate = 0x00000001,
			CreateThread = 0x00000002,
			VirtualMemoryOperation = 0x00000008,
			VirtualMemoryRead = 0x00000010,
			VirtualMemoryWrite = 0x00000020,
			DuplicateHandle = 0x00000040,
			CreateProcess = 0x000000080,
			SetQuota = 0x00000100,
			SetInformation = 0x00000200,
			QueryInformation = 0x00000400,
			QueryLimitedInformation = 0x00001000,
			Synchronize = 0x00100000
		}

		[Flags]
		public enum AllocationType : uint {
			Commit = 0x1000,
			Reserve = 0x2000,
			Decommit = 0x4000,
			Release = 0x8000,
			Reset = 0x80000,
			Physical = 0x400000,
			TopDown = 0x100000,
			WriteWatch = 0x200000,
			LargePages = 0x20000000
		}

		[Flags]
		public enum MemoryProtection : uint {
			Execute = 0x10,
			ExecuteRead = 0x20,
			ExecuteReadWrite = 0x40,
			ExecuteWriteCopy = 0x80,
			NoAccess = 0x01,
			ReadOnly = 0x02,
			ReadWrite = 0x04,
			WriteCopy = 0x08,
			GuardModifierflag = 0x100,
			NoCacheModifierflag = 0x200,
			WriteCombineModifierflag = 0x400
		}

		public static void Inject(Process targetProcess, string dllName) {
			// the target process - I'm using a dummy process for this
			// if you don't have one, open Task Manager and choose wisely
			//Process targetProcess = Process.GetProcessesByName("testApp")[0];

			// geting the handle of the process - with required privileges
			IntPtr procHandle = OpenProcess(targetProcess, ProcessAccessFlags.CreateThread
				| ProcessAccessFlags.QueryInformation | ProcessAccessFlags.VirtualMemoryOperation
				| ProcessAccessFlags.VirtualMemoryWrite | ProcessAccessFlags.VirtualMemoryRead);
			if (procHandle == IntPtr.Zero)
				throw new Win32Exception();

			// searching for the address of LoadLibraryA and storing it in a pointer
			IntPtr loadLibraryAddr = GetProcAddress(GetModuleHandle("kernel32.dll"), "LoadLibraryA");
			if (loadLibraryAddr == IntPtr.Zero)
				throw new Win32Exception();

			// alocating some memory on the target process - enough to store the name of the dll
			// and storing its address in a pointer
			IntPtr allocMemSize = new IntPtr((dllName.Length + 1) * Marshal.SizeOf(typeof(char)));
			IntPtr allocMemAddress = VirtualAllocEx(procHandle, IntPtr.Zero, allocMemSize, AllocationType.Commit | AllocationType.Reserve, MemoryProtection.ReadWrite);
			if (allocMemAddress == IntPtr.Zero)
				throw new Win32Exception();

			// writing the name of the dll there
			IntPtr bytesWritten;
			if (!WriteProcessMemory(procHandle, allocMemAddress, Encoding.Default.GetBytes(dllName), allocMemSize, out bytesWritten))
				throw new Win32Exception();

			// creating a thread that will call LoadLibraryA with allocMemAddress as argument
			IntPtr threadId;
			IntPtr hThread = CreateRemoteThread(procHandle, IntPtr.Zero, 0, loadLibraryAddr, allocMemAddress, 0, out threadId);
			if (hThread == IntPtr.Zero)
				throw new Win32Exception();

			if (!CloseHandle(procHandle))
				throw new Win32Exception();
		}
	}
}
