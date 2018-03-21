using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts
{
    public class Move
    {
        public List<GameSquare> toFlip { get; set; }
        public GameSquare centerSquare { get; set; }
        public float valueOfMove { get; set; }

        public Move (GameSquare centerSquare, List<GameSquare> toFlip, float value)
        {
            this.centerSquare = centerSquare;
            this.toFlip = toFlip;
            valueOfMove = value;
        }

        public Move(Move move)
        {
            if (move != null)
            {
                this.valueOfMove = move.valueOfMove;
                this.centerSquare = new GameSquare(move.centerSquare);
                this.toFlip = move.toFlip.ConvertAll(gameSquare => new GameSquare(gameSquare));
            }
        }

        public Move()
        { 
}
    }
}
