#pragma once
#include "Level.h"
#include <vector>

class Lvl2 : public Level
{
private:
	std::vector<b2Body*> houseParts;
	bool allShapesBroken = false;

public:

	void Load(b2World* world) override;
	void Unload(b2World* world) override;
	void Update(float deltaTime, b2World* world);
	bool IsLevelBeat() const override { return allShapesBroken; }
	int GetScoreNeeded() const override { return 0; }
	void Draw() const override;
};