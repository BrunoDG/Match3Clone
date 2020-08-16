using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLogic : MonoBehaviour
{
    public ArrayLayout boardLayout;
    
    [Header("UI Elements")]
    public Sprite[] pieces;
    public RectTransform gameBoard;

    [Header("Prefabs")]
    public GameObject nodePiece;

    int width = 8;
    int height = 8;
    Node[,] board;

    List<NodePiece> update;


    System.Random random;
    void Start()
    {
        StartGame();
    }

    void Update()
    {
        List<NodePiece> updatedList = new List<NodePiece>();
        for (int i = 0; i < update.Count; i++)
        {
            NodePiece piece = update[i];
            if (!piece.UpdatePiece()) updatedList.Add(piece);
        }

        for (int i = 0; i < updatedList.Count; i++)
        {
            NodePiece piece = updatedList[i];
            update.Remove(piece);
        }
    }
    void StartGame()
    {
        string seed = GetRandomSeed();
        random = new System.Random(seed.GetHashCode());
        update = new List<NodePiece>();

        InitializeBoard();
        VerifyBoard();
        InstantiateBoard();
    }

    void InitializeBoard()
    {
        board = new Node[width, height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                board[x, y] = new Node((boardLayout.rows[y].row[x]) ? -1 : FillPiece(), new Point(x, y));
            }
        }
    }

    public void ResetPiece(NodePiece piece)
    {
        piece.ResetPositon();
        update.Add(piece);
    }

    void VerifyBoard()
    {
        List<int> remove;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Point p = new Point(x, y);
                int val = GetValueAtPoint(p);
                if (val <= 0) continue;

                remove = new List<int>();

                while(IsConnected(p, true).Count > 0)
                {
                    val = GetValueAtPoint(p);
                    if (remove.Contains(val)) remove.Add(val);
                    SetValueAtPoint(p, NewValue(ref remove));
                }
            }
        }
    }

    void InstantiateBoard()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int val = board[x, y].value;
                if (val <= 0) continue;
                GameObject p = Instantiate(nodePiece, gameBoard);
                NodePiece node = p.GetComponent<NodePiece>();
                RectTransform rect = p.GetComponent<RectTransform>();
                rect.anchoredPosition = new Vector2(36 + (72 * x), -36 - (72 * y));
                node.Initialize(val, new Point(x, y), pieces[val - 1]);
            }
        }
    }

    /// TODO: Simplify this function by creating another one 
    /// to read the points and make the iteration, so it's more readable
    /// In the future

    List<Point> IsConnected(Point p, bool main)
    {
        List<Point> connected = new List<Point>();
        int val = GetValueAtPoint(p);
        Point[] directions =
        {
            Point.Up,
            Point.Right,
            Point.Down,
            Point.Left
        };

        // Checking if there are two or more equal shapes in each direction
        
        foreach(Point dir in directions)
        {
            List<Point> line = new List<Point>();

            int same = 0;
            for (int i = 1; i < 3; i++)
            {
                Point check = Point.Add(p, Point.Multiply(dir, i));
                if (GetValueAtPoint(check) == val)
                {
                    line.Add(check);
                    same++;
                }
            }

            /// If there's more than 1 equal shapes in the same direction
            /// then we have a match
            
            if (same > 1)
            {
                // Add these points to the connected list
                AddPoints(ref connected, line); 
            }
        }

        // Checking if we're in the middle of two equal shapes
        for (int i = 0; i < 2; i++)
        {
            List<Point> line = new List<Point>();

            int same = 0;
            Point[] check = {
                Point.Add(p, directions[i]),
                Point.Add(p, directions[i + 2])
            };

            /// Check if both sides of the piece are the same,
            /// if they are, add them to the list

            foreach (Point next in check)
            {
                if (GetValueAtPoint(next) == val)
                {
                    line.Add(next);
                    same++;
                }
            }

            if (same > 1)
            {
                AddPoints(ref connected, line);
            }
        }

        // Check for a 2x2 square
        for (int i = 0; i < 4; i++)
        {
            List<Point> square = new List<Point>();

            int same = 0;
            int next = i + 1;
            if (next >= 4) next -= 4;

            Point[] check =
            {
                Point.Add(p, directions[i]),
                Point.Add(p, directions[next]),
                Point.Add(p, Point.Add(directions[i], directions[next]))
            };

            /// Check if both sides of the piece are the same,
            /// if they are, add them to the list

            foreach (Point pnt in check)
            {
                if (GetValueAtPoint(pnt) == val)
                {
                    square.Add(pnt);
                    same++;
                }
            }

            if (same > 2) AddPoints(ref connected, square);
        }

        if (main)
        {
            for (int i = 0; i < connected.Count; i++)
            {
                AddPoints(ref connected, IsConnected(connected[i], false));
            }
        }

        //if (connected.Count > 0) connected.Add(p);

        return connected;
    }

    void AddPoints(ref List<Point> points, List<Point> add)
    {
        foreach(Point p in add)
        {
            bool doAdd = true;
            
            for (int i = 0; i < points.Count; i++)
            {
                if (points[i].Equals(p))
                {
                    doAdd = false;
                    break;
                }
            }

            if (doAdd) points.Add(p);
        }
    }

    int FillPiece()
    {
        int val = 1;
        val = (random.Next(0, 100) / (100 / pieces.Length)) + 1;
        return val;
    }

    int GetValueAtPoint(Point p)
    {
        if (p.x < 0 || p.x >= width || p.y < 0 || p.y >= height) return -1;
        return board[p.x, p.y].value;
    }

    void SetValueAtPoint(Point p, int v)
    {
        board[p.x, p.y].value = v;
    }
    
    int NewValue( ref List<int> remove)
    {
        List<int> available = new List<int>();
        for (int i = 0; i < pieces.Length; i++)
        {
            available.Add(i + 1);
        }
        foreach (int i in remove) {
            available.Remove(i);
        }

        if (available.Count <= 0) return 0;
        return available[random.Next(0, available.Count)];
    }

    string GetRandomSeed()
    {
        string seed = "";
        string acceptableChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz1234567890!@#$%^&*()";
        for (int i = 0; i < 8; i++)
        {
            seed += acceptableChars[Random.Range(0, acceptableChars.Length)];
        }

        return seed;
    }

    public Vector2 GetPositionFromPoint(Point p)
    {
        return new Vector2(36 + (72 * p.x), -36 - (72 * p.y));
    }
}

[System.Serializable]
public class Node
{
    /// Those will be the node values that correspond to the pieces:
    /// 0 = blank space
    /// 1 = yellow jelly 
    /// 2 = green Jelly
    /// 3 - red jelly
    /// 4 - blue jelly
    /// 5 - pueple jelly
    /// -1 = hole

    public int value;
    public Point index;

    public Node(int v, Point i)
    {
        value = v;
        index = i;
    }
}