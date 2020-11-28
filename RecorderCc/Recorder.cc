#include "Recorder.h"
using namespace std;
using namespace boost::iostreams;

HINSTANCE hInst;
WCHAR szTitle[] = L"Recorder";
WCHAR szMenuName[] = L"Recorder";
WCHAR szWindowClass[] = L"Recorder";

UINT cbSz = 8192 * 8;
LPRAWINPUT ri;
filtering_ostream zfs;
ofstream fs;

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
    if (InitConsole()) {
        return 1;
	}

	fs.open("inputrec.dat", ios_base::binary | ios_base::trunc);
	zfs.push(gzip_compressor());
	zfs.push(fs);

    SetWindowsHookExW(WH_MOUSE_LL, LowLevelMouseProc, NULL, 0);

	// N.B. proper usage of the API requires a first call to determine size
	// of data. We allocate a large buffer and skip that. It appears Window's
	// ability to buffer events is very limited despite the existence of API
	// to do just that. HID reports can be up to 4k and they may arrive at
	// 1000Hz.
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
	// Take the timestamp before any processing is done.
	FILETIME ft;
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
		// Previously had access issue. May have been byte alignment.
		int rc = GetRawInputData(
			(HRAWINPUT)lParam, RID_INPUT,
			ri, &cbSz,
			sizeof(RAWINPUTHEADER)
		);
		if (rc < 0) {
			// TODO: Expand buffer, log error.
		}

		return RawInputProc(&ri, 1, sizeof(RAWINPUTHEADER), ft);
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
	UINT	  cbSizeHeader,
	FILETIME  When
) {
	PRAWINPUT ri = *paRawInput;
	switch(ri->header.dwType) {
		case RIM_TYPEMOUSE: {
			// TODO: Shorten timestamp.
			MouseRecord mc;
			// Was 33, 42, on move or 25/27 on scroll.
			//
			// Optional fields and sint32 on X/Y make move records
			// 12 or 14 bytes, most of which is timestamp.
			//
			// No change when handle added?
			//
			// sint32 on ButtonData doesn't matter, it is signed
			// but a short. If mouse is scrolled it takes up 2-4 bytes
			// in all cases with negative motion being 2 bytes larger.
			// Could zero pad to the left?
			mc.set_time((uint64_t)When.dwHighDateTime << 32 |
				(uint64_t)When.dwLowDateTime);
			mc.set_handle((uint64_t)ri->header.hDevice);
			if (ri->data.mouse.usFlags)
				mc.set_flags(ri->data.mouse.usFlags);
			if (ri->data.mouse.usButtonFlags)
				mc.set_buttonflags(ri->data.mouse.usButtonFlags);
			if (ri->data.mouse.usButtonData)
				mc.set_buttondata(ri->data.mouse.usButtonData);
			if (ri->data.mouse.ulRawButtons)
				mc.set_buttons(ri->data.mouse.ulRawButtons);
			if (ri->data.mouse.lLastX)
				mc.set_x(ri->data.mouse.lLastX);
			if (ri->data.mouse.lLastY)
				mc.set_y(ri->data.mouse.lLastY);
			if (ri->data.mouse.ulExtraInformation)
				mc.set_extra(ri->data.mouse.ulExtraInformation);
			//printf("%s\n", mc.ShortDebugString().c_str());
			printf("mouse: %lld %lld\n",
				mc.ByteSizeLong(),
				mc.SpaceUsedLong()
			);
			mc.SerializeToOstream(&zfs);
			break;
		}
		case RIM_TYPEKEYBOARD: {
			KeyboardRecord kc;
			kc.set_time((uint64_t)When.dwHighDateTime << 32 |
				(uint64_t)When.dwLowDateTime);
			kc.set_handle((uint64_t)ri->header.hDevice);
			kc.set_code(ri->data.keyboard.MakeCode);
			if (ri->data.keyboard.Flags)
				kc.set_flags(ri->data.keyboard.Flags);
			if (ri->data.keyboard.Reserved)
				kc.set_reserved(ri->data.keyboard.Reserved);
			kc.set_vkey(ri->data.keyboard.VKey);
			kc.set_message(ri->data.keyboard.Message);
			if (ri->data.keyboard.ExtraInformation)
				kc.set_extra(ri->data.keyboard.ExtraInformation);
			kc.SerializeToOstream(&zfs);
			break;
		}
		case RIM_TYPEHID: {
			// TODO: Find a way to compact these records. Maybe
			// streaming compression? Could also avoid destructuring
			// and restructuring mouse and keyboard events.
			//
			// Some device produce short descriptors comparable to
			// a mouse or keyboard report, like gamepads. But others
			// produce very large reports. E.g. author's touchscreen
			// reports ~230 bytes at ~1000Hz.
			HidRecord hc;
			hc.set_time((uint64_t)When.dwHighDateTime << 32 |
				(uint64_t)When.dwLowDateTime);
			hc.set_handle((uint64_t)ri->header.hDevice);
			hc.set_size(ri->data.hid.dwSizeHid);
			hc.set_count(ri->data.hid.dwCount);
			hc.set_data(ri->data.hid.bRawData,
				ri->data.hid.dwSizeHid * ri->data.hid.dwCount);
			//printf("%s\n", hc.ShortDebugString().c_str());
			printf("hid: %lld %lld\n",
				hc.ByteSizeLong(),
				hc.SpaceUsedLong()
			);
			hc.SerializeToOstream(&zfs);
			break;
		}
	}
	return DefRawInputProc(paRawInput, 1, cbSizeHeader);
    // Documentation implies the above call is pointless.
}