#include "ContactListener.h"
#include "Utility.h"

void ContactListener::BeginContact(b2Contact* contact)
{
	b2Fixture* fixA = contact->GetFixtureA();
	b2Fixture* fixB = contact->GetFixtureB();
	b2Body* bodA = fixA->GetBody();
	b2Body* bodB = fixB->GetBody();

	uintptr_t partA = (uintptr_t)bodA->GetUserData();
	uintptr_t partB = (uintptr_t)bodB->GetUserData();


	if ((partA == 2 && partB == 1) || (partA == 1 && partB == 2))
	{
		ProcessCollision(bodA, bodB);
	}
}

void ContactListener::ProcessCollision(b2Body* bodyA, b2Body* bodyB)
{
	totalScore += 10;
	PlaySound(collisionSound);
}