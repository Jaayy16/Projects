#pragma once

#include "GameStruct.h"
#include "MapManager.h"
#include "EnemyManager.h"
#include "TowerManager.h"
#include "WaveManager.h"
#include "ShopManager.h"

class GameManager
{
private:
	MapManager mapManager;
	EnemyManager enemyManager;
	TowerManager towerManager;
	WaveManager waveManager;
	ShopManager shopManager;

	GameState gameState;
	UpgradeStats upgradeStats;
	bool waitingForUpgrade;

	GameResources resources;

	TowerShotBoost towerShotBoost;
	std::vector<powerUpDrop> powerUps;

	Notificatons notification;

	void handleWaveSpawning(float deltaTime);
	void handleUpgrades();
	void handleTowerPlace();
	void handlePowerUpPickUp();

public:
	GameManager();
	~GameManager();

	void init();
	void update(float deltaTime);
	void draw() const;

	void addCurrency(int amt);
	bool spendCurrency(int amt);
	int getCurrency() const { return resources.currency; }

	void spawnPowerUp(Vector2 pos);
	void collectPowerUp(int index);

	float getFireRateIncrease() const { return towerShotBoost.IsActive ? towerShotBoost.multiplier : 1.0F; }

	void showNotification(const std::string& msg, float duration = 2.0f, Color color = WHITE);

	GameState getGameState() const { return gameState; }
};


