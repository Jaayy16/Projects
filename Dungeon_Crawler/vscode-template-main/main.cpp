#include "raylib.h"
#include "iostream"
#include "string"
#include "cstdlib"
#include "vector"
#include "cstring"

using namespace std;

float gameTimer = 0.0f;
float timerLeft = 5.0f;
float spawnTimer = 0.0f;
float spawnCooldown = 3.0f;
float minCooldown = 0.25f;

float padding = 20.0f;

class Game;
class Player;
class Enemy;
class Weapon;
class PickUp;
class Golumn;
class Vampire;
class Skeleton;
class Sword;
class Axe;
class Hammer;

enum States
{
    ACTION,
    PAUSED,
    WIN,
    LOSE
};
States currentState = ACTION;

enum Facing
{
    UP,
    DOWN,
    LEFT,
    RIGHT
};

// add sprites to the game
Texture2D Background;
Texture2D expTexture;
Texture2D swordTexture;
Texture2D axeTexture;
Texture2D hammerTexture;
Texture2D heldWeaponTexture;
Texture2D heartTexture;

Sound lvlUpSound = LoadSound("Resources/sound.wav");

class Player
{
public:
    Facing dir = LEFT;
    Facing looking = LEFT;
    Weapon *heldWeapon;
    Color clr = WHITE;
    Texture2D playerLeftTexture;
    Texture2D playerRightTexture;

    float x;
    float y;

    int x_vel;
    int y_vel;

    float size = 60;

    float atkTimer = 0;
    float atkDur = 0.15;
    float atkCooldown = 0.15;

    int hitPoints = 30;

    float dmgTimer = 0;
    float dmgCooldown = 0;

    int lvl = 0;
    int exp = 0;
    int expToLvlUp = 100;

    int totalKills = 0;

    Player(float x, float y, int x_vel, int y_vel)
    {
        this->x = x;
        this->y = y;
        this->x_vel = x_vel;
        this->y_vel = y_vel;
        clr = WHITE;
    }

    void UpdatePlayer()
    {

        if (IsKeyDown(KEY_RIGHT) || IsKeyDown(KEY_D))
        {
            x += x_vel;
            dir = RIGHT;
            looking = RIGHT;
        }
        if (IsKeyDown(KEY_LEFT) || IsKeyDown(KEY_A))
        {
            x -= x_vel;
            dir = LEFT;
            looking = LEFT;
        }
        if (IsKeyDown(KEY_UP) || IsKeyDown(KEY_W))
        {
            y -= y_vel;
            dir = UP;
            looking = UP;
        }
        if (IsKeyDown(KEY_DOWN) || IsKeyDown(KEY_S))
        {
            y += y_vel;
            dir = DOWN;
            looking = DOWN;
        }

        if (atkTimer > 0)
        {
            atkTimer -= GetFrameTime();
            if (atkTimer < 0)
            {
                atkTimer = 0;
            }
        }

        if (dmgTimer > 0)
        {
            dmgTimer -= GetFrameTime();
            if (dmgTimer < 0)
            {
                dmgTimer = 0;
                clr = WHITE;
            }
        }

        RestrictMovement();
    }

    void RestrictMovement()
    {
        float Lwall = padding;
        float Twall = padding;
        float Rwall = GetScreenWidth() - padding - size;

        if (y < -Twall)
        {
            y = Twall;
        }

        if (y + size >= GetScreenHeight())
        {
            y = GetScreenHeight() - size;
        }

        if (x <= Lwall)
        {
            x = Lwall;
        }

        if (x >= Rwall)
        {
            x = Rwall;
        }
    }

    Rectangle GetRect()
    {
        return {x, y, size, size};
    }

    void DrawPlayer(bool atking)
    {
        Texture2D current = playerLeftTexture;

        if (looking == LEFT)
        {
            current = playerLeftTexture;
        }
        else if (looking == RIGHT)
        {
            current = playerRightTexture;
        }

        DrawTextureEx(current, Vector2{x, y}, 0, 3, clr);
    }

