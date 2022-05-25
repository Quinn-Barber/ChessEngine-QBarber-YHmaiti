//Update SlectedPiece with the GameObject inputted to this function
	public void SelectPiece(GameObject _PieceToSelect)
	{
		// Unselect the piece if it was already selected
		if(_PieceToSelect  == SelectedPiece)
		{
			SelectedPiece.renderer.material.color = Color.white;
			SelectedPiece = null;
			ChangeState (0);
		}
		else
		{
			// Change color of the selected piece to make it apparent. Put it back to white when the piece is unselected
			if(SelectedPiece)
			{
				SelectedPiece.renderer.material.color = Color.white;
			}
			SelectedPiece = _PieceToSelect;
			SelectedPiece.renderer.material.color = Color.blue;
			ChangeState (1);
		}
	}
	
	// Move the SelectedPiece to the inputted coords
	public void MovePiece(Vector2 _coordToMove)
	{
		bool validMovementBool = false;
		Vector2 _coordPiece = new Vector2(SelectedPiece.transform.position.x, SelectedPiece.transform.position.z);
		
		// Don't move if the user clicked on its own cube or if there is a piece on the cube
		if((_coordToMove.x != _coordPiece.x || _coordToMove.y != _coordPiece.y) || _boardPieces[(int)_coordToMove.x,(int)_coordToMove.y] != 0)
		{
			validMovementBool	= TestMovement (SelectedPiece, _coordToMove);
		}
		
		if(validMovementBool)
		{
			_boardPieces[(int)_coordToMove.x, (int)_coordToMove.y] = _boardPieces[(int)_coordPiece.x, (int)_coordPiece.y];
			_boardPieces[(int)_coordPiece.x , (int)_coordPiece.y ] = 0;
			
			SelectedPiece.transform.position = new Vector3(_coordToMove.x, _pieceHeight, _coordToMove.y);		// Move the piece
			SelectedPiece.renderer.material.color = Color.white;	// Change it's color back
			SelectedPiece = null;									// Unselect the Piece
			ChangeState (0);
			activePlayer = -activePlayer;
		}
	}
	
	// If the movement is legal, eat the piece
	public void EatPiece(GameObject _PieceToEat)
	{
		Vector2 _coordToEat = new Vector2(_PieceToEat.transform.position.x, _PieceToEat.transform.position.z);
		int _deltaX = (int)(_PieceToEat.transform.position.x - SelectedPiece.transform.position.x);
		
		if(TestMovement (SelectedPiece, _coordToEat) && (SelectedPiece.name != "Pawn" ||  _deltaX != 0))
		{
			Object.Destroy (_PieceToEat);
			_boardPieces[(int)_coordToEat.x, (int)_coordToEat.y] = 0; 
			MovePiece(_coordToEat);
		}
	}
	
	// Test if the piece can do the player's movement
	bool TestMovement(GameObject _SelectedPiece, Vector2 _coordToMove)
	{
		bool _movementLegalBool = false;
		bool _collisionDetectBool = false;
		Vector2 _coordPiece = new Vector2(_SelectedPiece.transform.localPosition.x, _SelectedPiece.transform.localPosition.z);
		
		int _deltaX = (int)(_coordToMove.x - _coordPiece.x);
		int _deltaY = (int)(_coordToMove.y - _coordPiece.y);
		int activePlayerPawnPostion = 1;
		//Debug.Log("Piece (" + _coordPiece.x + "," + _coordPiece.x + ") - Move (" + _coordToMove.x + "," + _coordToMove.y + ")");
		//Debug.Log("Delta (" + _deltaX + "," + _deltaY + ")");
		// Use the name of the _SelectedPiece GameObject to find the piece used
		switch (_SelectedPiece.name)
		{
			
		    case "Pawn": 
				if(activePlayer == -1)
					activePlayerPawnPostion = 6;		
			
			
				// Pawn can move 1 steap ahead of them, 2 if they are on their initial position
		        if(_deltaY == activePlayer || (_deltaY == 2*activePlayer && _coordPiece.y == activePlayerPawnPostion))
				{
				//Debug.Log ("_deltaY");
					_movementLegalBool = true;
				}
				
		        break;
		    case "Rook":
				// Rook can move horizontally or vertically
				if((_deltaX != 0 && _deltaY == 0) || (_deltaX == 0 && _deltaY != 0)) 
				{
					_movementLegalBool = true;
				}
		        break;
			case "Knight":
				// Knight move in a L movement. distance is evaluated by a multiplication of both direction
				if((_deltaX != 0 && _deltaY != 0) && Mathf.Abs(_deltaX*_deltaY) == 2)
				{
					_movementLegalBool = true;
				}
		        break;
			case "Bishop":
				// Bishop can only move diagonally
				if(Mathf.Abs(_deltaX/_deltaY) == 1)
				{
					_movementLegalBool = true;
				}
		        break;
			
			case "Queen":
				// Queen movement is a combination of Rook and bishop
				if(((_deltaX != 0 && _deltaY == 0) || (_deltaX == 0 && _deltaY != 0)) || Mathf.Abs(_deltaX/_deltaY) == 1)
				{
					_movementLegalBool = true;
				}
		        break;
			
			case "King":
				// Bishop can only move diagonally
				if(Mathf.Abs(_deltaX + _deltaY) == 1) 
				{
					_movementLegalBool = true;
				}
		        break;
			
		    default:
		        _movementLegalBool = false;
		        break;
		}
		
		// If the movement is legal, detect collision with piece in the way. Don't do it with knight since they can pass over pieces.
		if(_movementLegalBool && SelectedPiece.name != "Knight")
		{
			_collisionDetectBool = TestCollision (_coordPiece, _coordToMove);
		}
			
		return (_movementLegalBool && !_collisionDetectBool);
	}
	
	// Test if a unit is in the path of the tested movement
	bool TestCollision(Vector2 _coordInitial,Vector2 _coordFinal)
	{
		bool CollisionBool = false;
		int _deltaX = (int)(_coordFinal.x - _coordInitial.x);
		int _deltaY = (int)(_coordFinal.y - _coordInitial.y);
		int _incX = 0; // Direction of the incrementation in X
		int _incY = 0; // Direction of the incrementation in Y
		int i;
		int j;
		
		// Calculate the increment if _deltaX/Y is different from 0 to avoid division by 0
		if(_deltaX != 0)
		{
			_incX = (_deltaX/Mathf.Abs(_deltaX));
		}
		if(_deltaY != 0)
		{
			_incY = (_deltaY/Mathf.Abs(_deltaY));
		}
		
		i = (int)_coordInitial.x + _incX;
		j = (int)_coordInitial.y + _incY;
		
		while(new Vector2(i, j) != _coordFinal)
		{
					
			if(_boardPieces[i,j] != 0)
			{
				CollisionBool = true;
				break;
			}
			
			i += _incX;
			j += _incY;
		}
		Debug.Log (CollisionBool);
		return CollisionBool;
	}

	// Change the state of the game
	public void ChangeState(int _newState)
	{
		gameState = _newState;
	}