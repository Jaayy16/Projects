#include "Lvl1.h"
#include "Utility.h"

void Lvl1::Load(b2World* world)
{
	b2BodyDef targetDef;
	targetDef.type = b2_staticBody;
	targetDef.position.Set(15.0f, 10.0f);
	b2Body* target = world->CreateBody(&targetDef);
	
	target->SetUserData((void*)1);

	createdBodies.push_back(target);


	b2CircleShape circle;
	circle.m_radius = 0.8f;
	b2FixtureDef fixt;

	fixt.shape = &circle;
	fixt.density = 1.0f;
	fixt.restitution = 0.5f;
	target->CreateFixture(&fixt);
}

void Lvl1::Unload(b2World* world)
{
	for (b2Body* body : createdBodies)
	{
		world->DestroyBody(body);
	}

	createdBodies.clear();
}

void Lvl1::Update(float deltaTime, b2World* world)
{
}

void Lvl1::Draw() const
{ 
}