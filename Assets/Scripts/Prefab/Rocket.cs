using UnityEngine;

public enum RocketType
{
    Vertical,
    Horizontal
}


public class Rocket : TileObject
{
    public RocketType direction;        

    public Rocket(int internalColumn, int internalRow, RocketType direction, string state, Vector2 position, float speed, GameObject cubeGO)
        : base(internalColumn, internalRow, state, position, speed, cubeGO)
    {
        this.direction = direction;
    }
}
