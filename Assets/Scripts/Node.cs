using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public int G { get; set; } // энергия обычная
    public int H { get; set; } // энергия эвристическая
    public int F { get; set; } // сумма энергий
    public Vector3Int position; // позиция клетки
    public Node parent; // ссылка на родительскую клетку 

    public Node(Vector3Int position)
    {
        this.position = position;
    }
}
