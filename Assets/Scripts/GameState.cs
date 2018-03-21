using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts
{
    public class GameState
    {
        public List<GameSquare> board { get; set; }
        public float valueOfState {get;set;}
        public int blackScore { get; set; }
        public int whiteScore { get; set; }

        public Move associatedMove { get; set; }
        public Move firstMoveToGetHere { get; set; }

        public GameState()
        {

        }
        public GameState(List<GameSquare> board, float valueOfState)
        {
            this.board = board;
            this.valueOfState = valueOfState;
        }

        public GameState(GameState gameState)
        {
            this.board = gameState.board;
            this.valueOfState = gameState.valueOfState;
            this.blackScore = gameState.blackScore;
            this.whiteScore = gameState.whiteScore;
        }
    }
}
