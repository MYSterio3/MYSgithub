using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileMap_Render_System : MonoBehaviour
{
    private TileMap_Controller _mapController;
    public TileMap_Controller mapController { get => _mapController; set => _mapController = value; }

    private void Awake()
    {
        if (gameObject.TryGetComponent(out TileMap_Controller mapController)) { this.mapController = mapController; }
    }

    private void Set_New_Tiles(int size)
    {
        mapController.currentMap.mapSize = size;

        int rowNum = 0;
        int columnNum = 0;

        for (int i = 0; i < size * size; i++)
        {
            // set position
            Vector2 position = new(rowNum - 2, -columnNum + 2);

            // instantiate
            GameObject tile = Instantiate(mapController.controller.prefabsData.Get_Random_Tile(), position, Quaternion.identity);

            // set inside tile map as child
            tile.transform.parent = mapController.currentMap.transform;

            // get tile controller component
            if (tile.TryGetComponent(out Tile_Controller tileData))
            {
                // add to tiles list
                mapController.currentMap.tiles.Add(tileData);

                // set tile data
                tileData.Set_Data(mapController);

                // update tile position
                tileData.Update_Position(rowNum, columnNum);
            }

            // tile num update
            rowNum++;

            if (rowNum <= 4) continue;

            rowNum = 0;
            columnNum++;
        }
    }
    public void Set_New_Map(int mapSize)
    {
        // spawn map prefab
        GameObject setMap = Instantiate(this.mapController.controller.prefabsData.Get_MapController());

        // set map inside tilemap controller as child
        setMap.transform.parent = transform;

        // connect to current map component
        if (!setMap.TryGetComponent(out Map_Controller mapController)) return;
        this.mapController.currentMap = mapController;

        // add new map to all map list
        this.mapController.allMaps.Add(this.mapController.currentMap);

        // set tiles inside current map
        Set_New_Tiles(mapSize);
    }

    public void Render_Next_Map(bool isRowMove)
    {
        // save previous map
        Map_Controller previousMap = mapController.currentMap;

        // save player position
        Prefab_Controller playerPrefabController = mapController.playerPrefabController;
        int previousRow = playerPrefabController.currentRowNum;
        int previousColumn = playerPrefabController.currentColumnNum;

        // save map position
        int previousPosX = previousMap.positionX;
        int previousPosY = previousMap.positionY;

        // save map size
        int mapSize = mapController.currentMap.mapSize - 1;

        // remove player from previous map
        Tile_Controller playerTile = mapController.Get_Tile(previousRow, previousColumn);
        playerTile.Remove_Prefab(Prefab_Type.character, 0);

        // save and hide previous map
        mapController.currentMap.Map_Activation(false);
        
        // determine if row or column
        if (isRowMove) { previousRow += mapSize; previousPosX--; }
        else { previousColumn += mapSize; previousPosY++; }

        // set player to left and top
        if (previousRow > mapSize) { previousRow = 0; previousPosX +=2; }
        else if (previousColumn > mapSize) { previousColumn = 0; previousPosY -=2; }


        // new map render check
        bool hasMap = mapController.Has_Map(previousPosX, previousPosY);

        if (hasMap)
        {
            Map_Controller loadMap = mapController.Get_Map(previousPosX, previousPosY);
            loadMap.Map_Activation(true);
            mapController.currentMap = loadMap;
        }
        else
        {
            Set_New_Map(5);
            mapController.currentMap.Update_Position(previousPosX, previousPosY);
        }

        // set player on map
        mapController.Set_Character(0, previousRow, previousColumn);

        // update player map position
        Map_Controller currentMap = mapController.currentMap;
        playerPrefabController.Update_Map_Position(currentMap.positionX, currentMap.positionY);

        // set player tile
        if (!hasMap) mapController.Set_Player_Tile(false);
    }
}