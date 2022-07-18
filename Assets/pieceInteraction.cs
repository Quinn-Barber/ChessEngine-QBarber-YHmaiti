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

	// Variables for piece movement and board set-up (whether a piece is held and where it originally was)
	bool pieceCaptured = false;
	bool beingHeld = false;
    Vector2 origPos;
	Vector2 rookMove;
	Vector2 castleArr;

	// Variable for castling
	bool castle = false;
	bool enPassent = false;
	bool promotion = false;
	bool inCheck = false;
	int checkAtX, checkAtY;
	String checkBy;

	// Start is called before the first frame update 
	void Start()
    {
		// Find the Camera's GameObject from its tag
		PlayerCam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
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
		pieceCaptured = false;
		castle = false;
		enPassent = false;
		promotion = false;
		inCheck = false;

		for (int i = 0; i < 16; i++)
        {
            if (gameManager.changeDoubleUp[i] == false)
            {
				if (i <= 7)
				{
					gameManager.doubleUpBlack[i] = false;
				}
				else
				{
					gameManager.doubleUpWhite[i - 8] = false;
				}
            }
            else
            {
				gameManager.changeDoubleUp[i] = false;
			}
        }
		// The potential new position of the piece is the current position of the mouse snapped to the nearest square
		Vector2 newPos = new Vector2((float)Math.Round(this.gameObject.transform.position.x), (float)Math.Round(this.gameObject.transform.position.y));

        // Check if this move is made by the correct color according to the turn order
        if ( (gameManager.turnOrder && this.gameObject.name.ToCharArray()[0] != 'W') || (!gameManager.turnOrder && this.gameObject.name.ToCharArray()[0] != 'B') )
        {
			this.gameObject.transform.position = origPos;
			this.gameObject.GetComponent<Collider>().enabled = true;
			return;
		}

		inCheck = kingUnderAttack();
		if(inCheck == true)
        {
			Debug.Log("In check by " + checkBy);
			/*String debuglog = "";
			for(int i = 7; i >= 0; i--)
            {
				for(int j = 0; j < 8; j++)
                {
					if(gameManager.board[j][i] == null)
                    {
						debuglog += ("None" + "\t");
					}
                    else
                    {
						debuglog += (gameManager.board[j][i].tag + "\t");
					}
                }
				debuglog += ("\n");
            }
			Debug.Log(debuglog);*/
		}
		// Check if this move is legal, if it is not reenable this pieces collision and set it back to its original position, return.
		if ( !isLegalMove(newPos) )
        {
			this.gameObject.transform.position = origPos;
			this.gameObject.GetComponent<Collider>().enabled = true;
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
                if (!checkChecks((int)(origPos.x - 1), (int)(origPos.y - 1), (int)(newPos.x - 1), (int)(newPos.y - 1), true))
                {
					this.gameObject.transform.position = origPos;
					return;
                }
				hitInfo.collider.gameObject.SetActive(false);
				pieceCaptured = true;
				this.gameObject.transform.position = newPos;
			}
			else
			{
				// If they are the same color it is an illegal move, set it back to its original position
				this.gameObject.transform.position = origPos;
				return;
			}
		}

		if (!checkChecks((int)(origPos.x - 1), (int)(origPos.y - 1), (int)(newPos.x - 1), (int)(newPos.y - 1), pieceCaptured))
		{
			this.gameObject.GetComponent<Collider>().enabled = true;
			this.gameObject.transform.position = origPos;
			return;
		}

		// If it is legal and is a castle implement the following method
		if (castle)
        {
			// MOVE KING AND ROOK TO CORRESPONDING SPOT
			this.gameObject.GetComponent<Collider>().enabled = true;
			this.gameObject.transform.position = newPos;
			gameManager.board[(int)castleArr.x - 1][(int)castleArr.y - 1].transform.position = rookMove;

			// INDICATE PIECE HAS MOVEN
			gameManager.pieceMoved[(int)origPos.x - 1][(int)origPos.y - 1] = true;
			gameManager.pieceMoved[(int)castleArr.x - 1][(int)castleArr.y - 1] = true;


			// UPDATE KING TO CORRECT SPOT ON BOARD
			GameObject temp = gameManager.board[(int)origPos.x - 1][(int)origPos.y - 1];
			gameManager.board[(int)origPos.x - 1][(int)origPos.y - 1] = gameManager.board[(int)newPos.x - 1][(int)newPos.y - 1];
			gameManager.board[(int)newPos.x - 1][(int)newPos.y - 1] = temp;


			// UPDATE ROOK TO CORRECT SPOT ON BOARD
			gameManager.board[(int)rookMove.x - 1][(int)rookMove.y - 1] = gameManager.board[(int)castleArr.x - 1][(int)castleArr.y - 1];
			gameManager.board[(int)castleArr.x - 1][(int)castleArr.y - 1] = null;

			// UPDATE TURN ORDER
			gameManager.turnOrder = !gameManager.turnOrder;

			return;
		}
		// If it is legal and is an en Passent implement the following method
        if (enPassent)
        {
			if(this.gameObject.name.ToCharArray()[0] == 'B')
            {
				gameManager.board[(int)newPos.x - 1][(int)newPos.y].SetActive(false);
				gameManager.board[(int)newPos.x - 1][(int)newPos.y] = null;
				this.gameObject.GetComponent<Collider>().enabled = true;
				this.gameObject.transform.position = newPos;

				GameObject temp = gameManager.board[(int)origPos.x - 1][(int)origPos.y - 1];
				gameManager.board[(int)origPos.x - 1][(int)origPos.y - 1] = null;
				gameManager.board[(int)newPos.x - 1][(int)newPos.y - 1] = temp;
			}
            else
            {
				gameManager.board[(int)newPos.x - 1][(int)newPos.y - 2].SetActive(false);
				gameManager.board[(int)newPos.x - 1][(int)newPos.y - 2] = null;
				this.gameObject.GetComponent<Collider>().enabled = true;
				this.gameObject.transform.position = newPos;

				GameObject temp = gameManager.board[(int)origPos.x - 1][(int)origPos.y - 1];
				gameManager.board[(int)origPos.x - 1][(int)origPos.y - 1] = null;
				gameManager.board[(int)newPos.x - 1][(int)newPos.y - 1] = temp;
			}

			gameManager.turnOrder = !gameManager.turnOrder;
			return;
        }
		// If it is legal and is a promotion implement the following method
		if(promotion)
        {
			GameObject replace;
			if (gameManager.turnOrder == true)
            {
				replace = GameObject.Instantiate(gameManager.whiteQueen);
			}
            else
            {
				replace = GameObject.Instantiate(gameManager.blackQueen);
			}

			gameManager.board[(int)origPos.x - 1][(int)origPos.y - 1] = null;
			gameManager.board[(int)newPos.x - 1][(int)newPos.y - 1] = replace;
			this.gameObject.SetActive(false);
			replace.transform.position = newPos;

			gameManager.turnOrder = !gameManager.turnOrder;
			return;
		}

		// If it is legal and hasn't collided with anything, update the position and board as necessary (turn back on the collision as well)
		this.gameObject.GetComponent<Collider>().enabled = true;
		this.gameObject.transform.position = newPos;
		
		// INDICATE PIECE HAS MOVEN
		gameManager.pieceMoved[(int)origPos.x - 1][(int)origPos.y - 1] = true;

		// UPDATE THE BOARD TO BE CORRECT
		GameObject tmp = gameManager.board[(int)origPos.x - 1][(int)origPos.y - 1];
		gameManager.board[(int)origPos.x - 1][(int)origPos.y - 1] = gameManager.board[(int)newPos.x - 1][(int)newPos.y - 1];
		gameManager.board[(int)newPos.x - 1][(int)newPos.y - 1] = tmp;
        if (pieceCaptured)
        {
			gameManager.board[(int)origPos.x - 1][(int)origPos.y - 1] = null;
		}

		// UPDATE TURN ORDER
		gameManager.turnOrder = !gameManager.turnOrder;

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
				if(newPos.x >= (origPos.x + 3) || newPos.x <= (origPos.x - 3) || newPos.y >= (origPos.y + 2) || newPos.y <= (origPos.y - 2))
                {
					Debug.Log("Not valid king move");
					return false;
                }
				else if(newPos.x == origPos.x + 2 || newPos.x == origPos.x - 2)
                {
					if ( !Castling(newPos) )
                    {
						Debug.Log("Not valid king move");
						return false;
					}
                    else
                    {
						castle = true;
                    }
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
				if( !PawnMovement(newPos))
                {
					return false;
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
							if (gameManager.board[x - 1][y - 1] != null)
							{
								Debug.Log($"{gameManager.board[x - 1][y - 1]} in way diagonal move at {x - 1} {y - 1}");
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
							if (gameManager.board[x - 1][y - 1] != null)
							{
								Debug.Log($"{gameManager.board[x - 1][y - 1]} in way diagonal move at {x - 1} {y - 1}");
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
							if (gameManager.board[x - 1][y - 1] != null)
							{
								Debug.Log($"{gameManager.board[x - 1][y - 1]} in way diagonal move at {x - 1} {y - 1}");
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
							if (gameManager.board[x - 1][y - 1] != null)
							{
								Debug.Log($"{gameManager.board[x - 1][y - 1]} in way diagonal move at {x - 1} {y - 1}");
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
						if (gameManager.board[i - 1][(int)origPos.y - 1] != null)
						{
							Debug.Log($"{gameManager.board[i - 1][(int)origPos.y - 1]} in way sliding x move {i-1} {origPos.y - 1}");
							return false;
						}
                    }
                }
                else
                {
					for (int i = (int)origPos.x+1; i < newPos.x; i++)
					{
						if (gameManager.board[i - 1][(int)origPos.y - 1] != null)
						{
							Debug.Log($"{gameManager.board[i - 1][(int)origPos.y - 1]} in way sliding x move at {i - 1} {origPos.y - 1}");
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
						if (gameManager.board[(int)origPos.x - 1][i - 1] != null)
						{
							Debug.Log($"{gameManager.board[(int)origPos.x - 1][i - 1]} in way sliding y move at {origPos.x-1} {i-1}");
							return false;
						}
                    }
                }
                else
                {
					for (int i = (int)origPos.y+1; i < newPos.y; i++)
					{
						if (gameManager.board[(int)origPos.x - 1][i - 1] != null)
						{
							Debug.Log($"{gameManager.board[(int)origPos.x - 1][i - 1]} in way sliding y move at {origPos.x-1} {i-1}");
							return false;
						}
					}
				}
            }
        }

		return true;

    }

	private bool Castling(Vector2 newPos)
    {
        if (this.gameObject.name.ToCharArray()[0] == 'W')
        {
			if(newPos.x == origPos.x + 2)
            {
				// If it is the wrong piece, or either piece has moven it is false
				if(gameManager.board[(int)origPos.x - 1 + 4][(int)origPos.y - 1].name != "WhiteRookRight" || gameManager.pieceMoved[(int)origPos.x - 1][(int)origPos.y - 1] == true || gameManager.pieceMoved[(int)origPos.x - 1 + 4][(int)origPos.y - 1] == true)
                {
					return false;
                }
				// Otherwise update the vectors for which to access the rook from the board in the mouseup function
				castleArr.x = origPos.x + 4;
				castleArr.y = origPos.y;
				rookMove = newPos;
				rookMove.x -= 1;
            }
            else
            {
				// If it is the wrong piece, or either piece has moven it is false
				if (gameManager.board[(int)origPos.x - 1 - 3][(int)origPos.y - 1].name != "WhiteRookLeft" || gameManager.pieceMoved[(int)origPos.x - 1][(int)origPos.y - 1] == true || gameManager.pieceMoved[(int)origPos.x - 1 - 3][(int)origPos.y - 1] == true)
				{
					return false;
				}
				// Otherwise update the vectors for which to access the rook from the board in the mouseup function
				castleArr.x = origPos.x - 3;
				castleArr.y = origPos.y;
				rookMove = newPos;
				rookMove.x += 1;
			}
        }
        else
        {
			if (newPos.x == origPos.x + 2)
			{
				// If it is the wrong piece, or either piece has moven it is false
				if (gameManager.board[(int)origPos.x - 1 + 4][(int)origPos.y - 1].name != "BlackRookRight" || gameManager.pieceMoved[(int)origPos.x - 1][(int)origPos.y - 1] == true || gameManager.pieceMoved[(int)origPos.x - 1 + 4][(int)origPos.y - 1] == true)
				{
					return false;
				}
				// Otherwise update the vectors for which to access the rook from the board in the mouseup function
				castleArr.x = origPos.x + 4;
				castleArr.y = origPos.y;
				rookMove = newPos;
				rookMove.x -= 1;
			}
			else
			{
				// If it is the wrong piece, or either piece has moven it is false
				if (gameManager.board[(int)origPos.x - 1 - 3][(int)origPos.y - 1].name != "BlackRookLeft" || gameManager.pieceMoved[(int)origPos.x - 1][(int)origPos.y - 1] == true || gameManager.pieceMoved[(int)origPos.x - 1 - 3][(int)origPos.y - 1] == true)
				{
					return false;
				}
				// Otherwise update the vectors for which to access the rook from the board in the mouseup function
				castleArr.x = origPos.x - 3;
				castleArr.y = origPos.y;
				rookMove = newPos;
				rookMove.x += 1;
			}
		}
		return true;
    }

	private bool PawnMovement(Vector2 newPos)
    {
		if (this.gameObject.name.ToCharArray()[0] == 'B')
		{
			if (newPos.y >= origPos.y || newPos.y < origPos.y - 2)
			{
				Debug.Log("Not valid pawn move");
				return false;
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
				if (gameManager.board[(int)newPos.x - 1][(int)newPos.y - 1] == null)
				{
                    if (gameManager.doubleUpWhite[(int)newPos.x-1] == true && newPos.y == 3)
                    {
						enPassent = true;
                    }
                    else
                    {
						Debug.Log("Not valid pawn move");
						return false;
					}
				}
			}
			if (newPos.y == origPos.y - 2)
			{
				if (origPos.y != 7)
				{
					Debug.Log("Not valid pawn move");
					return false;
				}
				if (gameManager.board[(int)newPos.x - 1][(int)newPos.y - 1] != null || gameManager.board[(int)newPos.x - 1][(int)newPos.y] != null)
				{
					Debug.Log("Not valid pawn move");
					return false;
				}
                gameManager.doubleUpBlack[(int)origPos.x - 1] = true;
				gameManager.changeDoubleUp[(int)origPos.x - 1] = true;
			}
			if (newPos.x == origPos.x && gameManager.board[(int)newPos.x - 1][(int)newPos.y - 1] != null)
			{
				Debug.Log("Not valid pawn move");
				return false;
			}
			if( newPos.y == 1)
            {
				promotion = true;
            }
		}
		else
		{
			if (newPos.y <= origPos.y || newPos.y > origPos.y + 2)
			{
				Debug.Log("Not valid pawn move");
				return false;
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
				if (gameManager.board[(int)newPos.x - 1][(int)newPos.y - 1] == null)
				{
					if (gameManager.doubleUpBlack[(int)newPos.x - 1] == true && newPos.y == 6)
					{
						enPassent = true;
					}
					else
					{
						Debug.Log("Not valid pawn move here");
						return false;
					}
				}
			}
			if (newPos.y == origPos.y + 2)
			{
				if (origPos.y != 2)
				{
					Debug.Log("Not valid pawn move");
					return false;
				}
				if (gameManager.board[(int)newPos.x - 1][(int)newPos.y - 1] != null || gameManager.board[(int)newPos.x - 1][(int)newPos.y - 2] != null)
				{
					Debug.Log("Not valid pawn move");
					return false;
				}
                gameManager.doubleUpWhite[(int) origPos.x - 1] = true;
				gameManager.changeDoubleUp[(int)origPos.x - 1 + 8] = true;
			}
			if (newPos.x == origPos.x && gameManager.board[(int)newPos.x - 1][(int)newPos.y - 1] != null)
			{
				Debug.Log("Not valid pawn move");
				return false;
			}
			if( newPos.y == 8)
            {
				promotion = true;
            }
		}
		return true;
    }

	private bool kingUnderAttack()
    {
		int posX, posY;
		posX = posY = -1;
		bool breakLoop = false;
		for(int i = 0; i < 8; i++)
        {
			for(int j = 0; j < 8; j++)
            {
				if(gameManager.board[(int)i][j] != null && gameManager.board[(int)i][j].tag == "King" && gameManager.board[(int)i][j].name.ToCharArray()[0] == this.gameObject.name.ToCharArray()[0])
                {
					posX = i;
					posY = j;
					breakLoop = true;
					break;
                }
            }
			if(breakLoop == true)
            {
				break;
            }
        }
		if(posX == -1 || posY == -1)
        {
			return false;
        }

		bool breakOne, breakTwo, breakThree, breakFour, breakFive, breakSix;
		breakOne = breakTwo = breakThree = breakFour = breakFive = breakSix = false;

		for(int i = 0; i < 8; i++)
        {
			if(breakOne == false)
            {
                if(inBounds(i, posY))
                {
					if (gameManager.board[i][posY] != null && (gameManager.board[i][posY].tag == "Rook" || gameManager.board[i][posY].tag == "Queen"))
					{
						if (gameManager.board[i][posY].name.ToCharArray()[0] != this.gameObject.name.ToCharArray()[0])
						{
							if(i < posX)
                            {
								for(int j = 1; j < posX - i; j++)
                                {
									if(gameManager.board[i + j][posY] != null)
                                    {
										breakOne = true;
										break;
                                    }
                                }
                            }
                            else
                            {
								for(int j = 1; j < i - posX; j++)
								{
									if(gameManager.board[posX + j][posY] != null)
									{
										breakOne = true;
										break;
									}
								}
							}

                            if (!breakOne)
                            {
								checkAtX = i;
								checkAtY = posY;
								checkBy = gameManager.board[i][posY].name;
								Debug.Log("TRUE AT " + i + " " + posY);
								return true;
							}
						}
					}
				}
			}
			if(breakTwo == false)
            {
                if(inBounds(posX, i))
                {
					if (gameManager.board[posX][i] != null && (gameManager.board[posX][i].tag == "Rook" || gameManager.board[posX][i].tag == "Queen"))
					{
						if (gameManager.board[posX][i].name.ToCharArray()[0] != this.gameObject.name.ToCharArray()[0])
						{
							if (i < posY)
							{
								for (int j = 1; j < posY - i; j++)
								{
									if (gameManager.board[posX][i + j] != null)
									{
										breakTwo = true;
										break;
									}
								}
							}
							else
							{
								for (int j = 1; j < i - posY; j++)
								{
									if (gameManager.board[posX][posY + j] != null)
									{
										breakTwo = true;
										break;
									}
								}
							}

							if (!breakTwo)
                            {
								checkAtX = posX;
								checkAtY = i;
								checkBy = gameManager.board[posX][i].name;
								Debug.Log("TRUE AT " + posX + " " + i);
								return true;
							}
						}
					}
				}
			}
			if(breakThree == false)
            {
				if (inBounds(posX - i, posY - i))
				{
					if (gameManager.board[posX - i][posY - i] != null && (gameManager.board[posX - i][posY - i].tag == "Bishop" || gameManager.board[posX - i][posY - i].tag == "Queen"))
					{
						if (gameManager.board[posX - i][posY - i].name.ToCharArray()[0] != this.gameObject.name.ToCharArray()[0])
						{
							for(int j = 1; (posY - i + j) < posY; j++)
                            {
                                if (gameManager.board[posX - i + j][posY - i + j] != null)
                                {
									breakThree = true;
                                }
                            }

							if (!breakThree)
                            {
								checkAtX = posX - i;
								checkAtY = posY - i;
								checkBy = gameManager.board[posX - i][posY - i].name;
								Debug.Log("TRUE AT " + (posX - i) + " " + (posY - i));
								return true;
							}
						}
					}
				}
			}
			if(breakFour == false)
            {
				if (inBounds(posX + i, posY + i))
				{
					if (gameManager.board[posX + i][posY + i] != null && (gameManager.board[posX + i][posY + i].tag == "Bishop" || gameManager.board[posX + i][posY + i].tag == "Queen"))
					{
						if (gameManager.board[posX + i][posY + i].name.ToCharArray()[0] != this.gameObject.name.ToCharArray()[0])
						{
							for (int j = 1; (posY + i - j) > posY; j++)
							{
								if (gameManager.board[posX + i - j][posY + i - j] != null)
								{
									breakFour = true;
								}
							}

							if (!breakFour)
                            {
								checkAtX = posX + i;
								checkAtY = posY + i;
								checkBy = gameManager.board[posX + i][posY + i].name;
								Debug.Log("TRUE AT " + (posX + i) + " " + (posY + i));
								return true;
							}
						}
					}
				}
			}
			if(breakFive == false)
            {
				if (inBounds(posX - i, posY + i))
				{
					if (gameManager.board[posX - i][posY + i] != null && (gameManager.board[posX - i][posY + i].tag == "Bishop" || gameManager.board[posX - i][posY + i].tag == "Queen"))
					{
						if (gameManager.board[posX - i][posY + i].name.ToCharArray()[0] != this.gameObject.name.ToCharArray()[0])
						{
							for (int j = 1; (posY + i - j) > posY; j++)
							{
								if (gameManager.board[posX - i + j][posY + i - j] != null)
								{
									breakFive = true;
								}
							}

							if (!breakFive)
							{
								checkAtX = posX - i;
								checkAtY = posY + i;
								checkBy = gameManager.board[posX - i][posY + i].name;
								Debug.Log("TRUE AT " + (posX - i) + " " + (posY + i));
								return true;
							}
						}
					}
				}
			}
			if(breakSix == false)
            {
				if (inBounds(posX + i, posY - i))
				{
					if (gameManager.board[posX + i][posY - i] != null && (gameManager.board[posX + i][posY - i].tag == "Bishop" || gameManager.board[posX + i][posY - i].tag == "Queen"))
					{
						if (gameManager.board[posX + i][posY - i].name.ToCharArray()[0] != this.gameObject.name.ToCharArray()[0])
						{
							for (int j = 1; (posY - i + j) < posY; j++)
							{
								if (gameManager.board[posX + i - j][posY - i + j] != null)
								{
									breakSix = true;
								}
							}

							if (!breakSix)
							{
								checkAtX = posX + i;
								checkAtY = posY - i;
								checkBy = gameManager.board[posX + i][posY - i].name;
								Debug.Log("TRUE AT " + (posX + i) + " " + (posY - i));
								return true;
							}
						}
					}
				}
			}
        }

        if ( inBounds(posX + 1, posY - 2) && gameManager.board[posX + 1][posY - 2] != null && gameManager.board[posX + 1][posY - 2].tag == "Knight" && gameManager.board[posX + 1][posY - 2].name.ToCharArray()[0] != this.gameObject.name.ToCharArray()[0])
        {
			checkAtX = posX + 1;
			checkAtY = posY - 2;
			checkBy = gameManager.board[posX + 1][posY - 2].name;
			Debug.Log("TRUE AT " + (posX + 1) + " " + (posY - 2));
			return true;
        }
		else if (inBounds(posX - 1, posY - 2) && gameManager.board[posX - 1][posY - 2] != null && gameManager.board[posX - 1][posY - 2].tag == "Knight" && gameManager.board[posX - 1][posY - 2].name.ToCharArray()[0] != this.gameObject.name.ToCharArray()[0])
        {
			checkAtX = posX - 1;
			checkAtY = posY - 2;
			checkBy = gameManager.board[posX - 1][posY - 2].name;
			Debug.Log("TRUE AT " + (posX - 1) + " " + (posY - 2));
			return true;
        }
		else if (inBounds(posX + 2, posY - 1) && gameManager.board[posX + 2][posY - 1] != null && gameManager.board[posX + 2][posY - 1].tag == "Knight" && gameManager.board[posX + 2][posY - 1].name.ToCharArray()[0] != this.gameObject.name.ToCharArray()[0])
		{
			checkAtX = posX + 2;
			checkAtY = posY - 1;
			Debug.Log("TRUE AT " + (posX + 2) + " " + (posY - 1));
			checkBy = gameManager.board[posX + 2][posY - 1].name;
			return true;
		}
		else if (inBounds(posX - 2, posY - 1) && gameManager.board[posX - 2][posY - 1] != null && gameManager.board[posX - 2][posY - 1].tag == "Knight" && gameManager.board[posX - 2][posY - 1].name.ToCharArray()[0] != this.gameObject.name.ToCharArray()[0])
		{
			checkAtX = posX - 2;
			checkAtY = posY - 1;
			Debug.Log("TRUE AT " + (posX - 2) + " " + (posY - 1));
			checkBy = gameManager.board[posX - 2][posY - 1].name;
			return true;
		}
		else if (inBounds(posX + 1, posY + 2) && gameManager.board[posX + 1][posY + 2] != null && gameManager.board[posX + 1][posY + 2].tag == "Knight" && gameManager.board[posX + 1][posY + 2].name.ToCharArray()[0] != this.gameObject.name.ToCharArray()[0])
		{
			checkAtX = posX + 1;
			checkAtY = posY + 2;
			Debug.Log("TRUE AT " + (posX + 1) + " " + (posY + 2));
			checkBy = gameManager.board[posX + 1][posY + 2].name;
			return true;
		}
		else if (inBounds(posX + 2, posY + 1) && gameManager.board[posX + 2][posY + 1] != null && gameManager.board[posX + 2][posY + 1].tag == "Knight" && gameManager.board[posX + 2][posY + 1].name.ToCharArray()[0] != this.gameObject.name.ToCharArray()[0])
		{
			checkAtX = posX + 2;
			checkAtY = posY + 1;
			Debug.Log("TRUE AT " + (posX + 2) + " " + (posY + 1));
			Debug.Log(posX + " " + posY);
			checkBy = gameManager.board[posX + 2][posY + 1].name;
			return true;
		}
		else if (inBounds(posX - 1, posY + 2) && gameManager.board[posX - 1][posY + 2] != null && gameManager.board[posX - 1][posY + 2].tag == "Knight" && gameManager.board[posX - 1][posY + 2].name.ToCharArray()[0] != this.gameObject.name.ToCharArray()[0])
		{
			checkAtX = posX - 1;
			checkAtY = posY + 2;
			Debug.Log("TRUE AT " + (posX - 1) + " " + (posY + 2));
			checkBy = gameManager.board[posX - 1][posY + 2].name;
			return true;
		}
		else if (inBounds(posX - 2, posY + 1) && gameManager.board[posX - 2][posY + 1] != null && gameManager.board[posX - 2][posY + 1].tag == "Knight" && gameManager.board[posX - 2][posY + 1].name.ToCharArray()[0] != this.gameObject.name.ToCharArray()[0])
		{
			checkAtX = posX - 2;
			checkAtY = posY + 1;
			Debug.Log("TRUE AT " + (posX - 2) + " " + (posY + 1));
			checkBy = gameManager.board[posX - 2][posY + 1].name;
			return true;
		}

		if( this.gameObject.name.ToCharArray()[0] == 'B')
        {
			if (inBounds(posX - 1, posY - 1))
			{
                if (gameManager.board[posX - 1][posY - 1] != null && gameManager.board[posX - 1][posY - 1].tag == "Pawn" && gameManager.board[posX - 1][posY - 1].name.ToCharArray()[0] != this.gameObject.name.ToCharArray()[0])
                {
					return true;
				}
			}

			if (inBounds(posX + 1, posY - 1))
			{
				if (gameManager.board[posX + 1][posY - 1] != null && gameManager.board[posX + 1][posY - 1].tag == "Pawn" && gameManager.board[posX + 1][posY - 1].name.ToCharArray()[0] != this.gameObject.name.ToCharArray()[0])
				{
					return true;
				}
			}
		}
        else
        {
			if (inBounds(posX + 1, posY + 1))
			{
				if(gameManager.board[posX + 1][posY + 1] != null && gameManager.board[posX + 1][posY + 1].tag == "Pawn" && gameManager.board[posX + 1][posY + 1].name.ToCharArray()[0] != this.gameObject.name.ToCharArray()[0])
                {
					return true;
				}
			}

			if (inBounds(posX - 1, posY + 1))
			{
				if(gameManager.board[posX - 1][posY + 1] != null && gameManager.board[posX - 1][posY + 1].tag == "Pawn" && gameManager.board[posX - 1][posY + 1].name.ToCharArray()[0] != this.gameObject.name.ToCharArray()[0])
                {
					return true;
				}
			}
		}

		return false;
    }

	private bool checkChecks(int oldPosX, int oldPosY, int newPosX, int newPosY, bool isCaptured)
    {
		GameObject oldObj = gameManager.board[oldPosX][oldPosY];
		GameObject newObj = gameManager.board[newPosX][newPosY];
		if (isCaptured)
        {
			gameManager.board[oldPosX][oldPosY] = null;
			gameManager.board[newPosX][newPosY] = oldObj;
            if (kingUnderAttack())
            {
				gameManager.board[oldPosX][oldPosY] = oldObj;
				gameManager.board[newPosX][newPosY] = newObj;
				return false;
            }
		}
        else
        {
			gameManager.board[oldPosX][oldPosY] = newObj;
			gameManager.board[newPosX][newPosY] = oldObj;
			if (kingUnderAttack())
			{
				gameManager.board[oldPosX][oldPosY] = oldObj;
				gameManager.board[newPosX][newPosY] = newObj;
				return false;
			}
		}

		gameManager.board[oldPosX][oldPosY] = oldObj;
		gameManager.board[newPosX][newPosY] = newObj;
		return true;
    }

	private bool inBounds(int x, int y)
    {
		if (x < 0 || x > 7 || y < 0 || y > 7)
        {
			return false;
        }

		return true;
    }

}
