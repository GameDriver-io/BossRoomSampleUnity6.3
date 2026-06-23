
#if UNITY_USE_OPENXRSTUBS

#ifndef _MSC_VER
# include <alloca.h>
#else
# include <malloc.h>
#endif

#include <cstring>
#include <string.h>
#include <stdio.h>

#include "il2cpp-class-internals.h"
#include "codegen/il2cpp-codegen.h"
#include "il2cpp-object-internals.h"

//#include "vm/Exception.h"
//#include "vm/InternalCalls.h"

////////////// GOTCHA WARNING /////////////////////
// 
// The IL declarations need to be in the same order as the method declarations.
// This is because the logic picks the headers in order and delivers them in order to the methods.
// Also, be careful using the terms "il2cpp" and "eXtern" in all caps in comments.
// 
///////////////////////////////////////////////////

struct Pose_{};

IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR intptr_t NativeApi_TryInitialize();
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR intptr_t NativeApi_TryUpdateHands();
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR intptr_t NativeApi_GetDetectedHandMeshLayout();

extern "C"
{
	void* openxr_TryInitialize(){return nullptr;}
	void* openxr_TryUpdateHands(){return nullptr;}
	void* openxr_GetDetectedHandMeshLayout(){return nullptr;}
}

#endif // !UNITY_WEBGL || UNITY_WEBGL && !UNITY_2019 && !UNITY_2020
