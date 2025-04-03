using UnityEngine;

public class Stone : Obstacle
{
    public Stone(int internalColumn, int internalRow, int initialHealth, string state, Vector2 position, float speed, GameObject tile)
        : base(internalColumn, internalRow, initialHealth, state, position, speed, tile)
    { }


}

