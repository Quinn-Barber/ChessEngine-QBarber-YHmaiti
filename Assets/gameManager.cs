using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public class gameManager : MonoBehaviour
{
	// Camera used by the player
	private Camera PlayerCam;

	// Which player is AI and turn order
	public bool _player1AI;
	public bool _player2AI;
	public static bool turnOrder;

	// 2D Array for the pieces on the chess board
	public static GameObject[][] board;
	public static bool[][] pieceMoved;
	public static bool[] doubleUpBlack;
	public static bool[] doubleUpWhite;
	public static bool[] changeDoubleUp;

	// Start is called before the first frame update
	void Start()
    {
		// Find the Camera's GameObject from its tag
		PlayerCam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();

		// Initialize the chess board and turn order (true is whites move false is blacks move)
		pieceMoved = new bool[8][];
		board = new GameObject[8][];
		doubleUpBlack = new bool[8];
		doubleUpWhite = new bool[8];
		changeDoubleUp = new bool[16];

		turnOrder = true;

		// Fill the chess board with each starting piece (using collisions based on rays from certain positions)
		for (int i = 0; i < 8; i++)
		{
			doubleUpWhite[i] = false;
			doubleUpBlack[i] = false;
			pieceMoved[i] = new bool[8];			
			board[i] = new GameObject[8];
			for (int j = 0; j < 8; j++)
			{
				pieceMoved[i][j] = false;
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

    // Update is called once per frame
    void Update()
    {
        
    }
}
