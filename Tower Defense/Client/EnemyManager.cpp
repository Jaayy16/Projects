#include "EnemyManager.h"
#include "GameManager.h"
#include <algorithm>
#include <cmath>

EnemyManager::EnemyManager() : waypoints(nullptr), nextEnemyId(0), globalEnemySpeed(50.0f), globalEnemyHealth(100.0f)
{
}

EnemyManager::~EnemyManager()
{
	if (enemyTexture.id != 0) UnloadTexture(enemyTexture);
	if (deadEnemyTexture.id != 0) UnloadTexture(deadEnemyTexture);
}

void EnemyManager::init(std::vector<Waypoint>* waypointsPtr, Vector2 spawnPos)
{
	waypoints = waypointsPtr;
	spawnPos = spawnPos;

	this->spawnPos = spawnPos;

	enemyTexture = LoadTexture("Resources/enemy_walk.png");

	walkFrameW = 64;
	walkFrameH = 64;
	walkFrameCount = 13;
	frameDuration = 0.05f;

	deadEnemyTexture = LoadTexture("Resources/enemy_death.png");
	deathFrameW = 64;
	deathFrameH = 64;
	deathFrameCount = 13;

	if (!waypoints->empty())
	{
		std::sort(waypoints->begin(), waypoints->end(), [](const Waypoint& a, const Waypoint& b)
			{
				return a.order < b.order;
			});
	}
}

void EnemyManager::update(float deltaTime)
{
	if (!waypoints || waypoints->empty()) return;

	for (auto& enemy : enemies)
	{
		if (enemy.state == EnemyState::ALIVE) {
			moveEnemy(enemy, deltaTime);

			enemy.frameTimer += deltaTime;
			if (enemy.frameTimer >= frameDuration)
			{
				enemy.frameTimer = 0.0f;
				enemy.currentFrame = (enemy.currentFrame + 1) % walkFrameCount;
			}

			if (hasReachedEnd(enemy)) {
				enemy.state = EnemyState::DEAD;
			}
		}
		else if (enemy.state == EnemyState::DYING)
		{
			enemy.frameTimer += deltaTime;

			if (enemy.frameTimer >= frameDuration)
			{
				enemy.frameTimer = 0.0f;
				enemy.currentFrame++;

				if (enemy.currentFrame >= deathFrameCount)
				{
					enemy.state = EnemyState::DEAD;
				}
			}
		}
	}

	removeDeadEnemies();
}

void EnemyManager::moveEnemy(Enemy& enemy, float deltaTime)
{
	if (enemy.pathIndex >= (int)waypoints->size())
	{
		enemy.state = EnemyState::DEAD;
		return;
	}

	Waypoint current = waypoints->at(enemy.pathIndex);
	bool IsLast = (enemy.pathIndex == (int)waypoints->size() - 1);
	Waypoint next = waypoints->at(enemy.pathIndex + 1);

	Vector2 dir = { next.pos.x - enemy.pos.x, next.pos.y - enemy.pos.y };

	float dist = std::sqrt(dir.x * dir.x + dir.y * dir.y);

	if (dist > 0) {
		dir.x /= dist;
		dir.y /= dist;
	}

	enemy.pos.x += dir.x * enemy.speed * deltaTime;
	enemy.pos.y += dir.y * enemy.speed * deltaTime;

	float distToWaypoint = distance(enemy.pos, next.pos);

	Vector2 toNext = { next.pos.x - enemy.pos.x, next.pos.y - enemy.pos.y };
	float dot = toNext.x * dir.x + toNext.y * dir.y;

	if (distToWaypoint < 10.0f || dot < 0.0f)
	{
		if (IsLast)
		{
			enemy.state = EnemyState::DEAD;
		}
		else
		{
			enemy.pathIndex++;
		}
	}
}

void EnemyManager::spawnEnemy(float speed, float health)
{
	Enemy newEnemy;
	newEnemy.id = nextEnemyId++;
	newEnemy.pos = spawnPos;
	newEnemy.health = health;
	newEnemy.maxHealth = health;
	newEnemy.speed = speed;
	newEnemy.orignalSpeed = speed;
	newEnemy.pathIndex = 0;
	newEnemy.velocity = { 0, 0 };

	newEnemy.visualOffset = { 35, 30 };

	newEnemy.state = EnemyState::ALIVE;

	newEnemy.currentFrame = 0;
	newEnemy.frameTimer = 0.0f;

	newEnemy.IsSpeedBoosted = false;

	enemies.push_back(newEnemy);
}

