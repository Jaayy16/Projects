#include <iostream>
#include "Includes.h"
#include "tileson.hpp"
#include "GameManager.h"

int main()
{
	// Initialize Raylib window
	int screenWidth = 960;
	int screenHeight = 640;

	InitWindow(screenWidth, screenHeight, "Raylib and Tileson Example");
	SetTargetFPS(60);

	GameManager gameManager;
	gameManager.init();

	// Main game loop
	while (!WindowShouldClose())
	{
		//update
		float deltaTime = GetFrameTime();

		gameManager.update(deltaTime);

		BeginDrawing();
		ClearBackground(WHITE);

		gameManager.draw();

		EndDrawing();
	}
	CloseWindow();

	return 0;
}
