using Assets.Scripts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

[System.Serializable]
public class ModelPiece : MonoBehaviour
{
    public GamePiece pieceInfo;
    private double raiseTo = 2.5;
    private float startingY = 0;
    public bool flip = false;
    private int degreesPerUpdate = 15;
    private int totalDegrees = 0;
    private int maxDegrees = 180;
    private bool rotating = false;
    private bool raising = false;
    private bool falling = false;
    Player currentVisual;

    //public Animator flipAnimator { get; set; }
        
    public void setInfo(GamePiece gamePiece)
    {
        pieceInfo = gamePiece;
        currentVisual = pieceInfo.ownedBy;
    }

    private void Start()
    {
        startingY = transform.localPosition.y;
        raising = true;
    }

    private void GetCurrentVisual()
    {
        float thisAngle = transform.eulerAngles.x;
        Debug.Log(thisAngle);
        if (transform.eulerAngles.x == -0.000002515905)
        {
            currentVisual = Player.white;
            Debug.Log("current visual is: " + currentVisual);
        }
        else
        {
            currentVisual = Player.black;
            Debug.Log("current visual is: " + currentVisual);
        }
    }

    private void Update()
    {
        if (flip)
        {
            if (raising)
            {
                transform.localPosition += new Vector3(0, .05f, 0);
                if (transform.localPosition.y >= raiseTo)
                {
                    raising = false;
                    rotating = true;
                }
            }
            if (rotating)
            {
                transform.Rotate(new Vector3(degreesPerUpdate, 0, 0));
                totalDegrees += degreesPerUpdate;
                if (totalDegrees == maxDegrees)
                {
                    rotating = false;
                    falling = true;
                }
            }
            if (falling)
            {
                transform.localPosition -= new Vector3(0, .05f, 0);
                if (transform.localPosition.y <= startingY)
                {
                    transform.localPosition = new Vector3(transform.localPosition.x, startingY, transform.localPosition.z);
                    totalDegrees = 0;
                    falling = false;
                    raising = true;
                    flip = false;
                }
                
            }
        }
        
    }

}
