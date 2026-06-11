#include "MapManager.h"
#include <iostream>
#include "tileson.hpp"

MapManager::MapManager() : gameMap(nullptr)
{
	gameMap = new Gamemap();
}

MapManager::~MapManager()
{

	for (auto& pair : tilesetTextures)
	{
		if (pair.second.id > 0)
		{
			UnloadTexture(pair.second);
		}
	}

	delete gameMap;
}

bool MapManager::loadMap(const std::string& mapFilePath)
{

	tson::Tileson tileson;
	map = tileson.parse(mapFilePath);

	if (map->getStatus() != tson::ParseStatus::OK)
	{
		std::cout << "Failed to load map: " << map->getStatusMessage() << std::endl;
		return false;
	}

	if (map->getTilesets().empty())
	{
		std::cout << "Map has no tilesets!" << std::endl;
		return false;
	}

	for (auto& tileset : map->getTilesets())
	{
		auto imagePath = tileset.getImage().u8string();

		fs::path fullImagePath = fs::path(mapFilePath).parent_path() / imagePath;

		Texture2D texture = LoadTexture(fullImagePath.string().c_str());

		if (texture.id == 0)
		{
			std::cout << "Failed to load texture for tileset: " << fullImagePath.string() << std::endl;
			return false;
		}

		tilesetTextures[tileset.getFirstgid()] = texture;
	}

	parseWaypoints();
	parseTowerPlacements();

	return true;

}

void MapManager::parseWaypoints()
{
	if (!map) return;

	gameMap->waypoints.clear();

	bool foundLayer = false;

	auto& layers = map->getLayers();

	for (auto& layer : layers)
	{
		if (layer.getType() != tson::LayerType::ObjectGroup) continue;

		if (layer.getName() == "Waypoints" || layer.getName() == "Waypoint")
		{
			foundLayer = true;

			std::cout << "Found waypoint layer: " << layer.getName() << " with " << layer.getObjects().size() << " objects " << std::endl;

			for (auto& obj : layer.getObjects())
			{
				Waypoint wp;

				wp.pos = { (float)obj.getPosition().x, (float)obj.getPosition().y };

				auto* orderProp = obj.getProp("PathID");
				if (orderProp)
				{
					wp.order = orderProp->getValue<int>();
				}
				else {
					std::string name = obj.getName();

					size_t pos = name.find_last_of("0123456789");

					if (pos != std::string::npos)
					{
						size_t start = name.find_last_not_of("0123456789", pos);

						if (start != std::string::npos)
						{
							start++;
						}
						else
						{
							start = 0;
						}

						std::string numStr = name.substr(start, pos - start + 1);

						wp.order = std::stoi(numStr);
					}
					else
					{
						wp.order = 0;
					}

					std::cout << "Waypoint at ( " << wp.pos.x << " , " << wp.pos.y << " ) - no 'PathID' prop, using name order = " << wp.order << std::endl;

				}

				gameMap->waypoints.push_back(wp);
				std::cout << "Waypoint order = " << wp.order << "at ( " << wp.pos.x << " , " << wp.pos.y << " )" << std::endl;

			}
		}
	}

	if (!foundLayer)
	{
		std::cout << " Warning: no object layer named 'Waypoints' or 'Waypoint' has been found." << std::endl;
		return;
	}

	std::sort(gameMap->waypoints.begin(), gameMap->waypoints.end(), [](const Waypoint& a, Waypoint& b)
	{
		return a.order < b.order;
	});

	std::cout << "Total Waypoints loaded: " << gameMap->waypoints.size() << std::endl;
}

void MapManager::parseTowerPlacements()
{
	if (!map || !gameMap) return;

	gameMap->towerPlacements.clear();

	for (auto& layer : map->getLayers())
	{
		if (layer.getType() != tson::LayerType::ObjectGroup) continue;

		if (layer.getName().find("Placements") != std::string::npos)
		{
			std::cout << "Found placement layer: " << layer.getName() << std::endl;


			for (auto& obj : layer.getObjects())
			{

				auto* prop = obj.getProp("IsPlacement");

				if (prop && prop->getValue<bool>())
				{

					TowerPlacement placement;

					float objX = (float)obj.getPosition().x;
					float objY = (float)obj.getPosition().y;
					float objW = (float)obj.getSize().x;
					float objH = (float)obj.getSize().y;

					if (objW > 0 && objH > 0) {
						placement.pos = { objX + objW / 2.0f, objY + objH / 2.0f };
					}
					else
					{
						placement.pos = { objX, objY };
					}


					placement.radius = 25.0f;

					placement.IsOccupied = false;
					gameMap->towerPlacements.push_back(placement);

					std::cout << "Added placement at (" << placement.pos.x << "," << placement.pos.y << ")" << std::endl;

				}
			}
		}
	}
}

void MapManager::draw() const
{
	if (!map) return;

	int mapTileW = map->getTileSize().x;
	int mapTileH = map->getTileSize().y;

	for (auto& layer : map->getLayers())
	{
		if (layer.getType() != tson::LayerType::TileLayer) continue;

		const auto& tileData = layer.getTileData();

		for (const auto& [pos, tile] : tileData)
		{

			if (!tile) continue;
			int gid = tile->getId();
			if (gid == 0) continue;

			const tson::Tileset* tileset = map->getTilesetByGid(gid);
			if (!tileset) continue;

			int firstGid = tileset->getFirstgid();
			auto it = tilesetTextures.find(firstGid);
			if (it == tilesetTextures.end()) continue;

			Texture2D texture = it->second;

			int localId = gid - firstGid;


			int margin = tileset->getMargin();
			int spacing = tileset->getSpacing();
			int tilesetTileW = tileset->getTileSize().x;
			int tilesetTileH = tileset->getTileSize().y;

			int tilesetCols = tileset->getColumns();
			if (tilesetCols == 0) tilesetCols = (texture.width - 2 * margin + spacing) / (tilesetTileW + spacing);

			int tileCol = localId % tilesetCols;
			int tileRow = localId / tilesetCols;

			int srcX = margin + tileCol * (tilesetTileW + spacing);
			int srcY = margin + tileRow * (tilesetTileH + spacing);

			Rectangle srcRect = { (float)srcX, (float)srcY, (float)tilesetTileW, (float)tilesetTileH };

			int gridX = (int)std::get<0>(pos);
			int gridY = (int)std::get<1>(pos);

			Rectangle dstRect = { (float)(gridX * mapTileW), (float)(gridY * mapTileH), (float)mapTileW, (float)mapTileH };

			DrawTexturePro(texture, srcRect, dstRect, { 0, 0 }, 0.0f, WHITE);

		}


	}

}

void MapManager::drawObjs() const
{
	if (!map) return;

	for (const auto& placement : gameMap->towerPlacements)
	{
		DrawCircleV(placement.pos, 20, GREEN);
		DrawCircleLinesV(placement.pos, 20, DARKGREEN);
	}
}

