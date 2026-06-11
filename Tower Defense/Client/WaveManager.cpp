#include "WaveManager.h"

WaveManager::WaveManager() : currentWave(0), enemiesSpawned(0), enemiesToSpawn(0), spawnTimer(0.0f), difficulty(1.0f)
{
}

WaveManager::~WaveManager()
{
}

void WaveManager::init() {
	currentWave = 1;
	enemiesSpawned = 0;
	enemiesToSpawn = getCurrentWaveInfo().enemiesPerWave;
	difficulty = 1.0f;
}

void WaveManager::update(float deltaTime) {
	if (!isWaveComplete()) {
		spawnTimer += deltaTime;
	}
}

WaveInfo WaveManager::getCurrentWaveInfo() const {

	WaveInfo info;
	info.waveNum = currentWave;
	info.enemiesPerWave = 5 + (currentWave - 1) * 2;
	info.spawnRate = 1.0f + (currentWave - 1) * 0.2f;

	float diffMultiplier = 1.0f + ((currentWave - 1) / 3) * 0.3f;
	
	info.enemySpeed = 50.0f * diffMultiplier;
	info.enemyHealth = 40.0f * diffMultiplier;

	return info;
}

bool WaveManager::isWaveComplete() const {
	return enemiesSpawned >= enemiesToSpawn;
}

bool WaveManager::shouldSpawnEnemy() const {
	WaveInfo info = getCurrentWaveInfo();
	float timeBetweenSpawns = 1.0f / info.spawnRate;
	bool ready = spawnTimer >= timeBetweenSpawns && !isWaveComplete();
	return ready;
}

void WaveManager::nextWave() {
	currentWave++;
	enemiesSpawned = 0;
	spawnTimer = 0.0f;
	enemiesToSpawn = getCurrentWaveInfo().enemiesPerWave;
}

void WaveManager::recordSpawns()
{
	if (enemiesSpawned < enemiesToSpawn)
	{
		enemiesSpawned++;
		spawnTimer = 0.0f;
	}
}