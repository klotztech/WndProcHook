// The following ifdef block is the standard way of creating macros which make exporting 
// from a DLL simpler. All files within this DLL are compiled with the HOOKMEUP_EXPORTS
// symbol defined on the command line. This symbol should not be defined on any project
// that uses this DLL. This way any other project whose source files include this file see 
// HOOKMEUP_API functions as being imported from a DLL, whereas this DLL sees symbols
// defined with this macro as being exported.
#ifdef HOOKMEUP_EXPORTS
#define HOOKMEUP_API __declspec(dllexport)
#else
#define HOOKMEUP_API __declspec(dllimport)
#endif

// This class is exported from the HookMeUp.dll

/*
class HOOKMEUP_API CHookMeUp {
public:
	CHookMeUp(void);
	// TODO: add your methods here.
};
*/

//extern HOOKMEUP_API int nHookMeUp;


extern "C" {
	HOOKMEUP_API bool WINAPI DllMain(HINSTANCE hInstDll, DWORD fdwReason, LPVOID lpvReserved);
	HOOKMEUP_API int SetWindowsHookRedirect(HWND hWndReceiver, HWND hWndTarget);
	HOOKMEUP_API BOOL GTFO();
}
