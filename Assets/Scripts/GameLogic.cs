using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLogic : MonoBehaviour
{
    public ArrayLayout boardLayout;
    public Sprite[] pieces;
    int width = 8;
    int height = 8;
    Node[,] board;

    System.Random random;
    void Start()
    {
        
    }

    void StartGame()
    {
        string seed = GetRandomSeed();
        random = new System.Random(seed.GetHashCode());

        InitializeBoard();
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

    void VerifyBoard()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Point p = new Point(x, y);
                int val = GetValueAtPoint(p);
                if (val <= 0) continue;


            }
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
        return board[p.x, p.y].value;
    }
    
    void Update()
    {
        
    }

    string GetRandomSeed()
    {
        string seed = "";
        string acceptableChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz1234567890!@#$%^&*()";
        for (int i = 0; i < 16; i++)
        {
            seed += acceptableChars[Random.Range(0, acceptableChars.Length)];
        }

        return seed;
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