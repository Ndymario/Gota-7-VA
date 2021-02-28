#include "raylib.h"

char game_title[] = "Gota7 Voice Assistant";
int WIDTH = 300;
int HEIGHT = 500;

bool inGame = true;

int main()
{
    InitWindow(WIDTH, HEIGHT, game_title);
    SetTargetFPS(60);
    InitAudioDevice();
    Sound s = LoadSound("demo.wav");
    PlaySound(s);
    while (!WindowShouldClose())
    {
        BeginDrawing();
        ClearBackground(RAYWHITE);
        EndDrawing();
    }
    return 0;
}