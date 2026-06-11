#pragma once
#include "raylib.h"
#include <box2d/box2d.h>
#include <fmt/core.h>
#define RAYGUI_IMPLEMENTATION
#include "raygui.h"


#if defined(_WIN32)           
#define NOGDI             // All GDI defines and routines
#define NOUSER            // All USER defines and routines
#endif
