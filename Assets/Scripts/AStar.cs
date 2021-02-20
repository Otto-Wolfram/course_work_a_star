using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    // хранятся стартовая и финишная позиция
    Vector3Int startPos, goalPos;
    // открытый и закрытый список
    HashSet<Node> openList, closedList;
    // все узлы поля
    Dictionary<Vector3Int, Node> allNodes = new Dictionary<Vector3Int, Node>();
    // сюда заносятся все препятсивия
    List<Vector3Int> waterTiles = new List<Vector3Int>();
    // найденный в итоге путь
    Stack<Vector3Int> path;
    // узел, который обрабатывается в данный момент
    Node current;

    void Inicialize()
    {
        current = GetNode(startPos);

        openList = new HashSet<Node>();
        closedList = new HashSet<Node>();

        openList.Add(current);
    }



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

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Algorithm();
        }
    }

    public void Algorithm()
    {
        if (current == null)
        {
            Inicialize();
        }

        while (openList.Count > 0 && path == null)
        {
            List<Node> neighbours = FindNeighbors(current.position);
            ExamineNeighbors(neighbours, current);
            UpdateCurrentTile(ref current);
            path = GeneratePath(current);
        }

        Debugger.myInstance.CreateTiles(openList, closedList, allNodes, startPos, goalPos, path);
    }

    // получает объект клетки поля по её координатам
    Node GetNode(Vector3Int position)
    {
        if (allNodes.ContainsKey(position))
        {
            return allNodes[position];
        }
        else
        {
            Node node = new Node(position);
            allNodes.Add(position, node);

            return node;
        }
    }

    // находит все соседние узлы данного
    List<Node> FindNeighbors(Vector3Int parentPos)
    {
        List<Node> neighbors = new List<Node>();

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                Vector3Int neigborPos = new Vector3Int(parentPos.x-x, parentPos.y-y, parentPos.z);

                if (x != 0 || y != 0)
                {
                    if (neigborPos != startPos && tileMap.GetTile(neigborPos) != null && !waterTiles.Contains(neigborPos))
                    {
                        Node neighbor = GetNode(neigborPos);
                        neighbors.Add(neighbor);
                    }
                    
                }
            }
        }

        return neighbors;
    }

    // оценивает узлы
    void ExamineNeighbors(List<Node> neighbors, Node current )
    {
        for (int i = 0; i < neighbors.Count; i++)
        {
            Node neighbor = neighbors[i];

            if (!ConnectedDiagonally(current, neighbor))
            {
                continue; // не допускаем пути между диагональными стенками
            }

            int gScore = DetermineGScore(neighbors[i].position, current.position);

            // если клетка-сосед уже находится в открытом листе нужно проверить
            // является ли путь через текущую лучшей альтернативой
            if (openList.Contains(neighbor))
            {
                if (current.G + gScore < neighbor.G)
                {
                    // пусть лучший, меняем родительскую клетку
                    CalcValues(current, neighbor, gScore);
                }
            }
            else if (!closedList.Contains(neighbor))
            {
                CalcValues(current, neighbor, gScore);
                openList.Add(neighbor);
            }

        }
    }

    // рассчитывает параметр G для текущей клетки по отношению к соседней
    int DetermineGScore(Vector3Int neighbor, Vector3Int current)
    {
        int gScore = 0;
        int x = current.x - neighbor.x;
        int y = current.y - neighbor.y;

        if (Mathf.Abs(x-y) % 2 == 1)
        {
            gScore = 10;
        } 
        else
        {
            gScore = 14;
        }

        return gScore;
    }

    // рассчитывает вес узла относительно соседа
    void CalcValues(Node parent, Node neighbor, int cost)
    {
        neighbor.parent = parent;
        // накапливаем стоимость пути
        neighbor.G = parent.G + cost;
        neighbor.H = (Mathf.Abs(neighbor.position.x-goalPos.x) + Mathf.Abs(neighbor.position.y - goalPos.y)) * 10;
        neighbor.F = neighbor.G + neighbor.H;
    }

    // меняет плитку уровня
    void ChangeTile(Vector3Int clickPos)
    {
        tileMap.SetTile(clickPos, tiles[(int)tileType]);

        if (tileType == TileType.WATER)
        {
            waterTiles.Add(clickPos);
        }
        else
        {
            if (tileType == TileType.START)
            {
                startPos = clickPos;
            }
            else if (tileType == TileType.GOAL)
            {
                goalPos = clickPos;
            }
        }
        
        
    }

    // меняет тип устанавливаемой плитки
    public void ChangeTileType(TileButton tileButton)
    {
        tileType = tileButton.tileType;
    }

    // изменяет состояние узла, который является текущим в рассмотрении
    void UpdateCurrentTile(ref Node current)
    {
        openList.Remove(current);
        closedList.Add(current);

        // если в открытом листе осталось что-то, то надо продолжать поиск
        if (openList.Count > 0)
        {
            current = openList.OrderBy(x => x.F).First();
        }
    }

    Stack<Vector3Int> GeneratePath(Node current)
    {
        // алгоритм достиг конечного узла
        if (current.position == goalPos)
        {
            Stack<Vector3Int> finalPath = new Stack<Vector3Int>();
            tileType = TileType.PATH;

            while (current.position != startPos)
            {
                if (current.position != goalPos)
                {
                    ChangeTile(current.position); // рисуем путь
                }

                finalPath.Push(current.position);
                current = current.parent;
            }

            return finalPath;
        }
        return null;
    }

    // проверяет, соединены ли узлы диагонально
    // чтобы предотвратить проход через горизонтально соединенные стены
    private bool ConnectedDiagonally(Node current, Node neighbor)
    {
        Vector3Int direct = current.position - neighbor.position;
        Vector3Int first = new Vector3Int(current.position.x + (direct.x * -1), 
            current.position.y, current.position.z);
        Vector3Int second = new Vector3Int(current.position.x,
            current.position.y + (direct.y * -1), current.position.z);

        if (waterTiles.Contains(first) || waterTiles.Contains(second))
        {
            return false;
        }

        return true;
    }

    public void Reset()
    {
        tileType = TileType.GRASS;
        foreach (KeyValuePair<Vector3Int, Node> node in allNodes)
        {
            ChangeTile(node.Value.position);
            node.Value.parent = null;
            node.Value.G = 0;
            node.Value.H = 0;
            node.Value.F = 0;
        }
        foreach (Vector3Int node in waterTiles)
        {
            ChangeTile(node);
        }
        Debugger.myInstance.Reset();
        allNodes = new Dictionary<Vector3Int, Node>();
        waterTiles = new List<Vector3Int>();
        path = null;
        current = null;
    }
}