    void DrawHeldWeapon()
    {
        if (heldWeapon == nullptr)
        {
            return;
        }

        float scale = 2;
        float offsetX = 0;
        float offsetY = 0;

        if (looking == LEFT)
        {
            offsetX = -heldWeaponTexture.width * scale;
            offsetY = size / 2 - (heldWeaponTexture.height * scale) / 2;
        }
        else if (looking == RIGHT)
        {
            offsetX = size;
            offsetY = size / 2 - (heldWeaponTexture.height * scale) / 2;
        }
        else if (looking == UP)
        {
            offsetX = size / 2 - (heldWeaponTexture.height * scale) / 2;
            offsetY = -heldWeaponTexture.width * scale;
        }
        else if (looking == DOWN)
        {
            offsetX = size / 2 - (heldWeaponTexture.height * scale) / 2;
            offsetY = size;
        }

        DrawTextureEx(heldWeaponTexture, Vector2{x + offsetX, y + offsetY}, 0, scale, WHITE);
    }

    void DrawHitPoints()
    {
        int hitPointsToShow = (hitPoints + 9) / 10;
        heartTexture = LoadTexture("Resources/heart.png");

        for (int i = 0; i < hitPointsToShow; i++)
        {
            DrawTexture(heartTexture, GetScreenWidth() - (10 * ((i + 1) * 2)) - padding, 40, WHITE);
        }
    }

    void GetPlayerDamage()
    {
        if (dmgTimer == 0)
        {
            hitPoints--;
            dmgTimer = dmgCooldown;
            clr = RED;
            clr = WHITE;
        }
    }

    void IncreaseEXP(int toIncrease)
    {
        exp += toIncrease;

        if (exp >= expToLvlUp)
        {
            exp = 0;
            lvl++;

            expToLvlUp += 100;
            currentState = PAUSED;
        }
    }

    void DrawEXP()
    {
        char lvlTxt[16];
        char expTxt[16];

        sprintf(lvlTxt, "Level %d", lvl);
        sprintf(expTxt, "Exp; %d/%d", exp, expToLvlUp);

        DrawText(lvlTxt, padding + 10, 45, 45, WHITE);
        DrawText(expTxt, GetScreenWidth() / 3 - 20, 42, 42, WHITE);
    }

    void LoadTextures()
    {
        playerLeftTexture = LoadTexture("Resources/playerLeft.png");
        playerRightTexture = LoadTexture("Resources/playerRight.png");
    }
};

class Enemy
{
public:
    Facing looking = LEFT;
    Color clr = WHITE;

    Texture2D enemyLeftTexture;
    Texture2D enemyLeftAttackTexture;
    Texture2D enemyRightTexture;
    Texture2D enemyRightAttackTexture;

    float x;
    float y;

    int x_vel = 2;
    int y_vel = 2;

    float size = 60;

    float atkTimer = 0;
    float atkDur = 0.15;
    float atkCooldown = 0.15;

    int hitPoints = 10;

    float dmgTimer = 0;
    float dmgCooldown = 0;

    bool isAlive = true;

    Enemy(float x, float y, int x_vel, int y_vel)
    {
        this->x = x;
        this->y = y;
        this->x_vel = x_vel;
        this->y_vel = y_vel;
        clr = WHITE;
    }

    void EnemyUpdate(const Player &player)
    {
        int dist = 3;

        if (player.x < x - dist)
        {
            x -= x_vel;
            looking = LEFT;
        }
        else if (player.x > x + dist)
        {
            x += x_vel;
            looking = RIGHT;
        }

        if (player.y > y + dist)
        {
            y += y_vel;
        }
        else if (player.y < y - dist)
        {
            y -= y_vel;
        }

        if (atkTimer > 0)
        {
            atkTimer -= GetFrameTime();
            if (atkTimer < 0)
            {
                atkTimer = 0;
            }
        }

        if (dmgTimer > 0)
        {
            dmgTimer -= GetFrameTime();
            if (dmgTimer < 0)
            {
                dmgTimer = 0;
                clr = WHITE;
            }
        }
    }

