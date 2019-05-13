#ifndef INCLUDED_EngineWrapper
#define INCLUDED_EngineWrapper

#ifdef HX_WINDOWS
#include <windows.h>
#endif

//
// Library globals
//

const wchar_t* CLSNAMEW = L"*LimeHost*";
bool g_classRegistered = false;

#ifdef HX_WINDOWS
HINSTANCE g_hInstance;

//
// Forward Declarations for DLL Exports
//

typedef void (*ListenerCallback)(const char* jsonCall);

extern "C" 
{
 	#ifdef HX_WINDOWS
    __declspec(dllexport) 
	#endif
    int init_haxe();

  	#ifdef HX_WINDOWS
    __declspec(dllexport) 
    bool get_window_class(wchar_t* className, size_t size);
	#endif

  	#ifdef HX_WINDOWS
    __declspec(dllexport) 
	#endif   
    void init_window();

 	#ifdef HX_WINDOWS
    __declspec(dllexport) 
	#endif
    void init_local_window(HWND hWnd);

 	#ifdef HX_WINDOWS
    __declspec(dllexport) 
	#endif
    void start_loop();

 	#ifdef HX_WINDOWS
    __declspec(dllexport) 
	#endif
    bool update(int numEvents);
    
 	#ifdef HX_WINDOWS
    __declspec(dllexport) 
	#endif
    void set_external_com_listener(ListenerCallback listener);

 	#ifdef HX_WINDOWS
    __declspec(dllexport) 
	#endif
    void call_external(const char* jsonCall);
    
 	#ifdef HX_WINDOWS
    __declspec(dllexport) 
	#endif
    void exit_haxe(int result);
}

//
// Private Forward Declarations
//

LRESULT CALLBACK WndProcW(HWND hWnd, UINT message, WPARAM wParam, LPARAM lParam);
bool RegisterLimeHostW();

#endif // HX_WINDOWS

#endif /* INCLUDED_EngineWrapper */ 
