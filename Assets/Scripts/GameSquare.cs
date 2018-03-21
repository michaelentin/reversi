using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts
{
    public class GameSquare
    {
        public float centerX { get; set; }
        public float centerZ { get; set; }

        public float topBorder { get; set; }
        public float bottomBorder { get; set; }
        public float leftBorder { get; set; }
        public float rightBorder { get; set; }
        public float value { get; set; }

        public GamePiece piece { get; set; }

        public enum RelativePosition { upAndLeft, up, upAndRight, left, right, downAndLeft, down, downAndRight, error};

        public GameSquare()
        {
            piece = null;
        }

        public GameSquare(float x, float z)
        {
            centerX = x;
            centerZ = z;

            // to add to left & top (LT) and to add to right & bottom (RB)
            float toAddLT = .45f;
            float toAddRB = .55f;
            topBorder = centerX - toAddLT;
            bottomBorder = centerX + toAddRB;
            leftBorder = centerZ - toAddLT;
            rightBorder = centerZ + toAddRB;

            piece = null;

            value = AssignValue();
        }

        public GameSquare(GameSquare square)
        {
            this.centerX = square.centerX;
            this.centerZ = square.centerZ;
            this.value = square.value;
            if (square.piece != null)
            {
                this.piece = new GamePiece(square.piece.ownedBy);
            }
        }

        public bool isPointInside(Vector3 point)
        {
            //Debug.Log(leftBorder + ", " + rightBorder + ", " + topBorder + ", " + bottomBorder);
            if ((point.z >= this.leftBorder && point.z <= this.rightBorder) &&
                (point.x >= this.topBorder && point.x <= this.bottomBorder))
            {
                return true;
            }

            return false;
        }

        public bool isGameSquareEqualByPosition(GameSquare gameSquare)
        {
            if (gameSquare.centerX == this.centerX && gameSquare.centerZ == this.centerZ)
            {
                return true;
            }

            return false;
        }

        public RelativePosition GetRelativePositionTo(GameSquare clicked)
        {
            RelativePosition relativePosition = RelativePosition.error;

            // square up & to the left of clicked
            if (this.centerX == clicked.centerX - 1 && this.centerZ == clicked.centerZ - 1)
            {
                relativePosition = RelativePosition.upAndLeft;
            }

            // square above clicked
            if (this.centerX == clicked.centerX - 1 && this.centerZ == clicked.centerZ)
            {
                relativePosition = RelativePosition.up;
            }

            // square up & to the right of clicked
            if (this.centerX == clicked.centerX - 1 && this.centerZ == clicked.centerZ + 1)
            {
                relativePosition = RelativePosition.upAndRight;
            }

            // square to the left of clicked
            if (this.centerX == clicked.centerX && this.centerZ == clicked.centerZ - 1)
            {
                relativePosition = RelativePosition.left;
            }

            // square to the right of clicked
            if (this.centerX == clicked.centerX && this.centerZ == clicked.centerZ + 1)
            {
                relativePosition = RelativePosition.right;
            }

            // square below & to the left of clicked
            if (this.centerX == clicked.centerX + 1 && this.centerZ == clicked.centerZ - 1)
            {
                relativePosition = RelativePosition.downAndLeft;
            }

            // square below of clicked
            if (this.centerX == clicked.centerX + 1 && this.centerZ == clicked.centerZ)
            {
                relativePosition = RelativePosition.down;
            }

            // square below & to the right of clicked
            if (this.centerX == clicked.centerX + 1 && this.centerZ == clicked.centerZ + 1)
            {
                relativePosition = RelativePosition.downAndRight;
            }

            return relativePosition;
        }

        public GameSquare GetNextSquare(RelativePosition relativePosition)
        {
            GameSquare nextToCheck = new GameSquare();

            switch(relativePosition)
            {
                case RelativePosition.upAndLeft:
                    nextToCheck = new GameSquare(this.centerX - 1, this.centerZ - 1);
                    break;
                case RelativePosition.up:
                    nextToCheck = new GameSquare(this.centerX - 1, this.centerZ);
                    break;
                case RelativePosition.upAndRight:
                    nextToCheck = new GameSquare(this.centerX - 1, this.centerZ + 1);
                    break;
                case RelativePosition.left:
                    nextToCheck = new GameSquare(this.centerX, this.centerZ - 1);
                    break;
                case RelativePosition.right:
                    nextToCheck = new GameSquare(this.centerX, this.centerZ + 1);
                    break;
                case RelativePosition.downAndLeft:
                    nextToCheck = new GameSquare(this.centerX + 1, this.centerZ - 1);
                    break;
                case RelativePosition.down:
                    nextToCheck = new GameSquare(this.centerX + 1, this.centerZ);
                    break;
                case RelativePosition.downAndRight:
                    nextToCheck = new GameSquare(this.centerX + 1, this.centerZ + 1);
                    break;
            }

            return nextToCheck;
        }

        private float AssignValue()
        {
            float value = 10;

            // edges have a slightly larger weight than other squares
            if (centerX == 3.5 || centerX == -3.5 || centerZ == 3.5 || centerZ == -3.5)
            {
                value = 25f;
            }

            // corners have a huge weight
            if (centerX == -3.5 && centerZ == -3.5 ||
                centerX == -3.5 && centerZ == 3.5 ||
                centerX == 3.5 && centerZ == -3.5 ||
                centerX == 3.5 && centerZ == 3.5)
            {
                value = 1000f;
            }

            // "x" squares should not be played on at all costs
            if (centerX == -2.5 && centerZ == -2.5 ||
                centerX == -2.5 && centerZ == 2.5 ||
                centerX == 2.5 && centerZ == -2.5 ||
                centerX == 2.5 && centerZ == 2.5)
            {
                value = 1f;
            }

            // "c" squares should try to be avoided as well
            if (centerX == -3.5 && centerZ == -2.5 ||
                centerX == -3.5 && centerZ == 2.5 ||
                centerX == -2.5 && centerZ == -3.5 ||
                centerX == -2.5 && centerZ == 3.5 ||
                centerX == 2.5 && centerZ == -3.5 ||
                centerX == 2.5 && centerZ == 3.5 ||
                centerX == 3.5 && centerZ == -2.5 ||
                centerX == 3.5 && centerZ == 2.5)
            {
                value = 2f;
            }

            // everything else gets no multiplication

            return value;
        }

    }
}