    void GetEnemyDamage(int dmg)
    {
        if (dmgTimer == 0)
        {
            hitPoints -= dmg;
            dmgTimer = dmgCooldown;
            clr = RED;
            clr = WHITE;

            if (hitPoints <= 0)
            {
                isAlive = false;
            }
        }
    }

    Rectangle GetRect()
    {
        return {x, y, size, size};
    }

    Rectangle GetAtkArea()
    {
        float padding = 15;
        Rectangle AtkArea = {x - padding, y - padding, size + padding * 2, size + padding * 2};

        return AtkArea;
    }

    void LoadTextures(const string &path)
    {
        enemyLeftTexture = LoadTexture((path + "_Walk_Left.png").c_str());
        enemyLeftAttackTexture = LoadTexture((path + "Attack_Left.png").c_str());
        enemyRightTexture = LoadTexture((path + "_Walk_Right.png").c_str());
        enemyRightAttackTexture = LoadTexture((path + "_Attack_Right.png").c_str());
    }

    void UnloadTextures()
    {
        UnloadTexture(enemyLeftTexture);
        UnloadTexture(enemyLeftAttackTexture);
        UnloadTexture(enemyRightTexture);
        UnloadTexture(enemyRightAttackTexture);
    }

    void DrawEnemy()
    {
        Texture2D texture = enemyLeftTexture;

        bool isAtking = atkTimer > atkCooldown - atkDur;

        if (isAtking == true)
        {
            if (looking == LEFT)
            {
                texture = enemyLeftAttackTexture;
            }
            else
            {
                texture = enemyRightAttackTexture;
            }
        }
        else
        {
            if (looking == LEFT)
            {
                texture = enemyLeftTexture;
            }
            else
            {
                texture = enemyRightTexture;
            }
        }

        DrawTextureEx(texture, Vector2{x, y}, 0, 3, clr);
    }

    void spawnEnemy(float margin = 50)
    {
        int spawnPos = rand() % 4;

        if (spawnPos == 0)
        {
            x = -margin;
            y = (float)(rand() % GetScreenHeight());
        }
        else if (spawnPos == 1)
        {
            x = GetScreenWidth() + margin;
            y = (float)(rand() % GetScreenHeight());
        }
        else if (spawnPos == 2)
        {
            x = (float)(rand() % GetScreenWidth());
            y = -margin;
        }
        else if (spawnPos == 3)
        {
            x = (float)(rand() % GetScreenWidth());
            y = GetScreenHeight() + margin;
        }
    }
};

class Weapon
{
public:
    Texture2D playerLeftTexture;
    Texture2D playerRightTexture;

    float atkCooldown;
    float atkDur;
    int dmg;

    Vector2 worldPos;
    bool isSpawned = true;

    void LoadTextures(const string &path)
    {
        heldWeaponTexture = LoadTexture((path + ".png").c_str());
    }

    Weapon(float atkCooldown, float atkDur, int dmg)
    {
        this->atkCooldown = atkCooldown;
        this->atkDur = atkDur;
        this->dmg = dmg;
    }

    void RandomSpawn()
    {
        float scale = 4;
        int Xmax = GetScreenWidth() - heldWeaponTexture.width * scale;
        int Ymax = GetScreenHeight() - heldWeaponTexture.height * scale;

        worldPos.x = padding + (int)(rand() % int(Xmax - padding));
        worldPos.y = padding + (int)(rand() % int(Ymax - padding));
    }

    void DrawWeapon()
    {
        if (isSpawned == false)
        {
            return;
        }
        else
        {
            float scale = 4;
            Rectangle rectBorder = {worldPos.x, worldPos.y, heldWeaponTexture.width * scale, heldWeaponTexture.height * scale};
            DrawTextureEx(heldWeaponTexture, worldPos, 0, scale, WHITE);
            DrawRectangleLinesEx(rectBorder, 2, BLACK);
        }
    }

