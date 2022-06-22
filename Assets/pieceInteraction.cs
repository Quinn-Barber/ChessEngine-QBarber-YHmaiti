/*
 * Chess Engine pieceInteraction by Quinn Barber & Yohan Hmaiti
 * This C-Sharp script is a component in every single piece on
 * the chess board, and it ultimately determines piece movement,
 * piece interaction with the mouse, updating board positions,
 * and general legal flow of game, until the game is over.
 * Summer 2022 Project
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class pieceInteraction : MonoBehaviour
{
	// Camera used by the player
	private Camera PlayerCam;
	
	// Which player is AI and turn order
	private bool _player1AI;
	private bool _player2AI;

	// Variables for piece movement and board set-up (whether a piece is held and where it originally was)
	bool setUp = false;
	bool beingHeld = false;
    Vector2 origPos;

	// 2D Array for the pieces on the chess board
	public GameObject[][] board;

	// Start is called before the first frame update 
	void Start()
    {
		// Find the Camera's GameObject from its tag
		PlayerCam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
		
		// Initialize the chess board
		if(setUp == false)
        {
			setUp = true;
			board = new GameObject[8][];

			// Fill the chess board with each starting piece (using collisions based on rays from certain positions)
			for (int i = 0; i < 8; i++)
			{
				board[i] = new GameObject[8];
				for (int j = 0; j < 8; j++)
				{
					Ray ray;
					RaycastHit hitInfo;
					Vector2 pos = new Vector2((i + 1), (j + 1));
					pos = Camera.main.WorldToScreenPoint(pos);
					ray = PlayerCam.ScreenPointToRay(pos);
					if (Physics.Raycast(ray, out hitInfo))
					{
						GameObject newObject = (GameObject)hitInfo.collider.gameObject;
						board[i][j] = newObject;
					}
					else
					{
						board[i][j] = null;
					}
				}
			}
		}

	}

    // Update is called once per frame
    void Update()
    {

	}

	private void OnMouseDrag()
	{
		// If a piece is not held, one is being picked up, set the original position of the piece to origPos to save it as it follows the mouse
		if (beingHeld == false)
		{
			origPos = new Vector2(this.gameObject.transform.position.x, this.gameObject.transform.position.y);
		}
		beingHeld = true;
		
		// Turn of the collision of this piece as it moves with the mouse and before it is placed down
		this.gameObject.GetComponent<Collider>().enabled = false;

		// Follow the position of the mouse and set the piece to follow this position as its being held (snaps to center)
		Vector2 mousePos;
		mousePos = Input.mousePosition;
		mousePos = Camera.main.ScreenToWorldPoint(mousePos);
		this.gameObject.transform.position = mousePos;

	}

	private void OnMouseUp()
	{
		// No piece is being held anymore as the mouse was unclicked so set this to false
		beingHeld = false;

		// The potential new position of the piece is the current position of the mouse snapped to the nearest square
		Vector2 newPos = new Vector2((float)Math.Round(this.gameObject.transform.position.x), (float)Math.Round(this.gameObject.transform.position.y));

		// Check if this move is legal, if it is not reenable this pieces collision and set it back to its original position, return.
		if ( !isLegalMove(newPos) )
        {
			this.gameObject.GetComponent<Collider>().enabled = true;
			this.gameObject.transform.position = origPos;
			return;
		}

		// If it is legal, see if it has collided with any piece using collisions
		Ray ray;
		RaycastHit hitInfo;
		ray = PlayerCam.ScreenPointToRay(Input.mousePosition);

		// If it has collided, check what color the piece is to see what to do next
		if (Physics.Raycast(ray, out hitInfo))
		{
			// Turn back on the collision as the collision test has already taken place.
			this.gameObject.GetComponent<Collider>().enabled = true;

			if (hitInfo.collider.gameObject.name.ToCharArray()[0] != this.gameObject.name.ToCharArray()[0])
			{
				// If they are not the same color, it is capturing the piece, update the chess board as necessary
				hitInfo.collider.gameObject.SetActive(false);
				this.gameObject.transform.position = newPos;
			}
			else
			{
				// If they are the same color it is an illegal move, set it back to its original position
				this.gameObject.transform.position = origPos;
				return;
			}
		}

		// If it is legal and hasn't collided with anything, update the position and board as necessary (turn back on the collision as well)
		this.gameObject.GetComponent<Collider>().enabled = true;
		this.gameObject.transform.position = newPos;

		//Debug.Log($"{board[(int)origPos.x - 1][(int)origPos.y - 1]}");
		//Debug.Log($"{board[(int)newPos.x - 1][(int)newPos.y - 1]}");
		Debug.Log($"{origPos.x} {origPos.y} {newPos.x} {newPos.y}");

		board = new GameObject[8][];

		for (int i = 0; i < 8; i++)
		{
			board[i] = new GameObject[8];
			for (int j = 0; j < 8; j++)
			{
				Vector2 pos = new Vector2((i + 1), (j + 1));
				pos = Camera.main.WorldToScreenPoint(pos);
				ray = PlayerCam.ScreenPointToRay(pos);
				if (Physics.Raycast(ray, out hitInfo))
				{
					GameObject newObject = (GameObject)hitInfo.collider.gameObject;
					board[i][j] = newObject;
				}
				else
				{
					board[i][j] = null;
				}
			}
		}
		for(int i = 0; i < 8; i++)
        {
			for(int j = 0; j < 8; j++)
            {
				//Debug.Log($"{board[j][i]} at {j} {i}");
			}
        }


	}

	private bool isLegalMove(Vector2 newPos)
    {
		// Get the object tag to decide if this move is legal based on the type of piece, original position, and new position.
		String tagName = this.gameObject.tag;

		// If it is out of bounds or if it hasn't move returned false as it isn't a move
		if (newPos.x < 1 || newPos.x > 8 || newPos.y < 1 || newPos.y > 8 || (newPos.x == origPos.x && newPos.y == origPos.y))
		{
			Debug.Log("Out of bounds or not moved");
			return false;
		}


		// Check each piece type
		switch (tagName)
        {
			case "King":
				// If its a king make sure it moved a maximum of one square
				if(newPos.x >= (origPos.x + 2) || newPos.x <= (origPos.x - 2) || newPos.y >= (origPos.y + 2) || newPos.y <= (origPos.y - 2))
                {
					Debug.Log("Not valid king move");
					return false;
                }
				break;
			case "Queen":
				// If it is a queen make sure that if it moved in both x and y direction it was only diagonally
				if(newPos.y != origPos.y && newPos.x != origPos.x)
                {
					if( Math.Abs(newPos.x - origPos.x) != Math.Abs(newPos.y - origPos.y) )
                    {
						Debug.Log("Not valid queen move");
						return false;
					}
				}
				break;
			case "Bishop":
				// If it is a bishop make sure that it only moved diagonally
                if ( Math.Abs(newPos.x - origPos.x) != Math.Abs(newPos.y - origPos.y) )
                {
					Debug.Log("Not valid bishop move");
					return false;
                }
				break;
			case "Knight":
				// If it is a knight, see if it is any of the 8 possible moves it has to be
				if(newPos.x == origPos.x + 1)
                {
					if (newPos.y != origPos.y + 2 && newPos.y != origPos.y - 2)
					{
						Debug.Log("Not valid knight move");
						return false;
					}
				}
				else if(newPos.x == origPos.x + 2)
                {
					if (newPos.y != origPos.y - 1 && newPos.y != origPos.y + 1)
					{
						Debug.Log("Not valid knight move");
						return false;
					}
				}
				else if(newPos.x == origPos.x - 1)
                {
					if (newPos.y != origPos.y + 2 && newPos.y != origPos.y - 2)
					{
						Debug.Log("Not valid knight move");
						return false;
					}
				}
				else if(newPos.x == origPos.x - 2)
                {
					if (newPos.y != origPos.y - 1 && newPos.y != origPos.y + 1)
					{
						Debug.Log("Not valid knight move");
						return false;
					}
				}
                else
                {
					Debug.Log("Not valid knight move");
					return false;
                }
				break;
			case "Rook":
				// If it is a rook make sure it can only move on only one axis at a time
				if (newPos.x != origPos.x && newPos.y != origPos.y)
                {
					Debug.Log("Not valid rook move");
					return false;
				}
				break;
			case "Pawn":
				/* If it is a pawn, it is very complicated to see whether or not the move is legal
				 * pawns can only move up if they are a white piece or down if they are a black piece.
				 * They can also only move two squares if they are in their original board position.
				 * They can only take diagonally, and they can also en passent if another pawn has
				 * passed them on either side.
				 * Once they reach their respective end, they can promote to a queen, rook, knight, or bishop.
				 * If it was none of these moves, it is an illegal pawn move..
				 */
				if (this.gameObject.name.ToCharArray()[0] == 'B')
				{
					if (newPos.y >= origPos.y || newPos.y < origPos.y - 2)
                    {
						Debug.Log("Not valid pawn move");
						return false;
                    }
					if (newPos.y == origPos.y - 2)
					{
						if (origPos.y != 7)
						{
							Debug.Log("Not valid pawn move");
							return false;
						}
					}
					if (newPos.x != origPos.x)
					{
						if (newPos.y != origPos.y - 1)
                        {
							Debug.Log("Not valid pawn move");
							return false;
						}
						if (newPos.x > origPos.x + 1 || newPos.x < origPos.x - 1)
						{
							Debug.Log("Not valid pawn move");
							return false;
						}
					}
				}
				else
				{
					if (newPos.y <= origPos.y || newPos.y > origPos.y + 2)
					{
						Debug.Log("Not valid pawn move");
						return false;
					}
					if (newPos.y == origPos.y + 2)
					{
						if (origPos.y != 2)
						{
							Debug.Log("Not valid pawn move");
							return false;
						}
					}
					if (newPos.x != origPos.x)
					{
						if (newPos.y != origPos.y + 1)
						{
							Debug.Log("Not valid pawn move");
							return false;
						}
						if (newPos.x > origPos.x + 1 || newPos.x < origPos.x - 1)
						{
							Debug.Log("Not valid pawn move");
							return false;
						}
                    }
                }
				break;
		}

		// It is also illegal if the piece jumped any other piece, unless it was a knight move as knights can jump pieces
		if(tagName != "Knight")
        {
			if(newPos.x != origPos.x && newPos.y != origPos.y)
            {
				int x, y;
				x = (int)newPos.x;
				y = (int)newPos.y;
				if(newPos.x < origPos.x)
                {
					if(newPos.y < origPos.y)
                    {
						x++;
						y++;
						while(x != origPos.x && y != origPos.y)
                        {
							if (board[x - 1][y - 1] != null)
							{
								Debug.Log($"{board[x - 1][y - 1]} in way diagonal move at {x - 1} {y - 1}");
								return false;
							}
							x++;
							y++;
                        }
                    }
                    else
                    {
						x++;
						y--;
						while (x != origPos.x && y != origPos.y)
						{
							if (board[x - 1][y - 1] != null)
							{
								Debug.Log($"{board[x - 1][y - 1]} in way diagonal move at {x - 1} {y - 1}");
								return false;
							}
							x++;
							y--;
						}
					}
                }
                else
                {
					if (newPos.y < origPos.y)
					{
						x--;
						y++;
						while (x != origPos.x && y != origPos.y)
						{
							if (board[x - 1][y - 1] != null)
							{
								Debug.Log($"{board[x - 1][y - 1]} in way diagonal move at {x - 1} {y - 1}");
								return false;
							}
							x--;
							y++;
						}
					}
					else
					{
						x--;
						y--;
						while (x != origPos.x && y != origPos.y)
						{
							if (board[x - 1][y - 1] != null)
							{
								Debug.Log($"{board[x - 1][y - 1]} in way diagonal move at {x - 1} {y - 1}");
								return false;
							}
							x--;
							y--;
						}
					}
				}
            }
			else if(newPos.x != origPos.x)
            {
				if(newPos.x < origPos.x)
                {
					for(int i = (int)newPos.x+1; i < origPos.x; i++)
                    {
						if (board[i - 1][(int)origPos.y - 1] != null)
						{
							Debug.Log($"{board[i - 1][(int)origPos.y - 1]} in way sliding x move {i-1} {origPos.y - 1}");
							return false;
						}
                    }
                }
                else
                {
					for (int i = (int)origPos.x+1; i < newPos.x; i++)
					{
						if (board[i - 1][(int)origPos.y - 1] != null)
						{
							Debug.Log($"{board[i - 1][(int)origPos.y - 1]} in way sliding x move at {i - 1} {origPos.y - 1}");
							return false;
						}
					}
				}
            }
			else if(newPos.y != origPos.y)
            {
				if(newPos.y < origPos.y)
                {
					for(int i = (int)newPos.y+1; i < origPos.y; i++)
                    {
						if (board[(int)origPos.x - 1][i - 1] != null)
						{
							Debug.Log($"{board[(int)origPos.x - 1][i - 1]} in way sliding y move at {origPos.x-1} {i-1}");
							return false;
						}
                    }
                }
                else
                {
					for (int i = (int)origPos.y+1; i < newPos.y; i++)
					{
						if (board[(int)origPos.x - 1][i - 1] != null)
						{
							Debug.Log($"{board[(int)origPos.x - 1][i - 1]} in way sliding y move at {origPos.x-1} {i-1}");
							return false;
						}
					}
				}
            }
        }

		return true;

    }

}
