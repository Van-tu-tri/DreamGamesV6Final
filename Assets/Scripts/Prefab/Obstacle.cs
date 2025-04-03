using UnityEngine;


public abstract class Obstacle : TileObject
{
    protected int health;

    public Obstacle(int internalColumn, int internalRow, int initialHealth, string state, Vector2 position, float speed, GameObject tile)
        : base(internalColumn, internalRow, state, position, speed, tile)
    {
        this.health = initialHealth;
    }

    public virtual bool Damage()
    {
        health--;

        if (health <= 0)
        {
            return true;

        }
        return false;
    }

    public void PrintHealth()
    {
        Debug.Log(health);
    }


}
