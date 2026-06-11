#pragma once

#include "GameStruct.h"
#include <vector>
#include <algorithm>
#include <cmath>

class GameManager;

class EnemyManager
{

private:

	GameManager* gameManager;

	std::vector<Enemy> enemies;
	std::vector<Waypoint>* waypoints;
	Vector2 spawnPos;

	Texture2D enemyTexture;
	int walkFrameW;
	int walkFrameH;
	int walkFrameCount;
	float frameDuration;

	Texture2D deadEnemyTexture;
	int deathFrameW;
	int deathFrameH;
	int deathFrameCount;
	int deathFrameStart;

	float globalEnemySpeed;
	float globalEnemyHealth;
	float nextEnemyId;

	bool hasReachedEnd(const Enemy& enemy) const;
	void moveEnemy(Enemy& enemy, float deltaTime);
	void removeDeadEnemies();

	float distance(Vector2 a, Vector2 b) const;

public:
	EnemyManager();
	~EnemyManager();

	void setGameManager(GameManager* gm) { gameManager = gm; }

	void init(std::vector<Waypoint>* waypointsPtr, Vector2 spawnPos);
	void update(float deltaTime);
	void spawnEnemy(float speed, float health);
	
	void draw() const;

	void damageEnemy(int enemyId, int Damage);

	std::vector<Enemy*> getAliveEnemies();

	std::vector<Enemy>& getEnemiesList() { return enemies; }

	int getEnemyCount() const { return (int)enemies.size(); }
	int getKilledCount() const { return nextEnemyId - (int)enemies.size(); }

	void boostEnemySpeed(float increase = 1.5f);
};
