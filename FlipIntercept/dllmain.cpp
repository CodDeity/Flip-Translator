#include<iostream>
#include<Windows.h>
#include<string>

LRESULT CALLBACK KeyBoardHookProc(int nCode, WPARAM wp, LPARAM lp);
LRESULT wndProc(HWND wnd, UINT type, WPARAM wp, LPARAM lp);

static bool(*ClipCallBack)(char*);
static bool(*KeyBoardCallBack)(int);
HHOOK globalHook = NULL;
bool isHooked = false;

extern"C" {
	__declspec(dllexport) void RunClipboardLoop(bool(*ClipboardCallBack)(char*)) {
		(ClipCallBack) = ClipboardCallBack;
		WNDCLASSA _wcx = {};
		const char* className = "clipintercept";
		_wcx.lpszClassName = className;
		_wcx.hInstance = NULL;
		_wcx.lpfnWndProc = wndProc;
		if (RegisterClassA(&_wcx)) {
			HWND hWnd = CreateWindowA(className, "WNDCLIP", WS_OVERLAPPED, 0, 0, 0, 0, NULL, NULL, NULL, NULL);
			if (hWnd != NULL) {
				MSG msg;
				while (GetMessageA(&msg, hWnd, 0, 0))
				{
					TranslateMessage(&msg);
					DispatchMessageA(&msg);
				}
			}
		}
		else
		{
			(ClipCallBack)(NULL);
		}
	}
	__declspec(dllexport) void RunKeyIntercept(bool(*KeyInterceptCallBack)(int)) {
		KeyBoardCallBack = KeyInterceptCallBack;
		globalHook = SetWindowsHookExA(WH_KEYBOARD_LL, KeyBoardHookProc,NULL,0);
		if (globalHook == NULL) {
			int ERNO = GetLastError();
			if (ERNO < 0)
				ERNO = -1 * ERNO;
			(KeyBoardCallBack)(-1 * ERNO);
		}
	}
	__declspec(dllexport) void StopKeyIntercept(bool(*KeyInterceptCallBack)(int)) {
		if (globalHook != NULL) {
			UnhookWindowsHookEx(globalHook);
			globalHook = NULL;
		}
	}
}
LRESULT CALLBACK KeyBoardHookProc(int nCode, WPARAM wp, LPARAM lp) {
	if (nCode == HC_ACTION) {
		KBDLLHOOKSTRUCT* keyInfo = (KBDLLHOOKSTRUCT*)lp;
		if (wp == WM_KEYUP) {
			DWORD key = keyInfo->vkCode;
			(KeyBoardCallBack)(key);
		}
	}
	return CallNextHookEx(globalHook, nCode, wp, lp);
}

char* CatchData() {
	if (OpenClipboard(NULL) != 0) {
		if (IsClipboardFormatAvailable(CF_TEXT))
		{
			if (HANDLE cldh = GetClipboardData(CF_TEXT)) {
				if (cldh != nullptr) {
					if (char* cldata = (char*)GlobalLock(cldh)) {
						size_t clen = lstrlenA(cldata);
						char* tmp = new char[clen];
						lstrcpyA(tmp, cldata);
						GlobalUnlock(cldh);
						CloseClipboard();
						return tmp;
					}
					else {
						CloseClipboard();
						return NULL;
					}
				}
				else
				{
					CloseClipboard();
					return NULL;
				}
			}
			else {
				CloseClipboard();
				return NULL;
			}
		}
		else
		{
			CloseClipboard();
			return NULL;
		}
	}
	else
	{
		return NULL;
	}
}

LRESULT wndProc(HWND wnd, UINT type, WPARAM wp, LPARAM lp) {
	switch (type)
	{
	case WM_CREATE:
		AddClipboardFormatListener(wnd);
		break;
	case WM_DESTROY:
		RemoveClipboardFormatListener(wnd);
		break;
	case WM_CLIPBOARDUPDATE:
		if (!(ClipCallBack)(CatchData())) {
			DestroyWindow(wnd);
		}
	}
	return DefWindowProcA(wnd, type, wp, lp);
}