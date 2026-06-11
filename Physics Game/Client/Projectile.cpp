#include "Projectile.h"
#include <Box2D/Box2D.h>

Projectile::Projectile(b2World* world, const b2Vec2& pos, ProjectileShapes shape) : shape(shape)
{
	b2BodyDef bodyDef;
	bodyDef.type = b2_dynamicBody;
	bodyDef.position = pos;
	body = world->CreateBody(&bodyDef);

	body->SetUserData((void*)2);
	
	CreateFixt(body, shape);
}

void Projectile::CreateFixt(b2Body* body, ProjectileShapes shape)
{
	b2FixtureDef fixtDef;
	fixtDef.density = 1.0f;
	fixtDef.restitution = 0.5f;

	b2CircleShape circle;
	b2PolygonShape polygon;

	switch (shape)
	{
		case ProjectileShapes::Circle:
		{
			circle.m_radius = 0.3f;
			fixtDef.shape = &circle;
			break;
		}
		case ProjectileShapes::Triangle:
		{
			b2Vec2 vertices[3] = { {0.0f, -0.4f}, {0.35f, 0.3f}, {-0.35f,0.3f} };
			polygon.Set(vertices, 3);
			fixtDef.shape = &polygon;
			break;
		}
		case ProjectileShapes::Square:
		{
			polygon.SetAsBox(0.3f, 0.3f);
			fixtDef.shape = &polygon;
			break;
		}
	}
	body->CreateFixture(&fixtDef);
}

void Projectile::Launch(const b2Vec2& direction, float strength)
{
	body->SetLinearVelocity(b2Vec2_zero);
	body->ApplyLinearImpulseToCenter(strength * direction, true);
}