#include "il2cpp-config.h"

#ifndef _MSC_VER
# include <alloca.h>
#else
# include <malloc.h>
#endif


#include <cstring>
#include <string.h>
#include <stdio.h>
#include <cmath>
#include <limits>
#include <assert.h>
#include <stdint.h>

#include "il2cpp-class-internals.h"
#include "codegen/il2cpp-codegen.h"
#include "il2cpp-object-internals.h"

#include "vm/Exception.h"
#include "vm/InternalCalls.h"

#define DONTSTRIP __attribute__((used))
#define EXPORT __attribute__((visibility("default")))

extern "C"
{
    EXPORT DONTSTRIP void * il2cpp_resolve_icall_gdio(const char* name)
    {
        if(strlen(name) == 0)
            return 0;
        
        Il2CppMethodPointer method = il2cpp::vm::InternalCalls::Resolve(name);
        if (!method)
        {
            il2cpp::vm::Exception::Raise(il2cpp::vm::Exception::GetMissingMethodException(name));
        }
        return (void*)method;
    }
}

