#include "Recorder.h"
using namespace std;

HINSTANCE hInst;
WCHAR szTitle[] = L"Recorder";
WCHAR szMenuName[] = L"Recorder";
WCHAR szWindowClass[] = L"Recorder";

UINT cbSz = 8192 * 8;
LPRAWINPUT ri;

int InitConsole()
{
    FILE *fConsole;

	if (!AllocConsole()) {
		return 1;
	}
	
	freopen_s(&fConsole, "CONOUT$", "w", stdout);
	freopen_s(&fConsole, "CONIN$", "r", stdin);
	//freopen_s(&fConsole, "CONERR$", "w", stderr);
	
	cout.clear();
	ios::sync_with_stdio();
    return 0;
}

int
InitRecorder(
    HWND hWnd
) {
    if (InitConsole())
        return 1;
	// TODO: Probably not buggy, comment out to troubleshoot MinGW
	// issue.
    //SetWindowsHookExW(WH_MOUSE_LL, LowLevelMouseProc, NULL, 0);

	// N.B. proper usage of the API requires a first call to determine size
	// of data. We allocate a large buffer and skip that.
	ri = (LPRAWINPUT)malloc(cbSz);
    RAWINPUTDEVICE rids[] = {
        {
            0x01, // .UsagePage = Generic
            0x02, // .Usage = Mouse
			// Can set NOLEGACY but then GUI elements do not update.
			// This seems to not happen with C#.
			// It also seems to not matter if you don't need a GUI.
            RIDEV_INPUTSINK, // .Flags,
            hWnd, // .WindowHandle
        },
        {
            0x01, // .UsagePage = Generic
            0x05, // .Usage = Gamepad
            RIDEV_INPUTSINK, // .Flags
            hWnd, // .WindowHandle
        },
        // https://docs.microsoft.com/en-us/windows-hardware/design/component-guidelines/supporting-usages-in-digitizer-report-descriptors
        {
            0x0d, // .UsagePage = Digitizer
            0x04, // .Usage = ??? (Pen is 0x02 integrated  or 0x01 external.)
            RIDEV_INPUTSINK, // .Flags
            hWnd,
        },
    };
	// Keyboards need RIDEV_NOHOTKEYS.

    RegisterRawInputDevices(
        rids, sizeof(rids) / sizeof(rids[0]),
        sizeof(RAWINPUTDEVICE)
    );
    return 0;
}

int APIENTRY
wWinMain(
	_In_	 HINSTANCE	hInstance,
	_In_opt_ HINSTANCE	hPrevInstance,
	_In_	 LPWSTR		lpCmdLine,
	_In_ 	 int		nCmdShow
)
{
	WNDCLASSEXW wcex;
	HWND hWnd;
	MSG msg;
	
	UNREFERENCED_PARAMETER(hPrevInstance);
	UNREFERENCED_PARAMETER(lpCmdLine);
	
	wcex.cbSize			= sizeof(WNDCLASSEX);
	wcex.style			= CS_HREDRAW | CS_VREDRAW;
	wcex.lpfnWndProc	= WndProc;
	wcex.cbClsExtra		= 0;
	wcex.cbWndExtra		= 0;
	wcex.hInstance		= hInstance;
	wcex.hIcon			= LoadIconW(hInstance, IDI_APPLICATION);
	wcex.hCursor		= LoadCursorW(NULL, IDC_ARROW);
	wcex.hbrBackground	= (HBRUSH)(COLOR_WINDOW + 1);
	wcex.lpszMenuName	= szMenuName;
	wcex.lpszClassName	= szWindowClass;
	wcex.hIconSm	    = LoadIconW(wcex.hInstance, IDI_APPLICATION);
	if (!RegisterClassExW(&wcex)) {
		// TODO: Error handling.
	}
	
	hWnd = CreateWindowExW(WS_EX_CLIENTEDGE, szWindowClass, szTitle,
		WS_OVERLAPPEDWINDOW, CW_USEDEFAULT, CW_USEDEFAULT, CW_USEDEFAULT,
		CW_USEDEFAULT, NULL, NULL, hInstance, NULL);
	if (!hWnd) {
		// TODO: Error handling.
	}
	
	while (GetMessageW(&msg, NULL, 0, 0)) {
		TranslateMessage(&msg);
		DispatchMessageW(&msg);
	}
	
	return (int)msg.wParam;
}

LRESULT CALLBACK
WndProc(
	_In_ HWND	hWnd,
	_In_ UINT	message,
	_In_ WPARAM	wParam,
	_In_ LPARAM	lParam
)
{
	UINT dwSize;
	LPBYTE lpData;
	FILETIME ft;
	
	// Take the timestamp before any processing is done.
	GetSystemTimePreciseAsFileTime(&ft);
	switch (message) {
	case WM_CREATE: {
        if (InitRecorder(hWnd)) {
            // TODO: Error handling.
            PostQuitMessage(0);
        }
		break;
	}
	case WM_DESTROY: {
		PostQuitMessage(0);
		break;
	}
	case WM_INPUT: {
		// TODO: Evaluate GetRawInputBuffer.
		int rc = GetRawInputData(
			(HRAWINPUT)lParam, RID_INPUT,
			ri, &cbSz,
			sizeof(RAWINPUTHEADER)
		);
		wprintf(L"%d %d\n", rc, GetLastError());

		return RawInputProc(&ri, 1, sizeof(RAWINPUTHEADER));
        //return RawInputProc(hWnd, message, lParam, wParam);
		//return DefRawInputProc(&ri, 1, dwSize);
        // Documentation implies the above call is pointless.
	}
	default: {
		return DefWindowProcW(hWnd, message, wParam, lParam);
	}
	}
	
	return 0;
}

LRESULT CALLBACK
LowLevelMouseProc(
    int nCode,
    WPARAM wParam,
    LPARAM lParam
) {
    return CallNextHookEx(NULL, nCode, wParam, lParam);
}

LRESULT CALLBACK RawInputProc(
	PRAWINPUT *paRawInput,
	INT		  nInput,
	UINT	  cbSizeHeader
) {
	PRAWINPUT ri = *paRawInput;
	switch(ri->header.dwType) {
		case RIM_TYPEMOUSE: {
			MouseRecord mc;
			break;
		}
		case RIM_TYPEKEYBOARD: {
			KeyboardRecord kc;
			break;
		}
		case RIM_TYPEHID: {
			HidRecord hc;
			break;
		}
	}
	return 0;
}