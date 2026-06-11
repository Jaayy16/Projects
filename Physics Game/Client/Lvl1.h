#pragma once
#include "Level.h"
#include <vector>

class Lvl1 : public Level
{
private:
	bool neededScoreGotten = false;
	std::vector<b2Body*> createdBodies;

public:

	void Load(b2World* world) override;
	void Unload(b2World* world) override;
	bool IsLevelBeat() const override { return false; }
	int GetScoreNeeded() const override { return 50; }
	void SetScore(int currentScore) { neededScoreGotten = (currentScore >= 50); }
	void Update(float deltaTime, b2World* world);
	void Draw() const override;
};

