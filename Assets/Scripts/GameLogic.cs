using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLogic : MonoBehaviour
{
    public ArrayLayout boardLayout;
    
    [Header("UI Elements")]
    public Sprite[] pieces;
    public RectTransform gameBoard;
    public RectTransform killedBoard;

    [Header("Prefabs")]
    public GameObject nodePiece;
    public GameObject killedPiece;

    int width = 8;
    int height = 8;
    int[] fills;
    Node[,] board;

    List<NodePiece> update;
    List<NodePiece> dead;
    List<FlippedPieces> flipped;
    List<KilledPiece> killed;

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
            FlippedPieces flip = GetFlipped(piece);
            NodePiece flippedPiece = null;

            int x = (int)piece.index.x;
            fills[x] = Mathf.Clamp(fills[x] - 1, 0, width);

            List<Point> connected = IsConnected(piece.index, true);
            bool wasFlipped = (flip != null);

            if (wasFlipped) // if we flipped to make this update
            {
                flippedPiece = flip.GetOtherPiece(piece);
                AddPoints(ref connected, IsConnected(flippedPiece.index, true));
            }
            if (connected.Count == 0) // if we didn't make a match
            {
                if (wasFlipped) // if we flipped
                {
                    FlipPieces(piece.index, flippedPiece.index, false); // flip back to previous position

                }
            } 
            else // if we make a match
            {
                foreach (Point pnt in connected) // Remove the node pieces connected
                {
                    KillPiece(pnt);
                    Node node = GetNodeAtPoint(pnt);
                    NodePiece nodePiece = node.GetPiece();
                    if (nodePiece != null)
                    {
                        nodePiece.gameObject.SetActive(false);
                        dead.Add(nodePiece);
                    }
                    node.SetPiece(null);
                }

                ApplyGravityToBoard();
            }

            flipped.Remove(flip); // remove the flip
            update.Remove(piece);
        }
    }

    void ApplyGravityToBoard()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = (height -1); y >= 0; y--)
            {
                Point p = new Point(x, y);
                Node node = GetNodeAtPoint(p);
                int val = GetValueAtPoint(p);
                if (val != 0) continue; // if it is not a hole, do nothing
                for (int ny = (y-1); ny >= -1; ny--)
                {
                    Point next = new Point(x, ny);
                    int nextVal = GetValueAtPoint(next);
                    if (nextVal == 0) continue;
                    if (nextVal != -1) // if we did not hit an end, but it's not 0 then use this to fill the current hole
                    {
                        Node got = GetNodeAtPoint(next);
                        NodePiece piece = got.GetPiece();

                        // Set the hole
                        node.SetPiece(piece);
                        update.Add(piece);

                        // Replace the hole
                        got.SetPiece(null);
                    }
                    else // Hit an end
                    {
                        // FIll in the hole with the dead pieces
                        int newVal = FillPiece();
                        NodePiece piece;
                        Point fallPoint = new Point(x, (-1 - fills[x]));
                        if(dead.Count > 0)
                        {
                            NodePiece revived = dead[0];
                            revived.gameObject.SetActive(true);
                            piece = revived;
                            
                            dead.RemoveAt(0);
                        }
                        /// Usually won't use this, but if needed (For bombs or apecial event jellies
                        /// which we currently don't have, it will generate new pieces on the board
                        else
                        {
                            GameObject obj = Instantiate(nodePiece, gameBoard);
                            NodePiece n = obj.GetComponent<NodePiece>();
                            piece = n;
                        }

                        piece.Initialize(newVal, p, pieces[newVal - 1]);
                        piece.rect.anchoredPosition = GetPositionFromPoint(fallPoint);

                        Node hole = GetNodeAtPoint(p);
                        hole.SetPiece(piece);
                        ResetPiece(piece);
                        fills[x]++;
                    }
                    break;
                }
            }
        }
    }

    FlippedPieces GetFlipped(NodePiece p)
    {
        FlippedPieces flip = null;
        for (int i = 0; i < flipped.Count; i++)
        {
            if (flipped[i].GetOtherPiece(p) != null)
            {
                flip = flipped[i];
                break;
            }
        }
        return flip;
    }

    void StartGame()
    {
        fills = new int[width];
        string seed = GetRandomSeed();
        random = new System.Random(seed.GetHashCode());
        update = new List<NodePiece>();
        flipped = new List<FlippedPieces>();
        dead = new List<NodePiece>();
        killed = new List<KilledPiece>();

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
        //piece.flipped = null;
        update.Add(piece);
    }

    public void FlipPieces(Point one, Point two, bool main)
    {
        if (GetValueAtPoint(one) < 0) return;

        Node nodeOne = GetNodeAtPoint(one);
        NodePiece pieceOne = nodeOne.GetPiece();

        if (GetValueAtPoint(two) > 0)
        {
            Node nodeTwo = GetNodeAtPoint(two);
            NodePiece pieceTwo = nodeTwo.GetPiece();
            nodeOne.SetPiece(pieceTwo);
            nodeTwo.SetPiece(pieceOne);

            if (main)
            {
                flipped.Add(new FlippedPieces(pieceOne, pieceTwo));
            }

            update.Add(pieceOne);
            update.Add(pieceTwo);
        } 
        else
        {
            ResetPiece(pieceOne);
        }
    }

    void KillPiece(Point p)
    {
        List<KilledPiece> available = new List<KilledPiece>();
        for (int i = 0; i < killed.Count; i++)
        {
            if (!killed[i].falling)
            {
                available.Add(killed[i]);
            }
        }

        KilledPiece set = null;
        if (available.Count > 0)
        {
            set = available[0];
        }
        else
        {
            GameObject kill = GameObject.Instantiate(killedPiece, killedBoard);
            KilledPiece kPiece = kill.GetComponent<KilledPiece>();
            set = kPiece;
            killed.Add(kPiece);
        }

        int val = GetValueAtPoint(p) - 1;
        if (set != null && val >= 0 && val < pieces.Length) {
            set.Initialize(pieces[val], GetPositionFromPoint(p));
        }
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
                Node node = GetNodeAtPoint(new Point(x, y));
             
                int val = board[x, y].value;
                if (val <= 0) continue;
                GameObject p = Instantiate(nodePiece, gameBoard);
                NodePiece piece = p.GetComponent<NodePiece>();
                RectTransform rect = p.GetComponent<RectTransform>();
                rect.anchoredPosition = new Vector2(36 + (72 * x), -36 - (72 * y));
                piece.Initialize(val, new Point(x, y), pieces[val - 1]);
                node.SetPiece(piece);
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

    Node GetNodeAtPoint(Point p)
    {
        return board[p.x, p.y];
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
    NodePiece piece;

    public Node(int v, Point i)
    {
        value = v;
        index = i;
    }

    public void SetPiece(NodePiece p)
    {
        piece = p;
        value = (piece == null) ? 0 : piece.value;
        if (piece == null) return;
        piece.SetIndex(index);
    }

    public NodePiece GetPiece()
    {
        return piece;
    }
}

[System.Serializable]
public class FlippedPieces
{
    public NodePiece one;
    public NodePiece two;

    public FlippedPieces(NodePiece o, NodePiece t)
    {
        one = o;
        two = t;
    }

    public NodePiece GetOtherPiece(NodePiece p)
    {
        if (p == one) return two;
        
        else if (p == two) return one;
        
        else return null;
    }
}