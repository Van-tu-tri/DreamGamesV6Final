using DG.Tweening;
using UnityEngine;

/*
 NOTE
    Sometimes DOTween animation gives warnings. Maybe the problem is tihs: when the vase destroyed quickly by double rockets,
it does not have time to finish its DOTween animation. Keep an eye on it.

*/


public class Vase : Obstacle
{
    public Prefabs prefabs;
    public Vase(int internalColumn, int internalRow, int initialHealth, string state, Vector2 position, float speed, GameObject tile, Prefabs prefabs)
        : base(internalColumn, internalRow, initialHealth, state, position, speed, tile)
    {
        this.prefabs = prefabs;
    }

    public override bool Damage()
    {
        health--;

        if (health == 1)
        {
            // Little Animation and Sprite Change
            SpriteRenderer sr = tile.GetComponent<SpriteRenderer>();
            if (sr != null && prefabs != null)
            {
                Sequence s = DOTween.Sequence();

                s.Append(tile.transform.DOScale(1.15f, 0.12f).SetEase(Ease.OutQuad))   
                 .AppendCallback(() => {
                     sr.sprite = prefabs.brokenVase; 
                 })
                 .Append(tile.transform.DOScale(1f, 0.15f).SetEase(Ease.OutBack))     
                 .Join(sr.DOColor(new Color(1f, 0.8f, 0.8f), 0.15f))                  
                 .Append(sr.DOColor(Color.white, 0.15f));                            
            }
        }

        if (health <= 0)
        {
            return true;
        }

        return false;
    }


}