    Rectangle GetRect()
    {
        float scale = 4;
        Rectangle GetRect = {worldPos.x, worldPos.y, heldWeaponTexture.width * scale, heldWeaponTexture.height * scale};

        return GetRect;
    }

    void UnloadTextures()
    {
        UnloadTexture(heldWeaponTexture);
    }

    virtual void GetAtkBox(Rectangle box[], int &count, Player *player) = 0;

    virtual ~Weapon() = default;
};

class Pickup
{
public:
    float x;
    float y;
    float size = 20;

    bool pickedUp = false;

    Pickup(float x, float y)
    {
        this->x = x;
        this->y = y;
    }

    Rectangle GetRect()
    {
        return {x, y, size, size};
    }

    void DrawPickup()
    {
        if (pickedUp == false)
        {
            DrawTextureEx(expTexture, Vector2{x, y}, 0, 1, WHITE);
            DrawRectangleLinesEx(GetRect(), 1, BLACK);
        }
    }
};

class Golumn : public Enemy
{
public:
    Golumn() : Enemy(0, 0, 1, 1)
    {
        hitPoints = 10;
        spawnEnemy();

        LoadTextures("Resources/Enemy3");
    }
};

class Vampire : public Enemy
{
public:
    Vampire() : Enemy(0, 0, 3, 3)
    {
        hitPoints = 6;
        spawnEnemy();

        LoadTextures("Resources/Enemy2");
    }
};

class Skeleton : public Enemy
{
public:
    Skeleton() : Enemy(0, 0, 2, 2)
    {
        hitPoints = 3;
        spawnEnemy();

        LoadTextures("Resources/Enemy");
    }
};

class Sword : public Weapon
{
public:
    Sword() : Weapon(0.5, 0.5, 2)
    {
        LoadTextures("Resources/sword");
    }

    virtual void GetAtkBox(Rectangle box[], int &count, Player *player)
    {
        count = 0;

        float x = player->x;
        float y = player->y;
        int size = player->size;
        auto dir = player->dir;

        float range = 60;
        float border = 25;

        if (dir == LEFT)
        {
            box[count++] = {x - range, y, range, border};
            box[count++] = {x, y - range, range, border};
            box[count++] = {x, y + size, range, border};
        }
        else if (dir == RIGHT)
        {
            box[count++] = {x + size, y, range, border};
            box[count++] = {x, y - range, range, border};
            box[count++] = {x, y + size, range, border};
        }
        else if (dir == UP)
        {
            box[count++] = {x, y - range, border, range};
            box[count++] = {x - range, y, range, border};
            box[count++] = {x + size, y, range, border};
        }
        else if (dir == DOWN)
        {
            box[count++] = {x, y + size, border, range};
            box[count++] = {x - range, y, range, border};
            box[count++] = {x + size, y, range, border};
        }
    }
};

class Axe : public Weapon
{
public:
    Axe() : Weapon(0.7, 0.7, 3)
    {
        LoadTextures("Resources/axe");
    }

    virtual void GetAtkBox(Rectangle box[], int &count, Player *player)
    {
        count = 0;

        float x = player->x;
        float y = player->y;
        int size = player->size;
        auto dir = player->dir;

        float range = 30;
        float border = 25;

        if (dir == LEFT)
        {
            box[count++] = {x - range, y, range, border};
            box[count++] = {x, y - range, range, border};
        }
        else if (dir == RIGHT)
        {
            box[count++] = {x + size, y, range, border};
            box[count++] = {x, y - range, range, border};
        }
        else if (dir == UP)
        {
            box[count++] = {x, y - range, border, range};
            box[count++] = {x - range, y, range, border};
        }
        else if (dir == DOWN)
        {
            box[count++] = {x, y + size, border, range};
            box[count++] = {x - range, y, range, border};
        }
    }
};

class Hammer : public Weapon
{
public:
    Hammer() : Weapon(0.15, 0.15, 4)
    {
        LoadTextures("Resources/hammer");
    }

