#ifndef INCLUDED_LimeWrapper
#define INCLUDED_LimeWrapper

#ifndef ENGINE
#define ENGINE_NS external
#define ENGINE ExternalAPI_obj
#endif

#ifndef HXCPP_H
#include <hxcpp.h>
#endif

extern "C" 
{
	//
	// Haxe Forward Declarations
	//
	
    void hxcpp_set_top_of_stack();
    const char* hxRunLibrary();

	//
	// Lime Forward Declarations
	//

	int lime_register_prims();
	int lime_cairo_register_prims();
	int lime_opengl_register_prims();
}

//
// Static API class that provides an interface to the lime engine
//
#ifdef ENGINE_NS
namespace ENGINE_NS {
#endif

class HXCPP_CLASS_ATTRIBUTES ENGINE : public hx::Object
{
	public:

		static void init();

		static void setExternalCallback(::cpp::Function< void  (const char*) > cb);
		static void callFunction(const char* invokeJson);

		static void appCreateWindow();
		static void appCreateWindowFrom(int foreignHandle);

		static void appInit();

		static bool appSingleUpdatePass(int numEvents);
		static void appExec();

		static void appExit(int result);
};

#ifdef ENGINE_NS
}
#endif

#endif /* INCLUDED_LimeWrapper */ 
