#pragma once

#include "GameStruct.h"

class ShopManager
{

private:
	bool showUpgradePanel;

	Rectangle damageButtonRect;
	Rectangle rangeButtonRect;
	Rectangle closeButtonRect;

public:

	ShopManager();
	~ShopManager();

	void init();
	void update(float deltaTime);
	void draw(const UpgradeStats& stats, int currency) const;

	bool isDamageButtonClicked() const;
	bool isRangeButtonClicked() const;
	void closeUpgradePanel();

	void showPanel() { showUpgradePanel = true; }
	bool isUpgradepPanelVisible() const { return showUpgradePanel; }

};

