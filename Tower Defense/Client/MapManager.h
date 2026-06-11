#pragma once

#include "GameStruct.h"
#include "tileson.hpp"
#include <memory>
#include <string>
#include <vector>
#include <unordered_map>

namespace tson
{
	class Map;
	class Tileset;
}

class MapManager
{

private:

	Gamemap* gameMap;
	std::unique_ptr<tson::Map> map;
	std::unordered_map<int, Texture2D> tilesetTextures;

	void parseWaypoints();
	void parseTowerPlacements();

public:
	MapManager();
	~MapManager();

	bool loadMap(const std::string& mapFilePath);

	Gamemap* getGameMap() const { return gameMap; }
	void draw() const;
	void drawObjs() const;

	tson::Map* getMap() const { return map.get(); }

};
