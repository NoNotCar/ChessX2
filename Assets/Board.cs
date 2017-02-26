using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Board : MonoBehaviour {
    public int height;
    public int width;
    public GameObject squares;
    public GameObject WhiteRow;
    public GameObject BlackRow;
    public GameObject pawns;
    public GameObject mover;
    public Vector2[] voids;
    private GameObject canmove;
    public BoardState bs;
    public int turn = 1;
    public bool over = false;
    private List<GameObject> active_movers = new List<GameObject>();
    private List<GameObject> active_info = new List<GameObject>();
    private int screen_width;
    // Use this for initialization
    void Start()
    {
        var sc = GetComponent<ShapeCard>();
        voids = sc == null ? new Vector2[0] : sc.shape(width, height);
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (!voids.Contains(new Vector2(x, y))){
                    Instantiate(squares, new Vector2(x, y), Quaternion.identity);
                }
            }
        }
        bs = new BoardState(width, height, this);
        foreach(var vpos in voids)
        {
            bs.spawn(vpos, new Block());
        }
        for (int x = 0; x < width; x++)
        {
            Spawn(WhiteRow.GetComponent<Row>().backfabs[x], new Vector2(x, 0));
            Spawn(BlackRow.GetComponent<Row>().backfabs[x], new Vector2(x, height-1), 2);
            Spawn(pawns, new Vector2(x, 1));
            Spawn(pawns, new Vector2(x, height - 2),2);
        }
        GameObject.Find("Camera").GetComponent<Transform>().position = new Vector3((float)(width/2.0-0.5), (float)(height / 2.0 - 0.5), -10);
        Fit_Camera();
        screen_width = Screen.width;
        check_mate();
        canmove = (GameObject)Resources.Load("CanMove");

    }
	// Update is called once per frame
	void Update () {
        if (Screen.width != screen_width)
        {
            screen_width = Screen.width;
            Fit_Camera();
        }if (Input.GetKeyDown(KeyCode.Escape))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("Menu");
        }
    }
    public void ShowMoves(Piece p, GameObject mpre, Vector2 pos)
    {
        if (active_movers.Count > 0)
        {
            dest_movers();
        }
        foreach (Vector2 v in p.moves(pos, bs))
        {
            GameObject m = Instantiate(mover, new Vector3(v.x, v.y, -2), Quaternion.identity);
            m.GetComponent<MoverScript>().init(mpre);
            active_movers.Add(m);
        }
    }
    public void move(Vector2 start,Vector2 end)
    {
        dest_movers();
        dest_info();
        Piece mp = bs[start];
        Piece cp = bs[end];
        if (cp != null && cp.side!=turn)
        {
            Destroy(cp.script.gameObject);
        } else if (cp != null)
        {
            cp.script.gameObject.GetComponent<Transform>().position = new Vector3(start.x,start.y,-1);
        }
        bs.move(start, end);
        if (bs[end] == null)
        {
            Destroy(mp.script.gameObject);
        }
        turn = 3-turn;
        check_mate();
    }
    private void dest_movers()
    {
        foreach (GameObject g in active_movers)
        {
            Destroy(g);
        }
        active_movers.Clear();
    }
    private void dest_info()
    {
        foreach (GameObject g in active_info)
        {
            Destroy(g);
        }
        active_info.Clear();
    }
    public void Spawn(GameObject pfab, Vector2 pos, int side=1)
    {
        if (voids.Contains(pos)) { return; }
        GameObject np = Instantiate(pfab, new Vector3(pos.x, pos.y, -1), side==2 ? Quaternion.AngleAxis(180, Vector3.forward) : Quaternion.identity);
        bs.spawn(pos,np.GetComponent<PieceScript>().piece);
        if (side==2)
        {
            np.GetComponent<PieceScript>().blacken();
        }
    }
    private void Fit_Camera()
    {
        Camera cam = GameObject.Find("Camera").GetComponent<Camera>();
        float wh = (width / 2.0f + 0.5f) * Screen.height / Screen.width;
        float vh = height / 2.0f + 0.5f;
        cam.orthographicSize = wh > vh ? wh : vh;
    }
    private void check_mate()
    {
        if (bs.is_mate(turn))
        {
            over = true;
            GameObject.Find("EndCanvas").GetComponent<EndMessage>().ShowMessage(bs.is_check(turn) ? "CHECKMATE" : "STALEMATE");
        }
        add_c_moves();
    }
    private void add_c_moves()
    {
        if (bs.is_check(turn))
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    var pos = new Vector2(x, y);
                    var p = bs[pos];
                    if (p != null && p.side==turn && p.moves(pos, bs).Length > 0)
                    {
                        active_info.Add(Instantiate(canmove, new Vector3(pos.x, pos.y, -2), Quaternion.identity));
                    }
                }
            }
        }
    }
}

