
#if UNITY_USE_OVRSTUBS

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

struct Vector3f_{};
struct Quatf_{};
struct Posef_{};
struct PoseStatef_{};
struct ControllerState4_{};
struct ControllerState5_{};
struct ControllerState6_{};
struct HandStateInternal_{};
struct Skeleton2Internal_{};
struct Skeleton3Internal_{};


IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR intptr_t OVRP_1_1_0_ovrp_GetInitialized();
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR intptr_t OVRP_1_1_0_ovrp_GetUserPresent();
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR intptr_t OVRP_1_1_0_ovrp_GetNodePresent();
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR intptr_t OVRP_1_1_0_ovrp_GetUserEyeHeight();
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR intptr_t OVRP_1_9_0_ovrp_GetSystemHeadsetType();
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR intptr_t OVRP_1_9_0_ovrp_GetConnectedControllers();
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR intptr_t OVRP_1_9_0_ovrp_GetActiveController();
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR intptr_t OVRP_1_12_0_ovrp_GetNodePoseState();
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR intptr_t OVRP_1_18_0_ovrp_GetAppHasInputFocus();
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR intptr_t OVRP_1_29_0_ovrp_GetHeadPoseModifier();
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR intptr_t OVRP_1_38_0_ovrp_GetNodePositionValid();
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR intptr_t OVRP_1_44_0_ovrp_GetHandTrackingEnabled();
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR intptr_t OVRP_1_44_0_ovrp_GetHandState();
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR intptr_t OVRP_1_44_0_ovrp_GetMesh();
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR intptr_t OVRP_1_55_0_ovrp_GetSkeleton2();
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR intptr_t OVRP_1_92_0_ovrp_GetSkeleton3();
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR intptr_t OVRP_1_16_0_ovrp_GetControllerState4();
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR intptr_t OVRP_1_78_0_ovrp_GetControllerState5();
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR intptr_t OVRP_1_83_0_ovrp_GetControllerState6();
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR intptr_t OVRP_1_86_0_ovrp_GetControllerIsInHand();

extern "C"
{
	void* ovrp_GetInitialized(){return nullptr;}
	void* ovrp_GetUserPresent(){return nullptr;}
	void* ovrp_GetNodePresent(){return nullptr;}
	void* ovrp_GetUserEyeHeight(){return nullptr;}
	void* ovrp_GetSystemHeadsetType(){return nullptr;}
	void* ovrp_GetConnectedControllers(){return nullptr;}
	void* ovrp_GetActiveController(){return nullptr;}
	void* ovrp_GetNodePoseState(){return nullptr;}
	void* ovrp_GetAppHasInputFocus(){return nullptr;}
	void* ovrp_GetHeadPoseModifier(){return nullptr;}
	void* ovrp_GetNodePositionValid(){return nullptr;}
	void* ovrp_GetHandTrackingEnabled(){return nullptr;}
	void* ovrp_GetHandState(){return nullptr;}
	void* ovrp_GetMesh(){return nullptr;}
	void* ovrp_GetSkeleton2(){return nullptr;}
	void* ovrp_GetSkeleton3(){return nullptr;}
	void* ovrp_GetControllerState4(){return nullptr;}
	void* ovrp_GetControllerState5(){return nullptr;}
	void* ovrp_GetControllerState6(){return nullptr;}
	void* ovrp_GetControllerIsInHand(){return nullptr;}
}

#endif // !UNITY_WEBGL || UNITY_WEBGL && !UNITY_2019 && !UNITY_2020
