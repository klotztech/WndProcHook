// HookMeUp.cpp : Defines the exported functions for the DLL application.
//

#include "stdafx.h"
#include "HookMeUp.h"


// This is an example of an exported variable
//HOOKMEUP_API int nHookMeUp=0;

// This is an example of an exported function.
#pragma data_seg(".shared")
LRESULT CALLBACK HookProc(int nCode, WPARAM wParam, LPARAM lParam);
LRESULT CALLBACK HookProc2(int nCode, WPARAM wParam, LPARAM lParam);
HHOOK hook = NULL;
HINSTANCE hMod = NULL;
HWND g_hWndReceiver = NULL;
#pragma data_seg()
#pragma comment(linker, "/SECTION:.shared,RWS")

extern "C" {
	HOOKMEUP_API int SetWindowsHookRedirect(HWND hWndReceiver, HWND hWndTarget)
	{
		g_hWndReceiver = hWndReceiver;
		DWORD processId = 0;
		DWORD threadId = GetWindowThreadProcessId(hWndTarget, &processId);
		DWORD currThreadId = GetCurrentThreadId();
		printf("SetWindowsHookRedirect(hWndReceiver=%#10x, hWndTarget=%#10x): currThreadId=%d -> threadId=%d processId=%d -> ", (DWORD)hWndReceiver, (DWORD)hWndTarget, currThreadId, threadId, processId);
		//hook = SetWindowsHookEx(WH_CALLWNDPROC, HookProc, hMod, threadId /*GetCurrentThreadId()*/);
		hook = SetWindowsHookEx(WH_GETMESSAGE, HookProc2, hMod, threadId);
		printf("SetWindowsHookEx(WH_GETMESSAGE,..): hook=%#10x\n", (DWORD)hook);
		if (hook == 0)
			printf("WAT: hook == 0 (!)\n");

		return GetLastError();
	}

	HOOKMEUP_API BOOL GTFO()
	{
		printf("GTFO: UnhookWindowsHookEx... hook=%#10x\n", (DWORD)hook);
		BOOL ret = FALSE;
		if (hook != NULL)
			ret = UnhookWindowsHookEx(hook);
		hook = NULL;
		return ret;
	}

	HOOKMEUP_API bool WINAPI DllMain(HINSTANCE hInstDll, DWORD fdwReason, LPVOID lpvReserved)
	{
		switch (fdwReason)
		{
		case DLL_PROCESS_ATTACH:
			hMod = hInstDll;
			printf("DllMain carled: hInstDll=%#10x\n", (DWORD)hInstDll);
			//MessageBox(NULL, L"Hello World!", L"Dll says:", MB_OK);
			break;

		case DLL_PROCESS_DETACH:
			printf("DLL_PROCESS_DETACH: UnhookWindowsHookEx... hook=%#10x\n", (DWORD)hook);
			if (hook != NULL)
				UnhookWindowsHookEx(hook);
			break;

		case DLL_THREAD_ATTACH:
			break;

		case DLL_THREAD_DETACH:
			break;
		}
		return true;
	}
}

LRESULT CALLBACK HookProc(int nCode, WPARAM wParam, LPARAM lParam) {
	//printf("%d hookproc called\n", GetLastError());

	printf("HookProc(): nCode=%d wParam=%#10x lParam=%#10x g_hWndReceiver=%#10x", nCode, wParam, lParam, (DWORD)g_hWndReceiver);

	CWPSTRUCT *wndMsg = (CWPSTRUCT *)lParam;

	printf("\tCWPSTRUCT: message=%d\r", wndMsg->message);

	// no giant loop plz, don't be dat guy
	if (wndMsg->hwnd != g_hWndReceiver) {
		COPYDATASTRUCT copyData;
		copyData.dwData = 13;
		copyData.cbData = sizeof(CWPSTRUCT);
		copyData.lpData = wndMsg;
		LRESULT resCopyData = SendMessage(g_hWndReceiver, WM_COPYDATA, (WPARAM)wndMsg->hwnd, (LPARAM)&copyData);
		if (resCopyData == 0)
			printf("\nSendMessage(WM_COPYDATA) returned %d !\n", resCopyData);
	}

	if (nCode >= 0)
		return CallNextHookEx(hook, nCode, wParam, lParam);
	else
		return CallNextHookEx(0, nCode, wParam, lParam);

	return 0;
}

LRESULT CALLBACK HookProc2(int nCode, WPARAM wParam, LPARAM lParam) {
	//printf("%d hookproc called\n", GetLastError());

	printf("HookProc2(): nCode=%d wParam=%#10x lParam=%#10x g_hWndReceiver=%#10x", nCode, wParam, lParam, (DWORD)g_hWndReceiver);

	MSG *wndMsg = (MSG *)lParam;

	printf("\tMSG: time=%ul message=%d\r", wndMsg->time, wndMsg->message);

	// no giant loop plz, don't be dat guy
	if (wndMsg->hwnd != g_hWndReceiver) {
		COPYDATASTRUCT copyData;
		copyData.dwData = 13;
		copyData.cbData = sizeof(MSG);
		copyData.lpData = wndMsg;
		/*BOOL resCopyData = PostMessage(g_hWndReceiver, WM_COPYDATA, (WPARAM)wndMsg->hwnd, (LPARAM)&copyData);
		if (resCopyData == 0)
			printf("\PostMessage(WM_COPYDATA) returned %d! GetLastError(): %ul\n", resCopyData, GetLastError());*/
		LRESULT resCopyData = SendMessage(g_hWndReceiver, WM_COPYDATA, (WPARAM)wndMsg->hwnd, (LPARAM)&copyData);
		if (resCopyData == 0)
			printf("\n[HookProc2] SendMessage(WM_COPYDATA) returned %d!\n", resCopyData);
	}

	if (nCode >= 0)
		return CallNextHookEx(hook, nCode, wParam, lParam);
	else
		return CallNextHookEx(0, nCode, wParam, lParam);

	return 0;
}

/*
// This is the constructor of a class that has been exported.
// see HookMeUp.h for the class definition
CHookMeUp::CHookMeUp()
{
	return;
}
*/

//int WINAPI WinMain(HINSTANCE hInstance, HINSTANCE hPrevInstance, PSTR pCmdLine, int nCmdShow) {
//	
//	printf("WinMain rulez da world hInstance=%#10x\n", (unsigned int)hInstance);
//	/*if (!(HMODULE hDll = LoadLibrary("library.dll")))
//		return 1;
//	if (!(LPGetMsgProc pfnProc = (LPGetMsgProc)GetProcAddress(hDll, "GetMsgProc@12")))
//		return 2;
//
//	HHOOK hMsgHook = SetWindowsHookEx(WH_GETMESSAGE, pfnProc, hInstance, 0);
//
//	MSG msg;
//	while (GetMessage(&msg, NULL, 0, 0) > 0) {}
//
//	UnhookWindowsHookEx(hMsgHook);
//	*/
//	return 0;
//}