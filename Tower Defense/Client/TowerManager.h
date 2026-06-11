#pragma once

#include "GameStruct.h"
#include <vector>

class EnemyManager;

class GameManager;

class TowerManager
{
private:

	GameManager* gameManager; 

	std::vector<Tower> towers;
	std::vector<TowerPlacement>* towerPlacements;
	EnemyManager* enemyManager;

	Texture2D towerBaseTexture;

	int nextTowerId;
	float globalRange;
	int globalDamage;

	Texture2D gunSheet;
	int frameWidth;
	int frameHeight;
	int frameCount;
	float frameDuration;

	Enemy* findClosestEnemy(const Tower& tower) const;
	float distBetween(Vector2 a, Vector2 b) const;

public:
	TowerManager();
	~TowerManager();

	void setGameManager(GameManager* gm) { gameManager = gm; }

	void init(EnemyManager* enemyManager, const std::vector<TowerPlacement>& placements);
	void update(float deltaTime);
	void draw() const;

	bool placeTower(int index);
	void upgradeDMG(int damageIncrease);
	void upgradeRange(float rangeIncrease);

	std::vector<Tower>& getTowers() { return towers; }
	int getTowerCount() const { return (int)(towers.size()); }
	float getGlobalRange() const { return globalRange; }
	float getGlobalDamage() const { return globalDamage; }
};
