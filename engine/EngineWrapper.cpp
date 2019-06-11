#include <iostream>

// this is an environment-specific header file
#include <HxcppConfig.h>

#include <LimeWrapper.h>
#include <EngineWrapper.h>

using namespace std;

#ifdef ENGINE_NS
using namespace ENGINE_NS;
#endif

//
// Public Function Exports
//

extern "C" 
{
    #ifdef HX_WINDOWS
	__declspec(dllexport) 
	#endif
	int init_haxe()
	{
        hxcpp_set_top_of_stack();
        
		lime_register_prims();

        const char* err = NULL;
		err = hxRunLibrary();

        if (err) {
			cerr << "Error " << err << endl;
            return -1;
        }
		
        return 0;
    } // init_haxe

	#ifdef HX_WINDOWS
	__declspec(dllexport)
	bool get_window_class(wchar_t* className, size_t size)
	{
		bool ret;
		
		if (g_classRegistered) {
			ret = true;
		} else {
			ret = RegisterLimeHostW();
		}
		lstrcpynW(className, CLSNAMEW, size);

		return ret;
	}
	#endif

	#ifdef HX_WINDOWS
	__declspec(dllexport)
	#endif
	void init_window()
	{
		ENGINE::appCreateWindow();
		ENGINE::init();
	}

	#ifdef HX_WINDOWS
	__declspec(dllexport)
	void init_local_window(HWND hWnd)
	{
		std::cout << "create window from " << hWnd << std::endl;
		int handle = reinterpret_cast<int>(hWnd);
		ENGINE::appCreateWindowFrom(handle);
		ENGINE::init();
	}
	#endif
	
	#ifdef HX_WINDOWS
    __declspec(dllexport) 
	#endif
	void start_loop()
	{
		ENGINE::appExec();
    }
	
	#ifdef HX_WINDOWS
    __declspec(dllexport) 
	#endif
	bool update(int numEvents)
	{	
		return ENGINE::appSingleUpdatePass(numEvents);
    }

	#ifdef HX_WINDOWS
	__declspec(dllexport)
	#endif
	void set_external_com_listener(ListenerCallback listener)
	{
		ENGINE::setExternalCallback(listener);
	}

	#ifdef HX_WINDOWS
	__declspec(dllexport)
	#endif
	void call_external(const char* jsonCall)
	{
		ENGINE::callFunction(jsonCall);
	}

	#ifdef HX_WINDOWS
    __declspec(dllexport) 
	#endif
	void exit_haxe(int result)
	{
		ENGINE::appExit(result);
    }

} // extern "C"

//
// Private Window Class Management Helper Functions
//

#ifdef HX_WINDOWS
LRESULT CALLBACK WndProcW(HWND hWnd, UINT message, WPARAM wParam, LPARAM lParam)
{
	return ::DefWindowProc(hWnd, message, wParam, lParam);
}

bool RegisterLimeHostW()
{
	if (g_classRegistered) {
		return false;
	}

	WNDCLASSEXW clsInfo;
	clsInfo.cbSize = sizeof WNDCLASSEXW;

	clsInfo.lpfnWndProc = &WndProcW;
	clsInfo.cbClsExtra = 0;
	clsInfo.cbWndExtra = 0;
	clsInfo.hInstance = g_hInstance;
	clsInfo.hIcon = NULL;
	clsInfo.hCursor = NULL;
	clsInfo.hbrBackground = NULL;
	clsInfo.lpszMenuName = NULL;
	clsInfo.lpszClassName = CLSNAMEW;
	clsInfo.hIconSm = NULL;
	clsInfo.style = CS_GLOBALCLASS | CS_OWNDC | CS_DBLCLKS | CS_HREDRAW | CS_VREDRAW;

	if (::RegisterClassExW(&clsInfo)) {
		g_classRegistered = true;
	}

	return g_classRegistered;
}

//
// Library Entry Point
//

BOOL WINAPI DllMain(
    HINSTANCE hinstDLL,  // handle to DLL module
    DWORD fdwReason,     // reason for calling function
    LPVOID lpReserved )  // reserved
{
	switch( fdwReason ) {
		case DLL_PROCESS_ATTACH:
			g_hInstance = hinstDLL;
			break;
		
		case DLL_THREAD_ATTACH:
			break;
		
		case DLL_THREAD_DETACH:
			break;
		
		case DLL_PROCESS_DETACH:
			if (g_classRegistered) {
				::UnregisterClassW(CLSNAMEW, g_hInstance);
				g_classRegistered = false;
			}
			break;
	}
	return TRUE;
}
#endif