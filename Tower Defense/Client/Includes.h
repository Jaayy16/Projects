#pragma once
#include "raylib.h"
#include <box2d/box2d.h>
#include <fmt/core.h>

#include <vector>
#include <memory>
#include <algorithm>
#include <map>
#include <tuple>

#define RAYGUI_IMPLEMENTATION
#include "raygui.h"
#define RAYMATH_IMPLEMENTATION
#include "raymath.h"

#if defined(_WIN32)           
#define NOGDI             // All GDI defines and routines
#define NOUSER            // All USER defines and routines
#endif
