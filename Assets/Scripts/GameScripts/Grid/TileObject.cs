using UnityEngine;
using System.Collections.Generic;
using UnityEditor.Build;


public class TileObject
{
    public string state;

    // Physics
    public Vector2 position;
    public Queue<float> destinationList;
    public float speed;
    public float shrinkSpeed;
    public int internalColumn;
    public int internalRow;


    // Actual Reference to the object
    public GameObject tile;
    public TileObject(int internalColumn, int internalRow, string state, Vector2 position, float speed, GameObject tile)
    {
        this.internalColumn = internalColumn;
        this.internalRow = internalRow;
        this.destinationList = new Queue<float>();
        this.state = state;
        this.position = position;
        this.speed = speed;
        this.tile = tile;
        this.shrinkSpeed = 0.2f;
    }

    
}
