#pragma once
#include <Box2D/Box2D.h>
#include "Shapes.h"
#include "Utility.h"

class Projectile
{

private:
	b2Body* body = nullptr;
	ProjectileShapes shape;
	void CreateFixt(b2Body* body, ProjectileShapes shape);

public:
	Projectile(b2World* world, const b2Vec2& pos, ProjectileShapes shape);
	b2Body* GetBody() const { return body; }
	void Launch(const b2Vec2& dir, float str);
};