    virtual void GetAtkBox(Rectangle box[], int &count, Player *player)
    {
        count = 0;

        float x = player->x;
        float y = player->y;
        int size = player->size;
        auto dir = player->dir;

        float range = 20;
        float border = 25;

        if (dir == LEFT)
        {
            box[count++] = {x - range, y, range, border};
        }
        else if (dir == RIGHT)
        {
            box[count++] = {x + size, y, range, border};
        }
        else if (dir == UP)
        {
            box[count++] = {x, y - range, border, range};
        }
        else if (dir == DOWN)
        {
            box[count++] = {x, y + size, border, range};
        }
    }
};

class Game
{
public:
    void DrawTimer(float timeLeft)
    {
        int totalSec = (int)timeLeft;
        int min = totalSec / 60;
        int sec = totalSec % 60;

        char timerTxt[32];

        sprintf(timerTxt, "Time Remaining: %d:", min);

        if (sec < 10)
        {
            sprintf(timerTxt + strlen(timerTxt), "0");
        }

        sprintf(timerTxt + strlen(timerTxt), "%d", sec);

        DrawText(timerTxt, padding, GetScreenHeight() - 50, 28, WHITE);
        DrawText("Move: WASD or Arrow Keys", GetScreenWidth() - padding - 330, GetScreenHeight() - 50, 28, WHITE);
    }

    void EnemySpawnTime(float &timerLeft, float &spawnTimer, float &spawnCooldown, float &minCooldown, vector<Enemy *> &enemies)
    {
        float deltaTime = GetFrameTime();

        timerLeft -= deltaTime;
        float prog = 1.0f - (timerLeft / 150);
        spawnTimer += deltaTime;
        spawnCooldown = 3.0f - prog * 2.5f;

        if (timerLeft < 0.0f)
        {
            timerLeft = 0.0f;
        }

        if (spawnCooldown < minCooldown)
        {
            spawnCooldown = minCooldown;
        }

        if (spawnTimer >= spawnCooldown && timerLeft > 0.0f)
        {
            spawnTimer = 0;

            int type = rand() % 3;

            if (type == 0)
            {
                enemies.push_back(new Skeleton());
            }
            else if (type == 1)
            {
                enemies.push_back(new Vampire());
            }
            else
            {
                enemies.push_back(new Golumn());
            }
        }
    }

    void DrawLvlUp()
    {
        DrawRectangle(0, 0, GetScreenWidth(), GetScreenHeight(), Fade(WHITE, 0.5));

        PlaySound(lvlUpSound);

        DrawText("You've Leveled Up!", GetScreenWidth() / 2 - 120, 100, 32, GREEN);

        DrawText("1. - + Damage", GetScreenWidth() / 2 - 200, 260, 28, WHITE);
        DrawText("2. - + Speed", GetScreenWidth() / 2 - 200, 320, 28, WHITE);
        DrawText("3. - + Health", GetScreenWidth() / 2 - 200, 380, 28, WHITE);
    }

    void DrawWin(int kills, int lvl, int time)
    {
        char killsTxt[64];
        char lvlTxt[64];
        char timeTxt[64];

        const char *winTxt = "You Won!";
        const char *quitTxt = "Press 'Esc' to close the game";

        sprintf(killsTxt, "Enemies killed: %d", kills);
        sprintf(lvlTxt, "Highest Level Achieved: %d", lvl);
        sprintf(timeTxt, "Time Survived: %d secs", time);

        DrawRectangle(0, 0, GetScreenWidth(), GetScreenHeight(), Fade(WHITE, 0.5));

        DrawText(winTxt, (GetScreenWidth() - MeasureText(winTxt, 32)) / 2, 220, 32, BLACK);

        DrawText(killsTxt, (GetScreenWidth() - MeasureText(killsTxt, 30)) / 2, 310, 28, BLACK);
        DrawText(lvlTxt, (GetScreenWidth() - MeasureText(lvlTxt, 30)) / 2, 350, 28, BLACK);
        DrawText(timeTxt, (GetScreenWidth() - MeasureText(timeTxt, 30)) / 2, 370, 28, BLACK);

        DrawText(quitTxt, (GetScreenWidth() - MeasureText(quitTxt, 32)) / 2, 420, 32, BLACK);
    }

