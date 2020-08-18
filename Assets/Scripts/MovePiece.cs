using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine;

public class MovePiece : MonoBehaviour
{
    public static MovePiece instance;
    GameLogic game;

    NodePiece moving;
    Point newIndex;
    Vector2 mouseStart;

    List<NodePiece> nodes;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        game = GetComponent<GameLogic>();
        nodes = new List<NodePiece>();
    }

    void Update()
    {
        if(moving != null)
        {
            if (moving.drag)
            {
                Vector2 dir = ((Vector2)Input.mousePosition - mouseStart);
                Vector2 nDir = dir.normalized; // Normalized direction value
                Vector2 aDir = new Vector2(Mathf.Abs(dir.x), Mathf.Abs(dir.y)); // Absolute direction value

                newIndex = Point.Clone(moving.index);
                Point add = Point.Zero;

                // If your mouse moves further from the 36x36 area...
                if (dir.magnitude > 36)
                {
                    // Depending on the direction, you can...
                    if (aDir.x > aDir.y) // Obtain the axis value as (1, 0) or (-1, 0)
                    {
                        add = (new Point((nDir.x > 0) ? 1 : -1, 0));
                    }
                    else if (aDir.y > aDir.x) // Obtain the axis value as (0, 1) or (0, -1)
                    {
                        add = (new Point(0, (nDir.y > 0) ? -1 : 1));
                    }
                }
                newIndex.Add(add);

                Vector2 pos = game.GetPositionFromPoint(moving.index);
                if (!newIndex.Equals(moving.index))
                {
                    pos += Point.Multiply(new Point(add.x, -add.y), 18).ToVector();
                }
                moving.MovePositionTo(pos);
            }
            if (moving.selected && nodes.Count > 1)
            {
                Vector2 dir = moving.pos - nodes[0].pos;
                Debug.Log("Direction - " + dir);
                Vector2 nDir = dir.normalized;
                Vector2 aDir = new Vector2(Mathf.Abs(dir.x), Mathf.Abs(dir.y));

                newIndex = Point.Clone(moving.index);
                Point add = Point.Zero;

                if (dir.magnitude > 36 && dir.magnitude < 108)
                {
                    // Depending on the direction, you can...
                    if (aDir.x > aDir.y) // Obtain the axis value as (1, 0) or (-1, 0)
                    {
                        add = (new Point((nDir.x > 0) ? -1 : 1, 0));
                    }
                    else if (aDir.y > aDir.x) // Obtain the axis value as (0, 1) or (0, -1)
                    {
                        add = (new Point(0, (nDir.y > 0) ? 1 : -1));
                    }
                }
                newIndex.Add(add);

                Vector2 pos = game.GetPositionFromPoint(moving.index);
                if (!newIndex.Equals(moving.index))
                {
                    pos += Point.Multiply(new Point(add.x, -add.y), 18).ToVector();
                }
                moving.MovePositionTo(pos);
                nodes.Clear();
                moving.selected = false;
                DropPiece();
            }
        }
    }

    public void MoveCurrentPiece(NodePiece piece)
    {
        if (moving != null) return;
        moving = piece;
        mouseStart = Input.mousePosition;
    }

    public void CheckSelectedNodes(NodePiece piece)
    {
        nodes.Add(piece);
        if (nodes.Count > 1)
        {
            if (moving != null) return;
            moving = piece;
        }
    }

    public void DropPiece()
    {
        if (moving == null) return;

        if (!newIndex.Equals(moving.index)) // If piece is different to the other one
        {
            game.FlipPieces(moving.index, newIndex, true); // move the piece
        }
        else
        {
            game.ResetPiece(moving); // reset to original place
        }
        moving = null;
    }
}
