using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquareScript : MonoBehaviour {
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
	
	// Update is called once per frame
	void Update () {
		
	}
}
