using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pieceInteraction : MonoBehaviour
{
	private Camera PlayerCam;           // Camera used by the player
	private int _activePlayer;
	private bool _player1AI;
	private bool _player2AI;
	bool beingHeld = false;
    Vector2 origPos;

    // Start is called before the first frame update
    void Start()
    {
		PlayerCam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>(); // Find the Camera's GameObject from its tag 
		
	}

    // Update is called once per frame
    void Update()
    {
		
	}

	private void OnMouseDrag()
	{
		if (beingHeld == false)
		{
			origPos = new Vector2(this.gameObject.transform.position.x, this.gameObject.transform.position.y);
			
		}
		beingHeld = true;
		this.gameObject.GetComponent<Collider>().enabled = false;
		Vector2 mousePos;
		mousePos = Input.mousePosition;
		mousePos = Camera.main.ScreenToWorldPoint(mousePos);
		this.gameObject.transform.position = mousePos;
	}

	private void OnMouseUp()
	{
		Ray ray;
		RaycastHit hitInfo;
		beingHeld = false;
		Vector2 newPos = new Vector2((float)Math.Round(this.gameObject.transform.position.x), (float)Math.Round(this.gameObject.transform.position.y));
		if ( !isLegalMove(newPos) )
        {
			this.gameObject.GetComponent<Collider>().enabled = true;
			this.gameObject.transform.position = origPos;
			return;
		}
		ray = PlayerCam.ScreenPointToRay(Input.mousePosition);
		if (Physics.Raycast(ray, out hitInfo))
		{
			this.gameObject.GetComponent<Collider>().enabled = true;
			if (hitInfo.collider.gameObject.name != this.gameObject.name)
			{

				if (hitInfo.collider.gameObject.name.ToCharArray()[0] != this.gameObject.name.ToCharArray()[0])
				{
					// If they are not the same color
					hitInfo.collider.gameObject.SetActive(false);
					this.gameObject.transform.position = newPos;
				}
				else
				{
					// If they are the same color
					this.gameObject.transform.position = origPos;
				}
				return;
			}
		}
		this.gameObject.GetComponent<Collider>().enabled = true;
		if (newPos.x < 1 || newPos.x > 8 || newPos.y < 1 || newPos.y > 8)
		{
			this.gameObject.transform.position = origPos;
			return;
		}
		this.gameObject.transform.position = newPos;
	}

	private bool isLegalMove(Vector2 newPos)
    {
		String tagName = this.gameObject.tag;
        switch (tagName)
        {
			case "King":
				if(newPos.x >= (origPos.x + 2) || newPos.x <= (origPos.x - 2) || newPos.y >= (origPos.y + 2) || newPos.y <= (origPos.y - 2))
                {
					return false;
                }
				break;
			case "Queen":
				if(newPos.y != origPos.y && newPos.x != origPos.x)
                {
					if( Math.Abs(newPos.x - origPos.x) != Math.Abs(newPos.y - origPos.y) )
                    {
						return false;
					}
				}
				break;
			case "Bishop":
                if ( Math.Abs(newPos.x - origPos.x) != Math.Abs(newPos.y - origPos.y) )
                {
					return false;
                }
				break;
			case "Knight":
				break;
			case "Rook":
				break;
			case "Pawn":
				if (Math.Abs(origPos.x - newPos.x) > 0) return false;
				if( Math.Abs(origPos.y - newPos.y) > 1)
                {
					if( Math.Abs(origPos.y - newPos.y ) == 2)
                    {
						if( (origPos.y != 7 && origPos.y != 2) )
                        {
							return false;
                        }
                        else
                        {
							return true;
                        }
                    }
                    else
                    {
						return false;
                    }
                }
				break;
		}
		return true;
    }

}
