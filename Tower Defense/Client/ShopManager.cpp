#include "ShopManager.h"

ShopManager::ShopManager() : showUpgradePanel(false)
{
	int screenWidth = GetScreenWidth();
	int screenHeight = GetScreenHeight();

	damageButtonRect = { (float)(screenWidth / 2 - 150), (float)(screenHeight / 2 + 20), 200, 40 };
	rangeButtonRect = { (float)(screenWidth / 2 + 75), (float)(screenHeight / 2 + 20), 180, 40 };
	closeButtonRect = { (float)(screenWidth / 2 + 175), (float)(screenHeight / 2 - 75), 30, 40 };
}

ShopManager::~ShopManager()
{
}

void ShopManager::init()
{
	showUpgradePanel = false;
}

void ShopManager::update(float deltaTime)
{
}

void ShopManager::draw(const UpgradeStats& stats, int currency) const
{
	DrawText(TextFormat("Wave: %d", stats.wavesSurvived), 10, 10, 20, BLACK);
	DrawText(TextFormat("Damage: %d | Range: %.0f", stats.totalDamage, stats.totalRange), 10, 40, 20, BLACK);
	DrawText(TextFormat("Gold: %d", currency), GetScreenWidth() - 110, 10, 20, GOLD);

	if (showUpgradePanel)
	{
		DrawRectangle(0, 0, GetScreenWidth(), GetScreenHeight(), Fade(BLACK, 0.5f));

		int screenWidth = GetScreenWidth();
		int screenHeight = GetScreenHeight();

		Rectangle panelRect = { (float)(screenWidth / 2 - 200), (float)(screenHeight / 2 - 150), 500, 350 };

		DrawRectangleRec(panelRect, LIGHTGRAY);
		DrawRectangleLinesEx(panelRect, 3, DARKGRAY);

		DrawText("Upgrade Available!", (int)(panelRect.x + 100), (int)(panelRect.y + 20), 30, BLACK);

		DrawRectangleRec(damageButtonRect, RED);
		DrawRectangleLinesEx(damageButtonRect, 2, BLACK);
		DrawText("Upgrade Damage", (int)(damageButtonRect.x + 10), (int)(damageButtonRect.y + 10), 20, WHITE);

		DrawRectangleRec(rangeButtonRect, GREEN);
		DrawRectangleLinesEx(rangeButtonRect, 2, DARKGREEN);
		DrawText("Upgrade Range", (int)(rangeButtonRect.x + 10), (int)(rangeButtonRect.y + 10), 20, WHITE);

		DrawRectangleRec(closeButtonRect, DARKGRAY);
		DrawText("X", (int)(closeButtonRect.x + 8), (int)(closeButtonRect.y), 20, BLACK);

	}
}

bool ShopManager::isDamageButtonClicked() const
{
	if (!showUpgradePanel) return false;

	Vector2 mousePos = GetMousePosition();

	return CheckCollisionPointRec(mousePos, damageButtonRect) && IsMouseButtonPressed(MOUSE_LEFT_BUTTON);
}

bool ShopManager::isRangeButtonClicked() const
{
	if (!showUpgradePanel) return false;

	Vector2 mousePos = GetMousePosition();

	return CheckCollisionPointRec(mousePos, rangeButtonRect) && IsMouseButtonPressed(MOUSE_LEFT_BUTTON);
}

void ShopManager::closeUpgradePanel()
{
	showUpgradePanel = false;
}


