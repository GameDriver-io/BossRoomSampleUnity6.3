#include <stddef.h> // for NULL
#include <stdint.h> // for int32_t

#if defined(_MSC_VER)
#   define EXPORT __declspec(dllexport)
#   define PRESERVE
#elif defined(__GNUC__) || defined(__clang__)
#   define EXPORT __attribute__((visibility("default")))
#   define PRESERVE __attribute__((used))
#else
#   error "Unsupported compiler"
#endif

#if defined(__cplusplus)
extern "C" {
#endif

EXPORT PRESERVE int32_t WelsCreateSVCEncoder(void **encoder) {
    *encoder = NULL;
    return -1;
}
EXPORT PRESERVE void WelsDestroySVCEncoder(void *encoder) { }

#if defined(__cplusplus)
}
#endif
