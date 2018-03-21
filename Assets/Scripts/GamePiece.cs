using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Assets.Scripts
{
    public enum Player { white, black };

    [System.Serializable]
    public class GamePiece
    {
        public Player ownedBy;

        public GamePiece()
        {

        }

        public GamePiece(Player ownedBy)
        {
            this.ownedBy = ownedBy;
        }
    }
}
