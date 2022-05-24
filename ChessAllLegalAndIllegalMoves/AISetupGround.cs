using UnityEngine;
using System.Collections;

public class AI : MonoBehaviour {

	private GameManager _GameManager; 	// this variable can be skipped literally but need to change the rest if we do so
	private int _activePlayer;
	private bool _player1AI;
	private bool _player2AI;
	

	void Start () 
	{
		_GameManager = gameObject.GetComponent<GameManager>();
		_player1AI = _GameManager.player1AI;
		_player2AI = _GameManager.player2AI;
	}
	

	void Update () {
	
		if((_activePlayer == 1 && _player1AI == true) || (_activePlayer == -1 && _player2AI == true))
		{
			// call the AI function here or enemy possible move here 
		}
		
		
		
	}

	
	
}