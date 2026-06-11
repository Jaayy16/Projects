#pragma once

#include "GameStruct.h"

class WaveManager
{
private:
	int currentWave;
	int enemiesSpawned;
	int enemiesToSpawn;
	float spawnTimer;
	float difficulty;

public:
	WaveManager();
	~WaveManager();

	void init();
	
	void update(float deltaTime);

	WaveInfo getCurrentWaveInfo() const;

	bool isWaveComplete() const;

	void nextWave();

	int getCurrentWave() const { return currentWave;  }
	int getEnemiesSpawned() const { return enemiesSpawned; }
	int getEnemiesToSpawn() const { return enemiesToSpawn; }
	bool shouldSpawnEnemy() const;

	void recordSpawns();
};

