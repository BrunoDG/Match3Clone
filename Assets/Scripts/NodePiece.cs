using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Runtime.CompilerServices;
using System.Data.Odbc;
using System.Runtime.ExceptionServices;

public class NodePiece : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
{
    public int value;
    public Point index;
    public bool selected = false;
    public bool drag = false;

    //List<Point>

    [HideInInspector]
    public Vector2 pos;
    [HideInInspector]
    public RectTransform rect;

    bool updating;
    Image img;

    public void Initialize(int v, Point p, Sprite piece)
    {
        //flipped = null;
        img = GetComponent<Image>();
        rect = GetComponent<RectTransform>();

        value = v;
        SetIndex(p);
        img.sprite = piece;
    }

    public void SetIndex(Point p)
    {
        index = p;
        ResetPositon();
        UpdateName();
    }

    public void ResetPositon()
    {
        pos = new Vector2(36 + (72 * index.x), -36 - (72 * index.y));
    }
    public void MovePosition(Vector2 move)
    {
        rect.anchoredPosition += move * Time.deltaTime * 18f;
    }

    public void MovePositionTo(Vector2 move)
    {
        rect.anchoredPosition = Vector2.Lerp(rect.anchoredPosition, move, Time.deltaTime * 18f);
    }

    void UpdateName()
    {
        transform.name = "Node [" + index.x + " , " + index.y + "]";
    }

    public bool UpdatePiece()
    {
        if (Vector3.Distance(rect.anchoredPosition, pos) > 1)
        {
            MovePositionTo(pos);
            updating = true;
        } 
        else
        {
            rect.anchoredPosition = pos;
            updating = false;
        }
        return updating;
    }

    void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
    {
        if (updating) return;
        drag = true;
        MovePiece.instance.MoveCurrentPiece(this);
    }

    void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
    {
        drag = false;
        MovePiece.instance.DropPiece();
    }

    void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
    {
        selected = true;
        MovePiece.instance.CheckSelectedNodes(this);
    }


}
