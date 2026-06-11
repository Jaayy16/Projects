#pragma once
#include <raylib.h>
#include <Box2D/Box2D.h>

const float pixelPerM = 30.0f;
const b2Vec2 GRAVITY(0.0f, 9.8f);
const float Time_Step = 1.0f / 60.0f;

inline b2Vec2 ScreenToWorld(Vector2 display)
{
	return b2Vec2(display.x / pixelPerM, display.y / pixelPerM);
}

inline Vector2 WorldToScreen(b2Vec2 world)
{
	return Vector2{ world.x * pixelPerM, world.y * pixelPerM };
}