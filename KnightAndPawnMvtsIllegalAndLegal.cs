//  Yohan

// I added more documentation for each piece 
// I tried my best to move from normal c# syntax to accomodate some unity 
// attributes yet the debugging will take some time in case we need to change stuff around


// Note: CLASS NAMES NEED TO BE CHANGED SINCE I ASSEMBLED ALL IN ONE FILE 
//       SO WE NEED TO SEPERATE ALL THESE CHUNKS OF CODE TO THEIR RESPECTIVE FILES

// 2ND Note: I used MonoBehavior yet as other notes later will mention we can make a global
//              file that we can instantiate/inherit instead and that file will instanciate the 
//              MonoBehavior instead 


//--This is just a class to be added to allow global usage by other scripts---
public abstract class ChessPieceGeneral : MonoBehaviour
{

    public static int isBlack = 0; // I explain it later why I chosee this approach 


    // in C# if you want to override later on some method
    // you need to also update the original method signature 
    // and that happens by adding the keyword VIRTUAL!!!!!
    public virtual bool[,] checkValidMoveInit()
    {

        // working with array that might be adjusted to be static later on will make life easy
        // better than at each time having a game object called for each checss piece and then 
        // getting the position component for each 
        return new bool[8,8];
    }

    public int curX { 

        get; 
        set; 

    }

    public int curY { 

        get; 
        set; 
    
    }

    public void UpdatePosition(int x, int y)
    {
        this.curX = x;
        this.curY = y;
    }
}

//--------------------------------------------------------------
// now tackling the knight 

// so this will be a script by itself yet we can think of inheritting that global class 
// I mentioned before instead of monobihaviour 
public class Knight : MonoBehaviour
{

    // need to override the method that declares the array 
    public override bool[,] checkValidMoveInit()
    {

        // declare a result array to be sused as reference 
        bool[,] result= new bool[8, 8];

        // check all possible moves here
        isValid(curX + 1, curY + 2, ref result);
        isValid(curX + 2, curY + 1, ref result);
        isValid(curX - 1, curY - 2, ref result);
        isValid(curX - 2, curY - 1, ref result);
        isValid(curX + 1, curY - 2, ref result);
        isValid(curX + 2, curY - 1, ref result);
        isValid(curX - 1, curY + 2, ref result);
        isValid(curX - 2, curY + 1, ref result);

        // reutrn the array 
        return result;
    }

    // evaluate the knight move 

    // I kept this as void as I used the result array as reference 
    // so no need at each time to return the updated array 
    public void isValid(int x, int y, ref bool[,] result)
    {

        // variable to store the current chess piece 
        ChessPieceGeneral piece;

        // check the validity of the possible projection using x and y 
        if(x >= 0 && x < 8 && y >= 0 && y < 8)
        {

            // check the spot and store whatever is there in piece 
            // it might be filled or has a null there 
            piece = BoardManager.trackPositions[x, y];
            
            // if the space is empty we can safely fill it 
            if (!piece) 
                result[x, y] = true;

            // what am I think here? (check this again !!!!!)
            // Logic:
            //  
            //      ENNEMY possibility to be attacked ? 
            else if (piece.isBlack != isBlack)
                result[x, y] = true;


        }
    }
}

//------------------------------------------------------------------
// now the pawn:


/*
    the pawn can only move in specific waysforward one spot for every turn. 
    ---BUT---
    if the pawn is located on its start field we can move the pawn 2 spots 
    upwards unless there is another chess piece there. 
    --Attacks:---
    Only diagonal attacks are allowed!
*/
public class Pawn : ChessPieceGeneral
{

    // I explained before in the general global class why I am trying to override this 
    public override bool[,] checkValidMoveInit()
    {

        // declare an array that will be used as result to be returned later on 
        bool[,] referencedArray = new bool[8, 8];

        // create an of the global class
        // 2 chess pieces 
        ChessPieceGeneral piece1, piece2;

        // after researching I think this might help in the long run
        // since we have white and black pieces and illegal moves 
        // not only depend on same team pieces but also on killing the other color ennemies
        if (isBlack == 0) // if we have a white piece 
        {

            // in this process we will evaluate being in the corners so 0 and 7 are the main 
            // indexes to be checked 

            //  move Forward
            if(curY != 7)
            {

                // check if there is a gameObject present in the next position 
                // we need to get the array from the file where it is declared 

                // NEXT Instructions: 
                //      -> initialize the array for the board 
                //      -> replace BoardManager with the file that has the array there referenced
                //      -> the following names need also to be replaced if you declare the array with another name
                //          yet the indexes are the same 
                piece1 = BoardManager.trackPositions[curX, curY + 1];

                if(piece1 == null) {

                    referencedArray[curX, curY + 1] = true;

                }

            }
            // see if you can move two spots Forward in case there is no other piece in the way 
            // that can stop the pawn 
            if(curY == 1)
            {

                // get whatever is at that position if there is nothing it will just store null and 
                // make life easier for us 
                piece1 = BoardManager.trackPositions[curX, curY + 1];
                piece2 = BoardManager.trackPositions[curX, curY + 2];

                // if they are both null we can safely jump by 2
                // and then set that position as filled 
                if (!piece1 && !piece2) {

                    referencedArray[curX, curY + 2] = true;

                }

            }

            // move Diagonally to the Left
            if(curX != 0 && curY != 7)
            {

                piece1 = BoardManager.trackPositions[curX - 1, curY + 1];

                if(!piece1 && piece1.isBlack == 1) {

                    referencedArray[curX - 1, curY + 1] = true;

                }

            }

            // move Diagonally to the Right
            if (curX != 7 && curY != 7)
            {

                piece1 = BoardManager.trackPositions[curX + 1, curY + 1];

                if (!piece1 && piece1.isBlack == 1) {

                    referencedArray[curX + 1, curY + 1] = true;

                }

            }

        }
        // if we are dealing with a black piece this time 
        else if (isBlack == 1)
        {

            // go left diagonally 
            if (curX != 0 && curY != 0)
            {
                piece1 = BoardManager.trackPositions[curX - 1, curY - 1];

                if (!piece1 && piece1.isBlack == 0) {

                    referencedArray[curX - 1, curY - 1] = true;

                }

            }

            // go right diagonally 
            if (curX != 7 && curY != 0)
            {

                piece1 = BoardManager.trackPositions[curX + 1, curY - 1];

                if (!piece1 && piece1.isBlack == 0) {

                    referencedArray[curX + 1, curY - 1] = true;

                }

            }

            // move orward by 1 
            if (curY != 0)
            {
                piece1 = BoardManager.trackPositions[curX, curY - 1];

                if (!piece1) 

                    referencedArray[curX, curY - 1] = true;

            }
            
            // move forward by 2
            if (curY == 6)
            {

                piece1 = BoardManager.trackPositions[curX, curY - 1];
                piece2 = BoardManager.trackPositions[curX, curY - 2];

                if (!piece1 && !piece2) 
                    
                    referencedArray[curX, curY - 2] = true;
            }
        }

        // return the updated array 
        return referencedArray;
    }
}