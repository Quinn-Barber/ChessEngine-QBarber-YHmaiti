using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pieceInteraction : MonoBehaviour
{
    bool beingHeld = false;
    Vector2 origPos;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnMouseDrag()
    {
        if(beingHeld == false)
        {
            origPos = new Vector2(this.gameObject.transform.position.x, this.gameObject.transform.position.y);
        }
        beingHeld = true;
        Vector2 mousePos;
        mousePos = Input.mousePosition;
        mousePos = Camera.main.ScreenToWorldPoint(mousePos);
        this.gameObject.transform.position = mousePos;
    }

    private void OnMouseUp()
    {
        beingHeld = false;
        Vector2 newPos = new Vector2((float)Math.Round(this.gameObject.transform.position.x), (float)Math.Round(this.gameObject.transform.position.y));
        if(newPos.x < 1 || newPos.x > 8 || newPos.y < 1 || newPos.y > 8)
        {
            this.gameObject.transform.position = origPos;
            return;
        }
        this.gameObject.transform.position = newPos;
    }
}
