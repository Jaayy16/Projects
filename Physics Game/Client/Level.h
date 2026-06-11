#pragma once
#include <Box2D/Box2D.h>

class Level
{
public: 
	virtual ~Level() = default;

	virtual void Load(b2World* world) = 0;
	virtual void Unload(b2World* world) = 0;
	virtual void Update(float deltaTime, b2World* world) = 0;
	virtual bool IsLevelBeat() const = 0;
	virtual int GetScoreNeeded() const = 0;
	virtual void Draw() const = 0;
};


