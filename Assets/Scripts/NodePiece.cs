using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Runtime.CompilerServices;

public class NodePiece : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
{
    public int value;
    public Point index;

    [HideInInspector]
    public Vector2 pos;
    [HideInInspector]
    public RectTransform rect;

    Image img;

    public void Initialize(int v, Point p, Sprite piece)
    {
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
        pos = new Vector2(32 + (64 * index.x), -32 - (64 * index.y));
    }

    void UpdateName()
    {
        transform.name = "Node [" + index.x + " , " + index.y + "]";
    }

    void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
    {
        throw new System.NotImplementedException();
    }

    void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("Grab " + transform.name);
        //throw new System.NotImplementedException();
    }

    void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Let Go " + transform.name);
        //throw new System.NotImplementedException();
    }
}