    void DrawLose(int kills, int lvl)
    {
        char killsTxt[64];
        char lvlTxt[64];

        const char *loseTxt = "Game Over!";
        const char *quitTxt = "Press 'Esc' to close the game";

        sprintf(killsTxt, "Enemies killed: %d", kills);
        sprintf(lvlTxt, "Highest Level Achieved: %d", lvl);

        DrawRectangle(0, 0, GetScreenWidth(), GetScreenHeight(), Fade(WHITE, 0.5));

        DrawText(loseTxt, (GetScreenWidth() - MeasureText(loseTxt, 32)) / 2, 220, 32, BLACK);

        DrawText(killsTxt, (GetScreenWidth() - MeasureText(killsTxt, 30)) / 2, 310, 28, BLACK);
        DrawText(lvlTxt, (GetScreenWidth() - MeasureText(lvlTxt, 30)) / 2, 350, 28, BLACK);

        DrawText(quitTxt, (GetScreenWidth() - MeasureText(quitTxt, 32)) / 2, 420, 32, BLACK);
    }

    void LoadTextures()
    {
        Background = LoadTexture("Resources/Background.png");
        expTexture = LoadTexture("Resources/expShard.png");
        heartTexture = LoadTexture("Resources/heartTexture.png");
    }

    void UnloadTextures()
    {
        UnloadTexture(Background);
        UnloadTexture(expTexture);
        UnloadTexture(heartTexture);
    }

    Color AttackBoxColor(Weapon *weapon, Sword *sword, Axe *axe, Hammer *hammer)
    {
        if (weapon == sword)
        {
            return (Color){255, 0, 0, 50};
        }
        else if (weapon == axe)
        {
            return (Color){0, 0, 255, 50};
        }
        else if (weapon == hammer)
        {
            return (Color){0, 255, 0, 50};
        }
        else
        {
            return (Color){125, 125, 125, 50};
        }
    }
};

