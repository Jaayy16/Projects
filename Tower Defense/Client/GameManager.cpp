#include "GameManager.h"

GameManager::GameManager() : gameState(GameState::PLAYING), waitingForUpgrade(false)
{
	upgradeStats = { 0, 0, 0 };
	resources.currency = 75;
}

GameManager::~GameManager()
{
}

void GameManager::init()
{
	if (mapManager.loadMap("Map/Map2.tmj"))
	{

		Gamemap* gameMap = mapManager.getGameMap();

		if (!gameMap->waypoints.empty()) {
			Vector2 spawnPos = gameMap->waypoints[0].pos;

			Vector2 exitPos = gameMap->waypoints.back().pos;

			enemyManager.init(&gameMap->waypoints, spawnPos);

			gameMap->exitPortal.pos = exitPos;

		}

		towerManager.init(&enemyManager, gameMap->towerPlacements);
		waveManager.init();
		shopManager.init();

		towerManager.setGameManager(this);
		enemyManager.setGameManager(this);

		upgradeStats.totalDamage = 0;
		upgradeStats.totalRange = 0;
		upgradeStats.wavesSurvived = 0;
	}
	else
	{
		return;
	}

}

void GameManager::update(float deltaTime)
{
	if (gameState != GameState::PLAYING)
	{
		return;
	}

	waveManager.update(deltaTime);
	handleWaveSpawning(deltaTime);

	enemyManager.update(deltaTime);
	towerManager.update(deltaTime);

	shopManager.update(deltaTime);
	handleUpgrades();
	handleTowerPlace();
	handlePowerUpPickUp();

	if (!waitingForUpgrade && enemyManager.getEnemyCount() == 0 && waveManager.isWaveComplete())
	{
		shopManager.showPanel();
		waitingForUpgrade = true;
	}

	if (notification.IsActive)
	{
		notification.timer -= deltaTime;

		if (notification.timer <= 0.0f)
		{
			notification.IsActive = false;
		}
	}

}

void GameManager::handleWaveSpawning(float deltaTime)
{
	if (waveManager.shouldSpawnEnemy())
	{
		WaveInfo waveInfo = waveManager.getCurrentWaveInfo();
		enemyManager.spawnEnemy(waveInfo.enemySpeed, waveInfo.enemyHealth);
		waveManager.recordSpawns();
	}
}

void GameManager::handleUpgrades()
{
	int upgradeCost = 25;

	if (shopManager.isDamageButtonClicked())
	{
		if (spendCurrency(upgradeCost)) {

			towerManager.upgradeDMG(5);
			upgradeStats.totalDamage = (int)towerManager.getGlobalDamage();
			shopManager.closeUpgradePanel();
			waitingForUpgrade = false;

			waveManager.nextWave();
			upgradeStats.wavesSurvived = waveManager.getCurrentWave() - 1;
		}
		else
		{
			std::cout << "Not enough coins for upgrade " << std::endl;
		}
	}
	else if (shopManager.isRangeButtonClicked())
	{
		if (spendCurrency(upgradeCost)) {
			towerManager.upgradeRange(10);
			upgradeStats.totalRange = (int)towerManager.getGlobalRange();
			shopManager.closeUpgradePanel();
			waitingForUpgrade = false;

			waveManager.nextWave();
			upgradeStats.wavesSurvived = waveManager.getCurrentWave() - 1;

		}
		else
		{
			std::cout << "Not enough coins for upgrade " << std::endl;
		}
	}
	else if (shopManager.isUpgradepPanelVisible() && IsMouseButtonPressed(MOUSE_LEFT_BUTTON))
	{
		Vector2 mousePos = GetMousePosition();

		Rectangle closeRect = { (float)(GetScreenWidth() / 2 + 175), (float)(GetScreenHeight() / 2 - 75), 30, 40 };

		if (CheckCollisionPointRec(mousePos, closeRect))
		{
			shopManager.closeUpgradePanel();
			waitingForUpgrade = false;

			waveManager.nextWave();
			upgradeStats.wavesSurvived = waveManager.getCurrentWave() - 1;

		}
	}

}

void GameManager::spawnPowerUp(Vector2 pos)
{
	powerUpDrop powerUp;
	powerUp.pos = pos;
	powerUp.IsActive = true;
	powerUps.push_back(powerUp);
}

void GameManager::collectPowerUp(int index)
{
	if (index < 0 || index >= (int)powerUps.size())
	{
		return;
	}
	powerUps.erase(powerUps.begin() + index);

	if (!towerShotBoost.IsActive)
	{
		towerShotBoost.IsActive = true;
		towerShotBoost.remainingWaves = 2;
		showNotification("Fire Rate Boosted! (2 waves)", 2.0f, MAGENTA);
	}
	else
	{
		towerShotBoost.remainingWaves = std::max(towerShotBoost.remainingWaves, 2);
		showNotification("Extended Fire Rate Boost! (2 waves)", 2.0f, MAGENTA);
	}
}

void GameManager::handleTowerPlace()
{

	int towerCost = 50;

	if (IsMouseButtonPressed(MOUSE_BUTTON_LEFT))
	{

		Vector2 mousePos = GetMousePosition();

		for (size_t i = 0; i < mapManager.getGameMap()->towerPlacements.size(); ++i)
		{
			const TowerPlacement& placement = mapManager.getGameMap()->towerPlacements[i];

			if (!placement.IsOccupied && CheckCollisionPointCircle(mousePos, placement.pos, placement.radius))
			{
				if (spendCurrency(towerCost))
				{

					if (towerManager.placeTower((int)i))
					{
						mapManager.getGameMap()->towerPlacements[i].IsOccupied = true;
					}
				}
				else
				{
					showNotification("Not Enough Coins", 1.5f, GOLD);
				}

				break;
			}
		}
	}
}

void GameManager::handlePowerUpPickUp()
{
	if (IsMouseButtonPressed(MOUSE_LEFT_BUTTON))
	{
		Vector2 mousePos = GetMousePosition();

		for (size_t i = 0; i < powerUps.size(); i++)
		{
			if (CheckCollisionPointCircle(mousePos, powerUps[i].pos, powerUps[i].radius))
			{
				collectPowerUp((int)i);
				break;
			}
		}
	}
}

void GameManager::addCurrency(int amt)
{
	resources.currency += amt;
}

bool GameManager::spendCurrency(int amt)
{
	if (resources.currency >= amt)
	{
		resources.currency -= amt;
		return true;
	}
	return false;
}

void GameManager::showNotification(const std::string& msg, float duration, Color color)
{
	notification.text = msg;
	notification.timer = duration;
	notification.color = color;
	notification.IsActive = true;
}

void GameManager::draw() const
{
	mapManager.draw();
	mapManager.drawObjs();

	enemyManager.draw();
	towerManager.draw();
	shopManager.draw(upgradeStats, resources.currency);

	for (const auto& powerUp : powerUps)
	{
		DrawRectangle((int)powerUp.pos.x - 12, (int)powerUp.pos.y - 12, 24, 24, SKYBLUE);
		DrawRectangleLines((int)powerUp.pos.x - 12, (int)powerUp.pos.y - 12, 24, 24, DARKBLUE);
		DrawText("!", (int)powerUp.pos.x - 6, (int)powerUp.pos.y - 10, 20, BLACK);
	}

	if (notification.IsActive)
	{
		int screenW = GetScreenWidth();
		int textW = MeasureText(notification.text.c_str(), 30);

		int x = screenW / 2 - textW / 2;
		int y = GetScreenHeight() / 2 - 50;

		DrawText(notification.text.c_str(), x, y, 30, notification.color);
	}
}
