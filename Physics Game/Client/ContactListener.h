#pragma once
#include <Box2D/Box2D.h>
#include <unordered_map>
#include <raylib.h>

class ContactListener : public b2ContactListener
{
private:
	int totalScore = 0;
	Sound collisionSound;
	friend class Game;

	void ProcessCollision(b2Body* bodA, b2Body* bodB);

public:
	
	void BeginContact(b2Contact* contact) override;
	
	void AddScore(int points) { totalScore += points; }
	int GetScore() const { return totalScore; }
	void ResetScore() { totalScore = 0; }

};

