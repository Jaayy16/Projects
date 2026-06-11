#include "TowerManager.h"
#include "EnemyManager.h"
#include "GameManager.h"
#include <cmath>
#include <algorithm>
#include <iterator>
#include <iostream>



TowerManager::TowerManager() : towerPlacements(nullptr), enemyManager(nullptr), nextTowerId(0), globalDamage(20), globalRange(150.0f)
{
}

TowerManager::~TowerManager()
{
	if (towerBaseTexture.id != 0) UnloadTexture(towerBaseTexture);
	if (gunSheet.id != 0) UnloadTexture(gunSheet);
}

void TowerManager::init(EnemyManager* enemyMgr, const std::vector<TowerPlacement>& placements)
{
	enemyManager = enemyMgr;
	towerPlacements = const_cast<std::vector<TowerPlacement>*> (&placements);

	towerBaseTexture = LoadTexture("Resources/tower_base.png");
	gunSheet = LoadTexture("Resources/tower_gun_sheet.png");

	frameWidth = 96;
	frameHeight = 96;
	frameCount = 6;
	frameDuration = 0.05f;
}

void TowerManager::update(float deltaTime)
{
	if (!enemyManager) return;

	float currentTime = (float)GetTime();



	for (auto& tower : towers)
	{

		if (tower.IsFiring)
		{
			tower.frameTimer += deltaTime;

			if (tower.frameTimer >= frameDuration)
			{
				tower.frameTimer = 0.0f;
				tower.currentFrame++;

				if (tower.currentFrame >= frameCount)
				{
					tower.IsFiring = false;
					tower.currentFrame = 0;
					tower.state = TowerState::AIMING;
				}
			}
		}

		Enemy* targetEnemy = findClosestEnemy(tower);

		if (targetEnemy)
		{
			tower.state = TowerState::AIMING;

			Vector2 dir = { targetEnemy->pos.x - tower.transform.pos.x, targetEnemy->pos.y - tower.transform.pos.y };

			tower.gunRotate = atan2f(dir.y, dir.x) * RAD2DEG;

			tower.gunRotate += 90;

			float increase = gameManager ? gameManager->getFireRateIncrease() : 1.0f;
			float effectTowerShoot = tower.fireRate * increase;
			float timeSinceLastShot = currentTime - tower.lastShotTime;

			if (timeSinceLastShot >= (1.0f / effectTowerShoot))
			{
				tower.state = TowerState::ATTACKING;
				enemyManager->damageEnemy(targetEnemy->id, (int)tower.damage);
				tower.lastShotTime = currentTime;

				tower.IsFiring = true;
				tower.currentFrame = 0;
				tower.frameTimer = 0.0f;
				tower.state = TowerState::ATTACKING;
			}
		}
		else
		{
			if (!tower.IsFiring)
			{
				tower.state = TowerState::IDLE;
			}
		}
	}
}

Enemy* TowerManager::findClosestEnemy(const Tower& tower) const
{
	if (!enemyManager) return nullptr;


	auto alive = enemyManager->getAliveEnemies();
	Enemy* closestEnemy = nullptr;
	float closestDist = tower.range;

	for (auto* enemy : alive)
	{
		float dist = distBetween(tower.transform.pos, enemy->pos);

		if (dist < closestDist) {
			closestDist = dist;
			closestEnemy = enemy;
		}
	}

	return closestEnemy;

}

float TowerManager::distBetween(Vector2 a, Vector2 b) const
{
	float dx = a.x - b.x;
	float dy = a.y - b.y;

	return sqrtf(dx * dx + dy * dy);
}

bool TowerManager::placeTower(int index)
{
	if (!towerPlacements)
	{
		std::cout << "towerPlacements is null!" << std::endl;
		return false;
	}

	if (index < 0 || index >= (int)towerPlacements->size())
	{
		std::cout << "Index out of range" << std::endl;
		return false;
	}

	TowerPlacement& placement = towerPlacements->at(index);

	if (placement.IsOccupied)
	{
		return false;
	}

	Tower newTower;
	newTower.towerId = nextTowerId++;
	newTower.transform.pos = placement.pos;
	newTower.range = globalRange;
	newTower.damage = globalDamage;
	newTower.fireRate = 1.0f;
	newTower.lastShotTime = (float)GetTime();
	newTower.targetEnemyId = -1;
	newTower.state = TowerState::IDLE;
	newTower.gunRotate = 0.0f;

	newTower.gunOffset = { 47.5 , -10 };

	newTower.baseTexture = towerBaseTexture;
	newTower.gunTexture = gunSheet;

	newTower.currentFrame = 0;
	newTower.frameTimer = 0.0F;
	newTower.IsFiring = false;

	towers.push_back(newTower);

	placement.IsOccupied = true;
	placement.occupiedTowerId = newTower.towerId;

	return true;


}

void TowerManager::upgradeDMG(int damageIncrease) {
	globalDamage += damageIncrease;
	for (auto& tower : towers) {
		tower.damage = globalDamage;
	}
}

void TowerManager::upgradeRange(float rangeIncrease) {
	globalRange += rangeIncrease;
	for (auto& tower : towers) {
		tower.range = globalRange;
	}
}

void TowerManager::draw() const
{
	for (const auto& tower : towers)
	{
		if (tower.baseTexture.id != 0)
		{
			DrawTexture(tower.baseTexture, (int)(tower.transform.pos.x - tower.baseTexture.width / 2), (int)(tower.transform.pos.y - tower.baseTexture.height / 1.5), WHITE);
		}
		else
		{
			DrawCircle((int)tower.transform.pos.x, (int)tower.transform.pos.y, 12.0f, BLUE);
		}

		if (gunSheet.id != 0)
		{
			int frameToDraw = tower.IsFiring ? tower.currentFrame : 0;

			Rectangle srcRect = { (float)(frameToDraw * frameWidth), 0.0f, (float)frameWidth, (float)frameHeight };

			float dstX = tower.transform.pos.x + tower.gunOffset.x - frameWidth / 2.0f;
			float dstY = tower.transform.pos.y - tower.gunOffset.y - frameHeight / 2.0f;

			Rectangle dstRect = { dstX, dstY, (float)frameWidth, (float)frameHeight };

			Vector2 origin = { (float)frameWidth / 2, (float)frameHeight / 2 };

			DrawTexturePro(gunSheet, srcRect, dstRect, origin, tower.gunRotate, WHITE);
		}
		else
		{
			float gunLength = 15.0f;

			Vector2 gunEnd = {
				tower.transform.pos.x + gunLength * cos(tower.gunRotate * DEG2RAD),
				tower.transform.pos.y + gunLength * sin(tower.gunRotate * DEG2RAD)
			};

			DrawLineEx(tower.transform.pos, gunEnd, 4.0f, DARKBLUE);
		}

		//Range Circle
		DrawCircleLines((int)tower.transform.pos.x, (int)tower.transform.pos.y, (int)tower.range, Fade(BLUE, 0.3f));
	}
}