void EnemyManager::damageEnemy(int enemyId, int damage)
{
	for (auto& enemy : enemies)
	{
		if (enemy.id == enemyId && enemy.state == EnemyState::ALIVE)
		{
			enemy.health -= damage;

			if (enemy.health <= 0)
			{
				if (gameManager) gameManager->addCurrency(10);
				
				if (GetRandomValue(0, 99) < 5 && gameManager)
				{
					gameManager->spawnPowerUp(enemy.pos);
				}
				
				enemy.state = EnemyState::DYING;
				enemy.currentFrame = 0;

				enemy.frameTimer = 0.0f;


			}
			break;
		}
	}
}

std::vector<Enemy*> EnemyManager::getAliveEnemies()
{
	std::vector<Enemy*> alive;

	for (auto& enemy : enemies)
	{
		if (enemy.state == EnemyState::ALIVE)
		{
			alive.push_back(&enemy);
		}
	}
	return alive;
}

bool EnemyManager::hasReachedEnd(const Enemy& enemy) const
{
	if (!waypoints || waypoints->empty())
	{
		return false;
	}

	return enemy.pathIndex >= (int)waypoints->size();
}

void EnemyManager::removeDeadEnemies()
{
	enemies.erase(std::remove_if(enemies.begin(), enemies.end(), [](const Enemy& e) {return e.state == EnemyState::DEAD; }), enemies.end());
}

float EnemyManager::distance(Vector2 a, Vector2 b) const {

	float dx = a.x - b.x;
	float dy = a.y - b.y;

	return std::sqrt(dx * dx + dy * dy);
}

void EnemyManager::boostEnemySpeed(float increase)
{
	std::vector<Enemy*> alive = getAliveEnemies();

	if (alive.empty()) return;

	int index = GetRandomValue(0, (int)alive.size() - 1);

	Enemy* enemy = alive[index];

	if (!enemy->IsSpeedBoosted)
	{
		enemy->orignalSpeed = enemy->speed + 5;
		enemy->speed *= increase;
		enemy->IsSpeedBoosted = true;
	}
}

void EnemyManager::draw() const
{
	for (const auto& enemy : enemies)
	{

		if (enemy.state == EnemyState::DEAD) continue;

		Texture2D currentTexture;
		int frameW;
		int frameH;
		int frameCount;
		bool isAlive = (enemy.state == EnemyState::ALIVE);

		if (isAlive)
		{
			currentTexture = enemyTexture;
			frameW = walkFrameW;
			frameH = walkFrameH;
			frameCount = walkFrameCount;
		}
		else
		{
			currentTexture = deadEnemyTexture;
			frameW = deathFrameW;
			frameH = deathFrameH;
			frameCount = deathFrameCount;
		}

		if (currentTexture.id != 0)
		{
			Rectangle srcRect = { (float)(enemy.currentFrame * frameW), 0.0f, (float)frameW, (float)frameH };

			float dstX = enemy.pos.x + enemy.visualOffset.x - frameW / 2.0f;
			float dstY = enemy.pos.y + enemy.visualOffset.y  - frameH / 2.0f;

			Rectangle dstRect = { dstX, dstY, (float)frameW, (float)frameH };

			Vector2 origin = { (float)frameW / 2, (float)frameH / 2 };

			DrawTexturePro(currentTexture, srcRect, dstRect, origin, 0, WHITE);
		}
		else
		{
			DrawCircleV(enemy.pos, 10, RED);
		}

		if (isAlive) 
		{

			float healthBarW = (float)frameW;
			float healthBarH = 6.0f;

			float barX = enemy.pos.x - healthBarW / 2;
			float barY = enemy.pos.y + enemy.visualOffset.y - frameH / 2 - 25;

			DrawRectangle(barX, barY, healthBarW, healthBarH, BLACK);

			DrawRectangle(barX, barY, healthBarW * enemy.getHealthPercent(), healthBarH, GREEN);

			DrawRectangleLines(barX, barY, healthBarW, healthBarH, DARKGRAY);
		}
	}
}
