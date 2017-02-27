using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoverScript : MonoBehaviour {
    public GameObject moving;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
    public void init(GameObject to_move)
    {
        moving = to_move;
    }
    private void OnMouseDown()
    {
        Board cb = GameObject.Find("BoardState").GetComponent<Board>();
        Vector2 pos = GetComponent<Transform>().position;
        cb.move(moving.GetComponent<Transform>().position, pos);
    }
}
