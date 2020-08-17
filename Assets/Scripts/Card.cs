using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card : MonoBehaviour
{
    CardDescription baseCard;
    public bool selected = false;
    public bool dragging = false;

    public bool isDraggable = true;

    public Vector3 startDragPos;
    public Vector3 currMousePos;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (dragging)
        {
            transform.position = currMousePos;
        }
    }

    void OnMouseDown()
    {
        if (isDraggable)
        {
            Debug.Log("Start Drag");


            dragging = true;
            startDragPos = Camera.main.ScreenToWorldPoint(
                Input.mousePosition + new Vector3(0, 0, transform.position.z - Camera.main.transform.position.z));
            currMousePos = startDragPos;
            startDragPos -= transform.position;

        }
    }

    void OnMouseDrag()
    {
        if (dragging)
        {
            currMousePos = Camera.main.ScreenToWorldPoint(
                Input.mousePosition + new Vector3(0, 0, GameConstants.Z_LAYERS.DRAG_LAYER - Camera.main.transform.position.z));
        }
    }

    private void OnMouseUp()
    {
        dragging = false;
    }
}
