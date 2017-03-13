using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquareScript : MonoBehaviour
{
    public Sprite OddSprite;
    public Sprite EvenSprite;
    // Use this for initialization
    private void Start()
    {
        Vector2 pos = transform.position;

        if ((pos.x + pos.y) % 2 != 0)
        {
            GetComponent<SpriteRenderer>().sprite = EvenSprite;
        }
        else
        {
            GetComponent<SpriteRenderer>().sprite = OddSprite;
        }
    }
    
    private void OnMouseDown()
    {
        Board cb = GameObject.Find("BoardState").GetComponent<Board>();
        if (cb.active_movers.Count > 0)
        {
            cb.dest_movers();
        }
    }
}
