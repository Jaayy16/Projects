#pragma once
#include <raylib.h>
#include <box2d/box2d.h>
#include <memory>
#include "ContactListener.h"
#include "Projectile.h"
#include "Shapes.h"
#include "Level.h"

class Game
{
private:
	
	std::unique_ptr<Level> currentLvl;

	enum State{lvl_1, lvl_2};

	State state = lvl_1;
	int lvl1WinReq = 5;

	b2World* World = nullptr;
	
	ContactListener contactListener;
	Sound launchSound;
	Sound collisionSound;

	ProjectileShapes currentShape = ProjectileShapes::Circle;
	Vector2 projectileSpawnMenu = { 200, 450 };
	std::vector<std::unique_ptr<Projectile>> projectiles;


	Vector2 dragStart;
	bool IsDragging = false;

	bool levelComplete = false;

	void UpdateInput();
	void UpdateLevelTransition();
	void DrawUI();
	void DrawTrajLine();
	void SpawnProjectile(const b2Vec2& direction, float strength, const b2Vec2& spawnPos);
	void CleanUpProjectiles();
	void ClearDynamicBodies();


public:
	Game();
	~Game();
	void Run();
	void Draw();
	
};

