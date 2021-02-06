using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum TileType { START, GOAL, WATER, GRASS, PATH }

public class AStar : MonoBehaviour
{
    TileType tileType;
    [SerializeField] Tilemap tileMap;
    // Храним все возможные тайлы
    [SerializeField] Tile[] tiles;
    // объект Main Camera
    [SerializeField] Camera m_camera; 
    // сетка игрового мира
    [SerializeField] LayerMask layerMask;

    void Update()
    {
        // если нажали правой кнопкой мыши
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit2D hit = Physics2D.Raycast(m_camera.ScreenToWorldPoint(Input.mousePosition), 
                Vector2.zero, Mathf.Infinity, layerMask);

            // если кликнули по объекту
            if (hit.collider != null) 
            {
                // переход к мировой системе координат
                Vector3 mouseWorldPos = m_camera.ScreenToWorldPoint(Input.mousePosition);
                Vector3Int clickPos = tileMap.WorldToCell(mouseWorldPos);

                ChangeTile(clickPos);
            }
        }
    }

    // меняет плитку уровня
    void ChangeTile(Vector3Int clickPos)
    {
        tileMap.SetTile(clickPos, tiles[(int)tileType]);
    }

    // меняет тип устанавливаемой плитки
    public void ChangeTileType(TileButton tileButton)
    {
        tileType = tileButton.tileType;
    }
}
