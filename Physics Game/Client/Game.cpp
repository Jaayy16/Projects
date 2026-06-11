#include <algorithm>
#include <iostream>
#include "Game.h"
#include "Utility.h"
#include "Lvl1.h"
#include "Lvl2.h"

Game::Game()
{
	InitWindow(800, 600, "Drag to Shoot");
	SetTargetFPS(60);
	InitAudioDevice();

	launchSound = LoadSound("Sounds/launchSound.mp3");
	collisionSound = LoadSound("Sounds/collisionSound.mp3");
	contactListener.collisionSound = collisionSound;

	World = new b2World(GRAVITY);
	World->SetContactListener(&contactListener);

	b2BodyDef groundDef;
	groundDef.position.Set(0, 20.0f);
	b2Body* ground = World->CreateBody(&groundDef);

	b2PolygonShape groundBox;
	groundBox.SetAsBox(40.0f, 1.0f);
	ground->CreateFixture(&groundBox, 0.0f);

	currentLvl = std::make_unique<Lvl1>();
	currentLvl->Load(World);

}

Game::~Game()
{
	delete World;
	UnloadSound(launchSound);
	UnloadSound(collisionSound);
	CloseAudioDevice();
	CloseWindow();
}

void Game::Run()
{
	while (!WindowShouldClose()) {
		UpdateInput();

		World->Step(Time_Step, 8, 3);

		CleanUpProjectiles();

		currentLvl->Update(GetFrameTime(), World);
		
		if (state == lvl_1) 
		{
			UpdateLevelTransition();
		}
		else if (state == lvl_2 && currentLvl->IsLevelBeat())
		{
			levelComplete = true;
		}

		BeginDrawing();
		ClearBackground(WHITE);

		Draw();

		DrawUI();
		DrawTrajLine();
		currentLvl->Draw();

		if (state == lvl_2 && levelComplete)
		{
			DrawText("You beat the Phyiscs Game (press ESC to close)", 250, 300, 20, GOLD);

			if (IsKeyPressed(KEY_ESCAPE))
			{
				CloseWindow();
			}

		}

		EndDrawing();
	}
}

void Game::UpdateInput()
{
	if (IsMouseButtonPressed(MOUSE_BUTTON_LEFT))
	{
		dragStart = GetMousePosition();
		IsDragging = true;
	}

	if (IsMouseButtonReleased(MOUSE_BUTTON_LEFT))
	{
		Vector2 release = GetMousePosition();
		Vector2 drag = { release.x - dragStart.x, release.y - dragStart.y };
		b2Vec2 launchDir(-drag.x, -drag.y);

		float len = sqrtf(drag.x * drag.x + drag.y * drag.y);
		float str = len * 0.2f;

		if (len > 0.0f)
		{
			launchDir.x /= len;
			launchDir.y /= len;

			b2Vec2 spawnPos = ScreenToWorld(dragStart);
			SpawnProjectile(launchDir, str, spawnPos);
		}
		IsDragging = false;

	}

	if (IsKeyPressed(KEY_ONE))
	{
		currentShape = ProjectileShapes::Circle;
	}
	else if (IsKeyPressed(KEY_TWO))
	{
		currentShape = ProjectileShapes::Triangle;
	}
	else if (IsKeyPressed(KEY_THREE))
	{
		currentShape = ProjectileShapes::Square;
	}
}

void Game::SpawnProjectile(const b2Vec2& direction, float strength, const b2Vec2& spawnPos)
{
	projectiles.push_back(std::make_unique<Projectile>(World, spawnPos, currentShape));
	
	projectiles.back()->Launch(direction, strength);

	PlaySound(launchSound);
}

void Game::DrawUI()
{
	DrawText(TextFormat("Score: %d", contactListener.GetScore()), 10, 10, 20, BLACK);
	DrawText("Shape: [1] Circle, [2] Triangle, [3] Square", 10, 30, 20, BLACK);
	DrawText(TextFormat("Target Hits : %d", contactListener.GetScore() / 10), 10, 50, 20, BLACK);

	if (state == lvl_1)
	{
		DrawText("Level 1: Hit the target 5 times", 10, 70, 20, GREEN);
	}
	else
	{
		DrawText("Level 2: Knock down the house", 10, 70, 20, GREEN);
	}
}

void Game::DrawTrajLine()
{
	if (IsDragging)
	{
		Vector2 cur = GetMousePosition();

		DrawLineEx(dragStart, cur, 3, GRAY);
	}
}

void Game::UpdateLevelTransition()
{

	if (state == lvl_1 && contactListener.GetScore() > currentLvl->GetScoreNeeded())
	{
		currentLvl->Unload(World);

		for (auto& proj : projectiles)
		{
			if (proj->GetBody())
			{
				World->DestroyBody(proj->GetBody());
			}
		}

		projectiles.clear();

		currentLvl = std::make_unique<Lvl2>();
		currentLvl->Load(World);
		state = lvl_2;
		contactListener.ResetScore();
	}
}

void Game::CleanUpProjectiles()
{
	projectiles.erase(std::remove_if(projectiles.begin(), projectiles.end(),
		[this](const std::unique_ptr<Projectile>& proj)
		{
			b2Body* body = proj->GetBody();

			if (!body) return true;

			b2Vec2 pos = body->GetPosition();

			if (pos.x < -5.0f || pos.x > 45.0f || pos.y > 30.0f)
			{
				World->DestroyBody(body);
				return true;
			}
			return false;
		}), projectiles.end());
}

void Game::ClearDynamicBodies()
{
	for (b2Body* body = World->GetBodyList(); body;)
	{
		b2Body* next = body->GetNext();

		if (body->GetType() == b2_dynamicBody && body != nullptr)
		{
			World->DestroyBody(body);
		}

		body = next;
	}

	projectiles.clear();
}

void Game::Draw()
{
	for (b2Body* body = World->GetBodyList(); body; body = body->GetNext())
	{
		b2Vec2 pos = body->GetPosition();
		float angle = body->GetAngle();
		Vector2 displayPos = WorldToScreen(pos);

		for(b2Fixture* fixt = body->GetFixtureList(); fixt; fixt = fixt->GetNext())
		{
			b2Shape* shape = fixt->GetShape();

			if (shape->GetType() == b2Shape::e_circle)
			{
				b2CircleShape* circle = (b2CircleShape*)shape;
				float rad = circle->m_radius * pixelPerM;
				DrawCircleV(displayPos, rad, RED);
			}
			else if (shape->GetType() == b2Shape::e_polygon)
			{
				b2PolygonShape* poly = (b2PolygonShape*)shape;
				int count = poly->m_count;

				if (count < 3) continue;

				Vector2 vertices[8];

				for (int i = 0; i < count; i++)
				{

					b2Vec2 vert = poly->m_vertices[i];

					float x = vert.x * cosf(angle) - vert.y * sinf(angle);

					float y = vert.x * sinf(angle) + vert.y * cosf(angle);

					vertices[i] = { (pos.x + x) * pixelPerM, (pos.y + y) * pixelPerM };

				}

				for (int i = 1; i < count - 1; i++)
				{
					DrawTriangleLines(vertices[0], vertices[i], vertices[i + 1], RED);
				}

			}
		}
	}
}