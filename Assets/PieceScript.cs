using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PieceScript : MonoBehaviour {
    public string Notation;
    public bool royal=false;
    public Piece piece;
    public GameObject promotion = null;
    public double value;
    private string pname;
    public bool spawnable=true;
    public bool deathtouch = false;
    public PScript selfmove;
	void Awake () {
        piece = get_piece();
        pname = gameObject.name.Substring(0, gameObject.name.Length - 7);
        GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Pieces/White/" + pname);
    }

    // Update is called once per frame
    void Update () {

	}
    public void blacken()
    {
        piece.side = 2;
        GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Pieces/Black/" + pname);
    }
    private void OnMouseDown()
    {
        Board cb = GameObject.Find("BoardState").GetComponent<Board>();
        if (cb.turn == piece.side && !cb.over)
        {
            if (cb.active_movers.Count > 0)
            {
                cb.dest_movers();
            }
            else
            {
                cb.ShowMoves(piece, gameObject, GetComponent<Transform>().position);
            }
        }
    }
    public Piece get_piece(bool blacken=false)
    {
        Piece p = new Piece(Notation, this);
        if (blacken)
        {
            p.side = 2;
        }
        return p;
    }
}
