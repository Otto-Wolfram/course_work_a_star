using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Debugger : MonoBehaviour
{
    static Debugger instance;

    public static Debugger myInstance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<Debugger>();
            }

            return instance;
        }
    }

    public Grid grid;
    public Tilemap tilemap;
    public Tile tile;
    public Color openColor, closeColor, pathColor, startColor, goalColor;
    public Canvas canvas;
    public GameObject debugTextPrefab;
    List<GameObject> debugGameObjects = new List<GameObject>();

    public void CreateTiles(HashSet<Node> openList, HashSet<Node> closedList, Dictionary<Vector3Int, Node> allNodes, Vector3Int start, Vector3Int goal, Stack<Vector3Int> path = null)
    {
        /*
         * foreach (Node node in openList)
        {
            ColorTile(node.position, openColor);
        }

        foreach (Node node in closedList)
        {
            ColorTile(node.position, closeColor);
        }
        */
        if (path != null)
        {
            foreach (Vector3Int pos in path)
            {
                if (pos != start && pos != goal)
                {
                    ColorTile(pos, pathColor);
                }
            }
        }

        //ColorTile(start, startColor);
        //ColorTile(goal, goalColor);
        if (path != null)
        {
            foreach (KeyValuePair<Vector3Int, Node> node in allNodes)
            {
                if (node.Value.parent != null)
                {
                    if (path.Contains(node.Value.position) && node.Value.position != goal)
                    {
                        GameObject go = Instantiate(debugTextPrefab, canvas.transform);

                        go.transform.position = grid.CellToWorld(node.Key);

                        debugGameObjects.Add(go);

                        GenerateDebugText(node.Value, go.GetComponent<DebugText>());
                    }
                }
            }
        }
    }


    public void ColorTile(Vector3Int position, Color color)
    {
        tilemap.SetTile(position, tile);
        tilemap.SetTileFlags(position, TileFlags.None);
        tilemap.SetColor(position, color);
    }

    void GenerateDebugText(Node node, DebugText debugText)
    {
        if(node.parent.position.x < node.position.x && node.parent.position.y == node.position.y)
        {
            debugText.MyArrow.localRotation = Quaternion.Euler(new Vector3(0, 0, 180));
        }
        else if (node.parent.position.x < node.position.x && node.parent.position.y > node.position.y)
        {
            debugText.MyArrow.localRotation = Quaternion.Euler(new Vector3(0, 0, 135));
        }
        else if (node.parent.position.x < node.position.x && node.parent.position.y < node.position.y)
        {
            debugText.MyArrow.localRotation = Quaternion.Euler(new Vector3(0, 0, 225));
        }
        else if (node.parent.position.x > node.position.x && node.parent.position.y == node.position.y)
        {
            debugText.MyArrow.localRotation = Quaternion.Euler(new Vector3(0, 0, 0));
        }
        else if (node.parent.position.x > node.position.x && node.parent.position.y > node.position.y)
        {
            debugText.MyArrow.localRotation = Quaternion.Euler(new Vector3(0, 0, 45));
        }
        else if (node.parent.position.x > node.position.x && node.parent.position.y < node.position.y)
        {
            debugText.MyArrow.localRotation = Quaternion.Euler(new Vector3(0, 0, -45));
        }
        else if (node.parent.position.x == node.position.x && node.parent.position.y > node.position.y)
        {
            debugText.MyArrow.localRotation = Quaternion.Euler(new Vector3(0, 0, 90));
        }
        else if (node.parent.position.x == node.position.x && node.parent.position.y < node.position.y)
        {
            debugText.MyArrow.localRotation = Quaternion.Euler(new Vector3(0, 0, 270));
        }
    }

    void ShowHide()
    {
        canvas.gameObject.SetActive(!canvas.isActiveAndEnabled);
    }

    public void Reset()
    {
        foreach (GameObject go in debugGameObjects)
        {
            Destroy(go);
        }
    }
}
