using UnityEngine;

public enum CubeColor 
{
    Red,
    Blue,
    Green,
    Yellow
}

public class Cube : TileObject
{
    public CubeColor color;

    public Cube(int internalColumn, int internalRow, CubeColor color, string state, Vector2 position, float speed, GameObject cubeGO)
        : base(internalColumn, internalRow, state, position, speed, cubeGO)
    {
        this.color = color;
    }
}

