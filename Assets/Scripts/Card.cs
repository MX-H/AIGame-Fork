using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card : MonoBehaviour
{
    CardDescription baseCard;
    public bool selected = false;
    public bool dragging = false;
    public bool hovering = false;

    public bool isDraggable = true;

    public Vector3 startDragPos;
    public Vector3 currMousePos;


    private Vector3 savedPosition;
    private Quaternion savedRotation;
    private Vector3 savedScale;

    void Start()
    {
        SaveTransform();
    }

    // Update is called once per frame
    void Update()
    {
        if (dragging)
        {
            transform.position = currMousePos;
            Debug.DrawRay(currMousePos, Camera.main.transform.forward * 100, Color.red);

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

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            hovering = false;
        }
    }

    void OnMouseDrag()
    {
        if (dragging)
        {
            currMousePos = Camera.main.ScreenToWorldPoint(
                Input.mousePosition + new Vector3(0, 0, GameConstants.Z_LAYERS.DRAG_LAYER - Camera.main.transform.position.z));
            transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position, Camera.main.transform.up);


        }

    }

    private void OnMouseEnter()
    {
        if (!IsInteracting())
        {
            Debug.Log("Hovering");
            hovering = true;
            SaveTransform();

            transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position, Camera.main.transform.up);
            transform.position += transform.forward * ((GameConstants.Z_LAYERS.DRAG_LAYER - transform.position.z) / transform.forward.z);
            transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward, Camera.main.transform.up);

            Renderer rend = GetComponent<Renderer>();
            Vector3 extents = rend.bounds.extents;
            Vector3 bottomBound = rend.bounds.center - new Vector3(0, extents.y, extents.z);
            Vector3 topBound = rend.bounds.center - new Vector3(0, -extents.y, extents.z);

            float distance = Vector3.Dot((bottomBound - Camera.main.transform.position), Camera.main.transform.forward);
            float frustumHalfHeight = distance * Mathf.Tan(Camera.main.fieldOfView * 0.5f * Mathf.Deg2Rad);
            Vector3 camCenter = distance * Camera.main.transform.forward + Camera.main.transform.position;

            // Keep hover zoom-in within camera bounds
            if (Mathf.Abs(Vector3.Dot(bottomBound, Camera.main.transform.up)) > frustumHalfHeight)
            {
                Vector3 camBottom = camCenter - Camera.main.transform.up * frustumHalfHeight;
                transform.position += Vector3.Dot((camBottom - bottomBound), Camera.main.transform.up) * Camera.main.transform.up;
            }
            else if (Mathf.Abs(Vector3.Dot(topBound, Camera.main.transform.up)) > frustumHalfHeight)
            {
                Vector3 camTop = camCenter + Camera.main.transform.up * frustumHalfHeight;
                transform.position += Vector3.Dot((camTop - topBound), Camera.main.transform.up) * Camera.main.transform.up;
            }

        }
    }

    void OnMouseUp()
    {
        dragging = false;
    }

    void OnMouseOver()
    {

    }

    private void OnMouseExit()
    {
        if (hovering)
        {
            hovering = false;
            RestoreTransform();
        }
    }

    public bool IsInteracting()
    {
        return selected || dragging || hovering;
    }

    private void SaveTransform()
    {
        savedPosition = transform.localPosition;
        savedRotation = transform.localRotation;
        savedScale = transform.localScale;
    }

    private void RestoreTransform()
    {
        transform.localPosition = savedPosition;
        transform.localRotation = savedRotation;
        transform.localScale = savedScale;
    }
}