int main(void)
{
    // Initialization
    //--------------------------------------------------------------------------------------
    const int screenWidth = 800;
    const int screenHeight = 800;

    Rectangle box[3];
    int count = 0;

    InitWindow(screenWidth, screenHeight, "Dungeon Crawler");

    SetTargetFPS(60); // Set our game to run at 60 frames-per-second
    //--------------------------------------------------------------------------------------
    // Main game loop

    Game game = Game();
    Player player(screenWidth / 2 - 15, screenHeight / 2 - 15, 5, 5);
    Sword sword;
    Axe axe;
    Hammer hammer;

    game.LoadTextures();
    player.LoadTextures();

    player.heldWeapon = &sword;
    axe.RandomSpawn();
    hammer.RandomSpawn();

    vector<Enemy *> waves;
    vector<Pickup> expShards;

    while (!WindowShouldClose()) // Detect window close button or ESC key
    {
        // Update
        //----------------------------------------------------------------------------------
        // TODO: Update your variables here
        //----------------------------------------------------------------------------------

        if (currentState == ACTION)
        {
            game.EnemySpawnTime(timerLeft, spawnTimer, spawnCooldown, minCooldown, waves);

            player.UpdatePlayer();

            for (Enemy *enemy : waves)
            {
                enemy->EnemyUpdate(player);
            }

            for (auto &exp : expShards)
            {
                if (!exp.pickedUp && CheckCollisionRecs(player.GetRect(), exp.GetRect()))
                {
                    exp.pickedUp = true;
                    player.IncreaseEXP(50);
                }
            }

            Rectangle playerRect = player.GetRect();
            for (Enemy *enemy : waves)
            {
                Rectangle enemyRect = enemy->GetRect();

                if (CheckCollisionRecs(playerRect, enemyRect))
                {
                    if (enemy->atkTimer <= 0)
                    {
                        enemy->atkTimer = enemy->atkCooldown;

                        player.GetPlayerDamage();

                        if (player.hitPoints <= 0)
                        {
                            currentState = LOSE;
                        }
                    }
                }
            }

            gameTimer = 1 - (timerLeft / 150);

            if (axe.isSpawned == true && CheckCollisionRecs(playerRect, axe.GetRect()))
            {
                player.heldWeapon = &axe;
                axe.isSpawned = false;
            }
            else if (hammer.isSpawned == true && CheckCollisionRecs(playerRect, hammer.GetRect()))
            {
                player.heldWeapon = &hammer;
                hammer.isSpawned = false;
            }

            if (player.atkTimer == 0)
            {
                count = 0;
                player.heldWeapon->GetAtkBox(box, count, &player);

                for (int i = 0; i < count; i++)
                {
                    for (Enemy *enemy : waves)
                    {
                        if (CheckCollisionRecs(box[i], enemy->GetRect()))
                        {
                            player.atkTimer = player.heldWeapon->atkCooldown + player.heldWeapon->atkDur;
                            enemy->GetEnemyDamage(player.heldWeapon->dmg);
                            break;
                        }
                    }
                }

                for (int i = (int)waves.size() - 1; i >= 0; i--)
                {
                    if (waves[i]->isAlive == false)
                    {
                        Pickup exp(waves[i]->x, waves[i]->y);
                        expShards.push_back(exp);

                        player.totalKills++;

                        delete waves[i];

                        waves.erase(waves.begin() + i);
                    }
                }
            }

            if (timerLeft <= 0.0 && waves.empty() == true)
            {
                currentState = WIN;
            }
        }
        else if (currentState == PAUSED)
        {
            if (IsKeyPressed(KEY_ONE))
            {
                player.heldWeapon->dmg += 5;
                currentState = ACTION;
            }
            else if (IsKeyPressed(KEY_TWO))
            {
                player.x_vel++;
                player.y_vel++;
                currentState = ACTION;
            }
            if (IsKeyPressed(KEY_THREE))
            {
                player.hitPoints += 5;
                currentState = ACTION;
            }
        }

        // Draw
        //----------------------------------------------------------------------------------
        BeginDrawing();
        ClearBackground(BLACK);
        DrawTexture(Background, 0, 0, WHITE);

        bool atking = player.atkTimer > player.atkCooldown;
        player.DrawPlayer(atking);
        player.DrawHeldWeapon();

        for (Enemy *enemy : waves)
        {
            enemy->DrawEnemy();
        }

        if (atking == true)
        {
            Color clr = game.AttackBoxColor(player.heldWeapon, &sword, &axe, &hammer);

            for (int i = 0; i < count; i++)
            {
                DrawRectangleRec(box[i], clr);

                Color border = {clr.r, clr.g, clr.b, 150};
                DrawRectangleLinesEx(box[i], 2, border);
            }
        }
        else
        {
            for (int i = 0; i < count; i++)
            {
                DrawRectangleLinesEx(box[i], 2, WHITE);
            }
        }

        if (gameTimer >= 0.33)
        {
            axe.DrawWeapon();
        }

        if (gameTimer >= 0.66)
        {
            hammer.DrawWeapon();
        }

        for (auto &exp : expShards)
        {
            exp.DrawPickup();
        }

        game.DrawTimer(timerLeft);
        player.DrawHitPoints();
        player.DrawEXP();

        if (currentState == PAUSED)
        {
            game.DrawLvlUp();
        }
        else if (currentState == WIN)
        {
            game.DrawWin(player.totalKills, player.lvl, (int)(150 - timerLeft));
        }
        if (currentState == LOSE)
        {
            game.DrawLose(player.totalKills, player.lvl);
        }

        EndDrawing();
        //----------------------------------------------------------------------------------
    }

    player.heldWeapon->UnloadTextures();
    game.UnloadTextures();
    for (Enemy *enemy : waves)
    {
        delete enemy;
    }

    waves.clear();
    // De-Initialization
    //--------------------------------------------------------------------------------------
    CloseWindow(); // Close window and OpenGL context
    //--------------------------------------------------------------------------------------

    return 0;
}