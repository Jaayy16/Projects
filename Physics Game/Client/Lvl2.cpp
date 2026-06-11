#include "Lvl2.h"
#include <iostream>

void Lvl2::Load(b2World* world)
{
	//Ground
	b2BodyDef groundPlatDef;
	groundPlatDef.type = b2_staticBody;
	groundPlatDef.position.Set(12.0f, 16.0f);

	b2Body* ground = world->CreateBody(&groundPlatDef);
		
	b2PolygonShape groundShape;
	groundShape.SetAsBox(3.0f, 0.5f);
	ground->CreateFixture(&groundShape, 0.0f);

	//Wall 1 (dynamic)

	b2BodyDef wallDef;
	wallDef.type = b2_dynamicBody;
	wallDef.position.Set(10.5f, 15.0f);
	
	b2Body* wallOne = world->CreateBody(&wallDef);

	b2PolygonShape wallShape;
	wallShape.SetAsBox(0.3f, 1.5f);
	b2FixtureDef wallOneFixtDef;
	wallOneFixtDef.shape = &wallShape;
	wallOneFixtDef.density = 0.8f;
	
	wallOne->CreateFixture(&wallOneFixtDef);

	wallOne->SetUserData((void*)1);
	houseParts.push_back(wallOne);

	//Wall 2 (shorter version of wall 1)
	wallDef.position.Set(13.5f, 15.0f);
	b2Body* wallTwo = world->CreateBody(&wallDef);

	wallTwo->CreateFixture(&wallOneFixtDef);
	
	wallTwo->SetUserData((void*)1);
	houseParts.push_back(wallTwo);

	//top part of the structure
	b2BodyDef topDef;
	topDef.type = b2_dynamicBody;
	topDef.position.Set(12.0f, 13.0f);

	b2Body* top = world->CreateBody(&topDef);

	b2PolygonShape topShape;
	b2Vec2 vertices[3] = { {0.0f, -3.0f} , {3.0f, 0.0f}, {-3.0f, 0.0f} };

	topShape.Set(vertices, 3);

	b2FixtureDef topFixt;
	topFixt.shape = &topShape;
	topFixt.density = 0.5f;

	top->CreateFixture(&topFixt);

	top->SetUserData((void*)1);
	houseParts.push_back(top);

}

void Lvl2::Unload(b2World* world)
{
	for (b2Body* body : houseParts)
	{
		world->DestroyBody(body);
	}

	houseParts.clear();
}

void Lvl2::Update(float deltaTime, b2World* world)
{
	int activeCount = 0;
	for (b2Body* body : houseParts)
	{
		if (!body) continue;

		b2Vec2 pos = body->GetPosition();

		if (pos.y < 16.0f)
		{
			activeCount++;
		}

	}

	allShapesBroken = (activeCount == 0);
}

void Lvl2::Draw() const
{
}