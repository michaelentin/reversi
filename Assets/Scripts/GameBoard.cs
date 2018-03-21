using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{
    public class GameBoard : MonoBehaviour {

        List<GameSquare> visualBoard = new List<GameSquare>();
        List<GameSquare> playedOn = new List<GameSquare>();

        private const float STARTING_X = -3.5f;
        private const float STARTING_Z = -3.5f;

        private const float BOARD_HEIGHT = 1.15f;


        private const float PIECE_STARTING_HEIGHT = 1.2f;

        private Player currentTurn;
        private Player opponent;

        private bool AIOn = true;
        private bool bestMoveOn = true;
        private bool checkedForEndGame = false;
        private bool PlayedAI = false;
        private bool anyFlipping = false;
        private bool paused = false;
        private bool movingCamera = false;
        private float currentCameraPosition = .13f;

        public int whiteScore = 2;
        public int blackScore = 2;

        public int depth;

        private TextMesh GameOverMesh;
        private Camera MainCamera;
        private Slider mySlider;
        private TextMesh DepthMesh;

        // Use this for initialization
        void Start()
        {
            // set current turn
            currentTurn = Player.black;
            opponent = Player.white;

            float x_position = STARTING_X;
            float z_position = STARTING_Z;
            for (int i = 0; i < 8; i++)
            {
                String vals = "";
                z_position = STARTING_Z;
                for (int j = 0; j < 8; j++)
                {
                    GameSquare toAdd = new GameSquare(x_position, z_position);
                    vals += " " + toAdd.value;

                    // if an intial game piece, make sure we assign the right player to it

                    // intial white positions
                    if ((x_position == -.5 && z_position == -.5) || (x_position == .5 && z_position == .5))
                    {
                        toAdd.piece = new GamePiece(Player.white);
                    }

                    // intial black position
                    if ((x_position == .5 && z_position == -.5) || (x_position == -.5 && z_position == .5))
                    {
                        toAdd.piece = new GamePiece(Player.black);
                    }

                    if (toAdd.piece != null)
                    {
                        playedOn.Add(toAdd);
                    }

                    visualBoard.Add(toAdd);

                    // DEBUG
                    //if (toAdd.piece != null)
                    //{
                    //    Debug.Log(x_position + ", " + z_position + ": " + toAdd.piece.ownedBy);
                    //}
                    //else
                    //{
                    //    Debug.Log(x_position + ", " + z_position);

                    //}
                    

                    z_position++;
                }

                // making sure my weights are correct
                // Debug.Log(vals);

                x_position++;
                
            }

            UpdateScoreVisuals();

            TextMesh[] meshes = FindObjectsOfType<TextMesh>();
            foreach (TextMesh mesh in meshes)
            {
                if (mesh.name.Equals("GAME OVER"))
                {
                    GameOverMesh = mesh;
                }

                if (mesh.name.Equals("Depth"))
                {
                    DepthMesh = mesh;
                }
            }

            mySlider = FindObjectOfType<Slider>();

            MainCamera = FindObjectOfType<Camera>();

        }

        // Update is called once per frame
        void Update()
        {
            depth = (int) mySlider.value;
            DepthMesh.text = depth.ToString();
            if (Input.GetKeyDown("p") && !movingCamera)
            {
                if (paused)
                {
                    Debug.Log("display game");
                    paused = false;
                }
                else
                {
                    Debug.Log("display menu");
                    paused = true;
                }
                movingCamera = true;
            }

            if (movingCamera)
            {
                if (currentCameraPosition == .13f)
                {
                    MainCamera.transform.position -= new Vector3(0, 0, .5f);
                    if (MainCamera.transform.position.z <= -20.14f)
                    {
                        currentCameraPosition = -20.14f;
                        MainCamera.transform.position = new Vector3(MainCamera.transform.position.x, MainCamera.transform.position.y, currentCameraPosition);
                        movingCamera = false;
                    }
                }
                else
                {
                    MainCamera.transform.position += new Vector3(0, 0, .5f);
                    if (MainCamera.transform.position.z >= .13)
                    {
                        currentCameraPosition = .13f;
                        MainCamera.transform.position = new Vector3(MainCamera.transform.position.x, MainCamera.transform.position.y, currentCameraPosition);
                        movingCamera = false;
                    }
                }
            }

            bool endOfGame = false;
            if (!checkedForEndGame)
            {
                endOfGame = CheckForEndGame(visualBoard);
                checkedForEndGame = true;
            }

            // Debug.Log("played on: " + playedOn.Count);

            // check if the player can move
            List<Move> currentLegalMoves = GenerateLegalMoves(currentTurn, visualBoard);
            if (currentLegalMoves.Count == 0)
            {
                Debug.Log("Conceding turn");
                ConcedeTurn();
            }

            anyFlipping = false;

            ModelPiece[] pieces = GetComponentsInChildren<ModelPiece>();

            foreach (ModelPiece piece in pieces)
            {
                if (piece.flip)
                {
                    anyFlipping = true;
                    break;
                }
            }

            if (!paused && !endOfGame && !anyFlipping)
            {
                //Debug.Log(AIOn + " | " + PlayedAI);
                if (AIOn && currentTurn == Player.white) // 
                {
                    Debug.Log("Playing AI");
                    Move toPlay = new Move();
                    toPlay = PlayAI();

                    if (toPlay != null)
                    {
                        List<GameSquare> toFlip = new List<GameSquare>();

                        // we want to actually update our board
                        foreach (GameSquare square in toPlay.toFlip)
                        {
                            toFlip.Add(visualBoard.Find(gameSquare => gameSquare.isGameSquareEqualByPosition(square)));
                        }

                        // find the one on our board
                        GameSquare ourMove = visualBoard.Find(gameSquare => gameSquare.isGameSquareEqualByPosition(toPlay.centerSquare));


                        // update visuals
                        FlipPieces(toFlip);
                        PlayPiece(ourMove);

                        UpdateScoreVisuals();

                    }
                    else
                    {
                        ConcedeTurn();
                    }
                }
                else
                {
                    if (bestMoveOn)
                    {
                        DisplaySomeMoves();
                        bestMoveOn = false;
                    }

                    PlayUser();
                }
            }

            if (endOfGame)
            {
                if (blackScore > whiteScore)
                {
                    GameOverMesh.text = "YOU WIN!";
                    GameOverMesh.color = Color.green;
                }
                GameOverMesh.transform.localPosition = new Vector3(-1.21f, GameOverMesh.transform.localPosition.y, GameOverMesh.transform.localPosition.z);
            }

            checkedForEndGame = false;
        }

        private void PlayUser()
        {
            
            // someone's turn
            if (Input.GetMouseButtonDown(0) && !anyFlipping)
            {
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit, 100))
                {
                    Debug.Log(hit.point);
                    GameSquare clicked = FindGameSquareClicked(hit.point);

                    if (clicked != null)
                    {
                        if (clicked.piece != null)
                        {
                            // if not null, square already occupied
                            // Debug.Log("Cannot play on an already occupied square");
                        }
                        else
                        {
                            List<GameSquare> move = GetToCaptureList(clicked, true, currentTurn, visualBoard);

                            // only attempt to play if it's a legal move
                            if (move.Count > 0)
                            {
                                // attempt to play
                                PlayPiece(clicked);

                                // update the score
                                UpdateScoreVisuals();
                            }
                        }
                    }
                    else
                    {
                        // Debug.Log("Could not find game square, please try to click again");
                    }


                    if (Input.GetMouseButton(1))
                    {
                        ConcedeTurn();
                    }
                }
            }
        }

        private bool CheckForEndGame(List<GameSquare> board)
        {
            List<GameSquare> played = board.FindAll(square => square.piece != null);
            // check for end of game
            if (played.Count >= 64)
            {
                Debug.Log("Play count reached");
                return true;
            }

            List<Move> currentLegalMoves = GenerateLegalMoves(currentTurn, board);
            List<Move> opponentLegalMoves = GenerateLegalMoves(opponent, board);

            if (currentLegalMoves.Count == 0 && opponentLegalMoves.Count == 0)
            {
                Debug.Log("No Possible Moves");
                return true;
            }

            return false;
        }

        private void ConcedeTurn()
        {
            Player temp = currentTurn;
            currentTurn = opponent;
            opponent = temp;
        }

        private void DisplaySomeMoves()
        {
            // would probably choose one of these if we don't find a terminal position 
            List<Move> legalMoves = GenerateLegalMoves(currentTurn, visualBoard);

            Move bestMove = FindBestMove(legalMoves);
            Move worstMove = FindWorstMove(legalMoves);

            int numLegalMoves = legalMoves.Count;
            float bestX = bestMove.centerSquare.centerX;
            float bestZ = bestMove.centerSquare.centerZ;

            float worstX = bestMove.centerSquare.centerX;
            float worstZ = bestMove.centerSquare.centerZ;

            Debug.Log("Best move at: " + bestZ + ", " + bestX + ". You will flip " + bestMove.toFlip.Count + " tiles and its value is: " + bestMove.valueOfMove);
            Debug.Log("Worst move at: " + bestZ + ", " + bestX + ". You will flip " + worstMove.toFlip.Count + " tiles and its value is: " + worstMove.valueOfMove);
        }

        private Move PlayAI()
        {
            if (!PlayedAI)
            {
                
                Debug.Log("thinking");

                List<Move> legalMoves = GenerateLegalMoves(currentTurn, visualBoard);

                // help with calculation time
                if (legalMoves.Count == 1)
                {
                    // Debug.Log("Only one move available");
                    PlayedAI = true;
                    return legalMoves.First();
                }

                List<GameSquare> boardCopy = new List<GameSquare>();

                boardCopy = visualBoard.ConvertAll(gameSquare => new GameSquare(gameSquare));

                GameState state = new GameState();
                state.board = boardCopy;
                state.blackScore = boardCopy.FindAll(square => square.piece != null && square.piece.ownedBy == Player.black).Count;
                state.whiteScore = boardCopy.FindAll(square => square.piece != null && square.piece.ownedBy == Player.white).Count;
                state.associatedMove = new Move();
                state.firstMoveToGetHere = new Move();

                GameState nextIdealGameState = new GameState();

                Debug.Log("pre nega max, it is " + currentTurn + "'s turn");
                nextIdealGameState = NegaMax(state, depth, 1, Player.white, Player.black, float.MinValue, float.MaxValue);


                Debug.Log(nextIdealGameState.firstMoveToGetHere);
                if (nextIdealGameState.firstMoveToGetHere != null)
                {
                    Debug.Log("next ideal game state has move at: " + nextIdealGameState.firstMoveToGetHere.centerSquare.centerZ + ", " + nextIdealGameState.firstMoveToGetHere.centerSquare.centerX + ". It will flip " + nextIdealGameState.firstMoveToGetHere.toFlip.Count + " pieces");
                }

                PlayedAI = true;

                return nextIdealGameState.firstMoveToGetHere;
            }

            // Debug.Log("down here?");
            PlayedAI = false;
            return null;
        }

        private GameState NegaMax(GameState gameState, int depth, int helpNega, Player currentPlayer, Player opposingPlayer, float alpha, float beta)
        {
            // do nega max
            GameState bestGameState = gameState;

            // UpdateScoreVisuals();
            // Debug.Log("post flip score: " + blackScore + ", " + whiteScore);

            bool endOfGame = CheckForEndGame(gameState.board);

            if (depth == this.depth && endOfGame)
            {
                //Debug.Log("hmmm");
            }

            List<Move> legalMoves = GenerateLegalMoves(currentPlayer, gameState.board);

            if (depth == 0 || endOfGame)
            {
                if (endOfGame)
                {
                    float thisScore = gameState.board.FindAll(gameSquare => gameSquare.piece != null && gameSquare.piece.ownedBy == currentPlayer).Count;
                    float theirScore = gameState.board.FindAll(gameSquare => gameSquare.piece != null && gameSquare.piece.ownedBy == opposingPlayer).Count;
                    if (thisScore > theirScore)
                    {
                        gameState.valueOfState = 1000000000f;
                    }
                    else
                    {
                        gameState.valueOfState = .1f;
                    }
                }
                gameState.valueOfState *= helpNega;
                return gameState;
            }

            float bestVal = float.MinValue;

            if (legalMoves.Count == 0)
            {
                return gameState;
            }

            IEnumerable<Move> orderedLegalMoves = legalMoves.OrderBy(move => move.valueOfMove);

            Debug.Log("there are " + legalMoves.Count + " legal moves for " + currentPlayer);

            foreach (Move move in orderedLegalMoves)
            {

                GameState nextGameState = new GameState();

                // create next game state from original game state
                nextGameState.board = new List<GameSquare>();
                nextGameState.board = gameState.board.ConvertAll(gameSquare => new GameSquare(gameSquare));
                nextGameState.associatedMove = new Move(move);
                nextGameState.valueOfState = move.centerSquare.value * (move.toFlip.Count);
                nextGameState.firstMoveToGetHere = new Move();

                if (depth == this.depth)
                {
                    // Debug.Log(move.centerSquare.centerZ + ", " + move.centerSquare.centerX);
                    nextGameState.firstMoveToGetHere = new Move(move);
                }
                else
                {
                    nextGameState.firstMoveToGetHere = new Move(gameState.firstMoveToGetHere);
                }

                //// "play" the move on the new game board
                GameSquare placeToPlay = nextGameState.board.Find(gameSquare => gameSquare.isGameSquareEqualByPosition(move.centerSquare));
                placeToPlay.piece = new GamePiece(currentPlayer);

                List<GameSquare> toCaptureList = GetToCaptureList(placeToPlay, false, currentPlayer, nextGameState.board);

                foreach (GameSquare square in toCaptureList)
                {
                    square.piece.ownedBy = currentPlayer;
                }

                // update the score on the new game state
                nextGameState.blackScore = nextGameState.board.FindAll(gameSquare => gameSquare.piece != null && gameSquare.piece.ownedBy == Player.black).Count;
                nextGameState.whiteScore = nextGameState.board.FindAll(gameSquare => gameSquare.piece != null && gameSquare.piece.ownedBy == Player.white).Count;

                // send that new game state to the next round of negamax

                GameState returnedGameState = new GameState();
                returnedGameState = NegaMax(nextGameState, depth - 1, -helpNega, opposingPlayer, currentPlayer, -beta, -alpha);

                // negate the returned value
                returnedGameState.valueOfState = -returnedGameState.valueOfState;
                float returnedVal = returnedGameState.valueOfState;

                Debug.Log("returned val: " + returnedVal);

                if (returnedVal > bestVal)
                {
                    bestGameState = returnedGameState;
                    bestVal = returnedVal;
                }

                if (returnedVal > alpha) alpha = returnedVal;
                if (alpha > beta) break;
            }


            return bestGameState;
        }

        private List<Move> GenerateLegalMoves(Player currentPlayer, List<GameSquare> board)
        {
            
            List<GameSquare> UnplayedSpaces = board.FindAll(square => square.piece == null);
            // Debug.Log(currentPlayer + "has these unplayed: " + UnplayedSpaces.Count);

            List<Move> legalMoves = new List<Move>();

            foreach (GameSquare unplayed in UnplayedSpaces)
            {
                List<GameSquare> toCaptureList = GetToCaptureList(unplayed, false, currentPlayer, board);
                if (toCaptureList.Count > 0)
                {
                    // Debug.Log(currentPlayer + "will capture " + toCaptureList.Count);
                    legalMoves.Add(new Move(unplayed, toCaptureList, unplayed.value * toCaptureList.Count));
                }
            }
           // Debug.Log(currentPlayer + " has " + legalMoves.Count + " legal moves");

            return legalMoves;
        }

        private Move FindWorstMove(List<Move> legalMoves)
        {
            float lowestScoringLegalMove = legalMoves.Min(move => move.valueOfMove);
            Move worstMove = legalMoves.Find(move => move.valueOfMove == lowestScoringLegalMove);

            return worstMove;
        }

        private Move FindBestMove(List<Move> legalMoves)
        {
            
            float highestScoringLegalMove = legalMoves.Max(move => move.valueOfMove);
            Move bestMove = legalMoves.Find(move => move.valueOfMove == highestScoringLegalMove);

            return bestMove;
        }

        private void UpdateScoreVisuals()
        {
            whiteScore = visualBoard.FindAll(square => square.piece != null && square.piece.ownedBy == Player.white).Count;
            blackScore = visualBoard.FindAll(square => square.piece != null && square.piece.ownedBy == Player.black).Count;

            TextMesh[] meshes = GetComponentsInChildren<TextMesh>();
            foreach (TextMesh mesh in meshes)
            {
                if (mesh.name.Equals("WhiteScore"))
                {
                    mesh.text = whiteScore.ToString();
                }
                if (mesh.name.Equals("BlackScore"))
                {
                    mesh.text = blackScore.ToString();
                }
            }
        }

        private void FlipPieces(List<GameSquare> toFlip)
        {
            ModelPiece[] pieces = FindObjectsOfType<ModelPiece>();

            foreach(GameSquare square in toFlip)
            {
                Debug.Log("need to flip at: " + square.centerX + ", " + square.centerZ);
            }
            
            foreach (ModelPiece piece in pieces)
            {
                if (piece.pieceInfo.ownedBy == opponent)
                {
                    //Debug.Log("Found an opponent piece " + piece.name + " in the field at: " + piece.transform.localPosition.x + ", " + piece.transform.localPosition.z);
                    GameSquare spot = toFlip.Find(square => Math.Abs(square.centerX - piece.transform.localPosition.x) < .0001 && Math.Abs(square.centerZ - piece.transform.localPosition.z) < .0001);
                    if (spot != null)
                    {
                        // flip the piece
                        piece.pieceInfo.ownedBy = currentTurn;
                        piece.flip = true;

                        // update the spot info
                        spot.piece.ownedBy = currentTurn;
                    }
                }
            }
        }

        private List<GameSquare> GetToCaptureList(GameSquare clicked, bool realTime, Player currentPlayer, List<GameSquare> board)
        {
            // if it is not next to an opponent square, no good
            Player opposingPlayer = GetOpposingPlayer(currentPlayer);

            // just get the squares
            List<GameSquare> surroundingSquaresWithPieces = GetSurroundingSquaresWithPieces(clicked, board);

            // if no pieces on surrounding squares, no good
            if (surroundingSquaresWithPieces.Count == 0)
            {
                // Debug.Log("no pieces nearby");
                return new List<GameSquare>();
            }
            else
            {
                // Debug.Log("surrounding squares with pieces: " + surroundingSquaresWithPieces.Count);
            }

            // retrieve surrounding squares with opponent pieces
            List<GameSquare> surroundingOpponentSquares = GetSurroundingOpponentSquares(clicked, surroundingSquaresWithPieces, currentPlayer, opposingPlayer);

            // if no surrounding pieces are opponent pieces, nod good
            if (surroundingOpponentSquares.Count == 0)
            {
                // Debug.Log("uh oh, this spot is not next to an opponent location");
                return new List<GameSquare>();
            }
            else
            {
                // Debug.Log("surround squares next to opponent: " + surroundingOpponentSquares.Count);
            }


            // if oppenent's square would not not enclosed in any direction by another
            // of your pieces, spot is not a legal move
            List<GameSquare> toFlip = GetPiecesToFlip(clicked, surroundingOpponentSquares, opposingPlayer, board);

            if (toFlip.Count > 0)
            {
                // Debug.Log("legal move, will capture: " + toFlip.Count);

                if (realTime)
                {
                    FlipPieces(toFlip);
                }

                return toFlip;
            }
            else
            {
                // Debug.Log("not gonna capture anything :(");
                return new List<GameSquare>();
            }
        }

        private static Player GetOpposingPlayer(Player currentPlayer)
        {
            Player opposingPlayer = Player.white;

            if (currentPlayer == Player.white)
            {
                opposingPlayer = Player.black;
            }
            if (currentPlayer == Player.black)
            {
                opposingPlayer = Player.white;
            }

            return opposingPlayer;
        }

        private List<GameSquare> GetPiecesToFlip(GameSquare clicked, List<GameSquare> surroundingOpponentSquares, Player opposingPlayer, List<GameSquare> board)
        {
            List<GameSquare> toFlip = new List<GameSquare>();

            // check to the left of clicked
            foreach (GameSquare square in surroundingOpponentSquares)
            {
                // next in line is a var for holding the next square in the line (logic done in GameSquare)
                GameSquare nextInLine = square;
                // set up a temporary to flip list
                List<GameSquare> tempFlip = new List<GameSquare>();
                // used for checking if the temp flips should actually be added or not
                bool isLegal = false;

                // continue until we hit a piece of our own or there are no pieces played at the next space
                while (nextInLine != null && nextInLine.piece != null)
                {
                    // logic for deciding what to do with the current piece

                    // we know there is a piece played, therefore it is either ours or their's
                    if (nextInLine.piece.ownedBy == opposingPlayer)
                    {
                        tempFlip.Add(nextInLine);
                    }
                    else
                    {
                        // stop once we hit our own piece
                        isLegal = true;
                        break;
                    }

                    // logic for getting next in line for next iteration

                    // get next relative position
                    GameSquare.RelativePosition relativePosition = square.GetRelativePositionTo(clicked);

                    // if not an error, do logic
                    if (relativePosition != GameSquare.RelativePosition.error)
                    {
                        // retrieve coordinates of the next square
                        GameSquare coordinatesOfNext = nextInLine.GetNextSquare(relativePosition);

                        // actually find it on the board
                        nextInLine = board.Find(gameSquare => gameSquare.isGameSquareEqualByPosition(coordinatesOfNext));
                    }
                    else
                    {
                        Debug.Log("there was an error in finding relative position");
                        return new List<GameSquare>();
                    }
                }

                // only add the temp if we ran into another of our pieces in the search
                if (isLegal)
                {
                    toFlip.AddRange(tempFlip);
                }
                
            }

            return toFlip;
        }

        private List<GameSquare> GetSurroundingOpponentSquares(GameSquare clicked, List<GameSquare> surroundingSquaresWithPieces, Player currentPlayer, Player opposingPlayer)
        {

            List<GameSquare> surroundingOpponentSquares = new List<GameSquare>();

            foreach (GameSquare gameSquare in surroundingSquaresWithPieces)
            {
                if (gameSquare.piece.ownedBy == opposingPlayer)
                {
                    surroundingOpponentSquares.Add(gameSquare);
                }
            }

            return surroundingOpponentSquares;
        }

        private List<GameSquare> GetSurroundingSquaresWithPieces(GameSquare clicked, List<GameSquare> board)
        {
            // initial positions
            float current_x = clicked.centerX - 1;
            float current_z = clicked.centerZ - 1;

            // intialize surrounding squares
            List<GameSquare> surroundingSquares = new List<GameSquare>();

            // find any squares that are next to this one
            for (int i = 0; i < 3; i++)
            {
                current_z = clicked.centerZ - 1;
                for (int j = 0; j < 3; j++)
                {
                    GameSquare toFind = new GameSquare(current_x, current_z);
                    GameSquare currentSquare = board.Find(gameSquare => gameSquare.isGameSquareEqualByPosition(toFind));
                    if (currentSquare != null)
                    {
                        surroundingSquares.Add(currentSquare);
                    }
                    current_z++;
                }
                current_x++;
            }

            // find squares surrounding this square with game pieces on them
            List<GameSquare> surroundingSquaresWithPieces = surroundingSquares.FindAll(gameSquare => gameSquare.piece != null);

            // if no pieces are surrounding this square, return empty list
            if (surroundingSquaresWithPieces == null)
            {
                return new List<GameSquare>();
            }

            return surroundingSquaresWithPieces;
        }

        // Call for actuall updating the visual aspect of the game
        private void PlayPiece(GameSquare gameSquare)
        {
            // update our boards
            visualBoard.Find(query => query.isGameSquareEqualByPosition(gameSquare)).piece = new GamePiece(currentTurn);
            playedOn.Add(gameSquare);

            GameObject piece = (GameObject) Instantiate(Resources.Load("Model_Piece"));
            piece.transform.parent = gameObject.transform;
            piece.transform.localPosition = new Vector3(gameSquare.centerX, PIECE_STARTING_HEIGHT, gameSquare.centerZ);

            piece.AddComponent<ModelPiece>();
            piece.GetComponent<ModelPiece>().setInfo(new GamePiece(currentTurn));

            if (currentTurn == Player.white)
            {
                Debug.Log("White's move");
                piece.transform.Rotate(new Vector3(180, 0, 0));
                currentTurn = Player.black;
                opponent = Player.white;
            }
            else
            {
                Debug.Log("Black's move");
                currentTurn = Player.white;
                opponent = Player.black;
            }

            piece.AddComponent<Rigidbody>();
            piece.GetComponent<Rigidbody>().isKinematic = true;

            bestMoveOn = true;
            PlayedAI = false;

        }

        private GameSquare FindGameSquareClicked(Vector3 point)
        {
            GameSquare toReturn = visualBoard.Find(gameSquare => gameSquare.isPointInside(point));

            return toReturn;
        }

    }
}
