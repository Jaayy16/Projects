#pragma once

#include <raylib.h>
#include <vector>
#include <string>

enum class GameState
{
	PLAYING,
	WAVE_CLEARED,
	GAME_OVER
};

enum class TowerState {
	IDLE,
	AIMING,
	ATTACKING
};

enum class EnemyState
{
	ALIVE,
	DYING,
	DEAD
};

//additonal Funcionality
struct GameResources
{
	int currency;
};

struct ObjTransform {
	Vector2 pos;
	float rotation = 0.0f;
};

struct Tower {
	int towerId;
	ObjTransform transform;
	float range;
	int damage;
	float fireRate;
	float lastShotTime;
	int targetEnemyId;

	TowerState state;

	Vector2 gunOffset = { 0,0 };

	int currentFrame;
	float frameTimer;
	bool IsFiring;

	Texture2D baseTexture;
	Texture2D gunTexture;
	float gunRotate;
};

struct Enemy {
	int id;
	Vector2 pos;
	Vector2 velocity;
	float health;
	float maxHealth;
	float speed;
	int pathIndex;
	
	Vector2 visualOffset;
	EnemyState state;

	Texture2D enemyTexture;
	int currentFrame;
	float frameTimer;

	//additional functionalities
	bool IsSpeedBoosted = false;
	float orignalSpeed = 0.0f;

	float getHealthPercent() const { return health / maxHealth; }
};

struct Waypoint {
	Vector2 pos;
	int order;
};

struct TowerPlacement {
	Vector2 pos;
	bool IsOccupied;
	int occupiedTowerId;
	float radius;
	int towerId;
};

struct Portal {
	Vector2 pos;
	float radius;
};

struct Gamemap {
	std::vector<Waypoint> waypoints;
	std::vector<TowerPlacement> towerPlacements;
	Portal SpawnPortal;
	Portal exitPortal;
	Texture2D tilesetTexture;

	int width;
	int height;
};

struct WaveInfo {

	int waveNum;
	int enemiesPerWave;
	float spawnRate;
	float enemySpeed;
	float enemyHealth;

};

struct UpgradeStats {
	int totalDamage;
	float totalRange;
	int wavesSurvived;
};

//additonal Funcionality

struct TowerShotBoost
{
	bool IsActive = false;
	int remainingWaves = 0;
	float multiplier = 2.0f;
};

struct Notificatons
{
	std::string text;
	float timer = 0.0f;
	bool IsActive = false;
	Color color;
};

struct powerUpDrop
{
	Vector2 pos;
	bool IsActive = false;
	float radius = 20.0f;
};