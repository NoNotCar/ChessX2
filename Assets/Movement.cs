using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class BoardState : object
{
    public Piece[,] board;
    public Piece lastmoved;
    public int height;
    public int width;
    public int gravity;
    private int[] royals = new int[2] { 0, 0 };
    public Board bscript;
    public BoardState(int w, int h, Board b = null)
    {
        board = new Piece[w, h];
        height = h;
        width = w;
        if (b != null)
        {
            bscript = b;
            gravity = b.gravity;
        }
    }
    public bool in_board(Vector2 pos)
    {
        return 0 <= pos.x && pos.x <= width-1 && 0 <= pos.y && pos.y <= height-1;
    }
    public int blocklevel(Vector2 pos, int side)
    {
        if (!in_board(pos))
        {
            return 3;
        }
        Piece p = this[pos];
        if (p == null)
        {
            return 0;
        }else if (p.indestructible)
        {
            return 3;
        }
        return p.side == side ? 2 : 1;
    }
    public Piece this[Vector2 pos]
    {
        get
        {
            try
            {
                /*/if ((int)Math.Round(pos.x)!=(int)pos.x|| (int)Math.Round(pos.y) != pos.y) {
                    throw new Exception("VECTOR NOT INT");
                }/*/
                return board[(int)pos.x, (int)pos.y];
            }
            catch (IndexOutOfRangeException ball)
            {
                Debug.Log(pos);
                throw ball;
            }
        }
        set
        {
            try
            {
                board[(int)pos.x, (int)pos.y] = value; ;
            }
            catch (IndexOutOfRangeException ball)
            {
                Debug.Log(pos);
                throw ball;
            }
        }
    }
    public void spawn(Vector2 pos, Piece p)
    {
        this[pos] = p;
        if (bscript!=null && p.script != null && p.script.royal)
        {
            royals[p.side-1] += 1;
        }
    }
    public bool defeat(int side)
    {
        int rs = 0;
        foreach (Piece p in board)
        {
            if (p!=null && p.side==side && p.script.royal)
            {
                rs += 1;
            }
        }
        return rs < royals[side-1];
    }
    public BoardState copy()
    {
        BoardState b = new BoardState(width, height);
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2 pos = new Vector2(x, y);
                Piece p = this[pos];
                if (p != null)
                {
                    b.spawn(pos, p);
                }
            }
        }
        b.royals = royals;
        b.gravity = gravity;
        return b;
    }
    public BoardState[] extrapolate(int side, bool nocheck=true)
    {
        List<BoardState> bss = new List<BoardState>();
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++)
            {
                Vector2 pos = new Vector2(x, y);
                Piece p = this[pos];
                if (p != null && p.side == side)
                {
                    foreach (Vector2 mpos in p.moves(pos, this, nocheck))
                    {
                        BoardState nb = copy();
                        nb.move(pos, mpos);
                        bss.Add(nb);
                    }
                }
            }
        }
        return bss.ToArray();
    }
    public void move(Vector2 start, Vector2 end, bool chain=false)
    {
        Piece mp = this[start];
        Piece cp = this[end];
        if (!chain) { lastmoved = mp; }
        if (cp!=null && cp.side == mp.side)
        {
            this[start] = cp;
            this[end] = mp;
            if (bscript != null)
            {
                cp.script.gameObject.GetComponent<Transform>().position = new Vector3(start.x, start.y, -1);
                mp.script.gameObject.GetComponent<Transform>().position = new Vector3(end.x, end.y, -1);
            }
        }
        else if (cp!=null && cp.script.deathtouch)
        {
            this[start] = null;
            this[end] = null;
        }else
        {
            this[start] = null;
            this[end] = mp;
            if (bscript != null)
            {
                mp.script.gameObject.GetComponent<Transform>().position = new Vector3(end.x, end.y, -1);
            }
        }
        if (this[start] != cp && bscript!=null)
        {
            UnityEngine.Object.Destroy(cp.script.gameObject);
        }
        if (this[end] != mp)
        {
            if (bscript != null)
            {
                UnityEngine.Object.Destroy(mp.script.gameObject);
            }
            return;
        }
        if (mp.script.promotion != null && (mp.side==2?end.y==0:end.y==height-1))
        {
            if (bscript == null)
            {
                this[end] = mp.script.promotion.GetComponent<PieceScript>().get_piece();
            }else
            {
                UnityEngine.Object.Destroy(mp.script.gameObject);
                bscript.Spawn(mp.script.promotion, end, mp.side);
            }
        }
        if (gravity == 1 && !chain)
        {
            foreach (Vector2 uv in new Vector2[] { Vector2.up, Vector2.right, Vector2.down, Vector2.left })
            {
                var spos = end+uv;
                while (blocklevel(spos, 0) == 0)
                {
                    spos += uv;
                }
                if(blocklevel(spos, 0) == 3) { continue; }
                var p = this[spos];
                if (p!=null && spos != end + uv)
                {
                    move(spos, end + uv, true);
                }
            }
        }
    }
    public bool is_mate(int side)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2 pos = new Vector2(x, y);
                Piece p = this[pos];
                if (p != null && p.side == side)
                {
                    foreach (Vector2 mpos in p.moves(pos, this, false))
                    {
                        return false;
                    }
                }
            }
        }
        return true;
    }
    public bool is_check(int side)
    {
        return extrapolate(3-side).Any(b => b.defeat(side));
    }
    public IEnumerable<Vector2> poss()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                yield return new Vector2(x, y);
            }
        }
    }
}
public class Piece : object
{
    public Atom[] movement;
    public PieceScript script;
    public int side=1;
    public bool indestructible = false;
    public Piece(string notation="",PieceScript ps=null)
    {
        movement = MParser.parse(notation);
        script = ps;
    }
    public Vector2[] moves(Vector2 pos, BoardState b, bool nocheck=false)
    {
        List<Vector2> moves = new List<Vector2>();
        foreach (Atom a in movement)
        {
            foreach (Vector2 m in a.moves(pos, pos, b, side))
            {
                BoardState nbs = b.copy();
                nbs.move(pos, m + pos);
                if (nocheck|| !nbs.extrapolate(3-side).Any(e => e.defeat(side)))
                {
                    moves.Add(m+pos);
                }
            }
        }
        moves.DeDuplicate();
        return moves.ToArray();
    }
}
public class Block: Piece
{
    public Block(): base()
    {
        indestructible = true;
        side = 0;
    }
}
abstract public class Atom: object
{
    public int? limit;
    abstract public Vector2[] moves(Vector2 pos, Vector2 opos, BoardState b, int side);
    public Vector2 Rot_V(Vector2 v, int n)
    {
        for (int i=0; i<n; i++)
        {
            v = new Vector2(v.y, -v.x);
        }
        return v;
    }
}
public class Jumper: Atom
{
    public Vector2[] offsets;
    public Jumper(Vector2 offset)
    {
        List<Vector2> offl = new List<Vector2>();
        Vector2 reflect;
        if (offset.x != 0 && offset.y != 0 && Math.Abs(offset.x) != Math.Abs(offset.y))
        {
            reflect = Vector2.Reflect(offset, Vector2.up);
        }
        else
        {
            reflect = Vector2.zero;
        }
        for (int i = 0; i < 4; i++)
        {
            offl.Add(Rot_V(offset, i));
            if (reflect != Vector2.zero)
            {
                offl.Add(Rot_V(reflect, i));
            }
        }
        offsets = offl.ToArray();
    }
    public override Vector2[] moves(Vector2 pos, Vector2 opos, BoardState b, int side)
    {
        List<Vector2> ms = new List<Vector2>();
        foreach (Vector2 v in offsets)
        {
            if (b.blocklevel(pos + v,side) < 2)
            {
                ms.Add(v);
            }
        }
        return ms.ToArray();
    }
}
public class Rider : Jumper
{
    public Rider(Vector2 offset) : base(offset) { }
    public override Vector2[] moves(Vector2 pos, Vector2 opos, BoardState b, int side)
    {
        List<Vector2> l = new List<Vector2>();
        foreach (Vector2 o in offsets)
        {
            Vector2 cv = Vector2.zero;
            for (int i=1; ;i++)
            {
                cv += o;
                int bl = b.blocklevel(pos + cv, side);
                if (bl < 2)
                {
                    l.Add(cv);
                }if (bl>0||(limit!=null&&limit==i)) {
                    break;
                }
            }
        }
        return l.ToArray();
    }
}
public class SRider: Jumper
{
    bool ranged;
    public SRider(Vector2 offset, bool r): base(offset)
    {
        ranged = r;
    }
    public override Vector2[] moves(Vector2 pos, Vector2 opos, BoardState b, int side)
    {
        return recursive_f(new List<Vector2> {Vector2.zero},pos,b,side).ToArray();
    }
    private List<Vector2> recursive_f(List<Vector2> cpath,Vector2 pos, BoardState b, int side)
    {
        if (cpath.Count() == limit+1)
        {
            return new List<Vector2>() { cpath.Last() };
        }
        var moves = new List<Vector2>();
        var c = cpath.Last();
        foreach (var v in offsets)
        {
            if (!cpath.Contains(v + c))
            {
                var bl = b.blocklevel(v + c + pos, side);
                if (bl == 0)
                {
                    var ncpath = new List<Vector2>(cpath);
                    ncpath.Add(v + c);
                    moves.AddRange(recursive_f(ncpath, pos, b, side));
                    if (ranged && cpath.Count()<limit)
                    {
                        moves.Add(v + c);
                    }
                }
                else if (bl == 1 && (cpath.Count() == limit || ranged))
                {
                    moves.Add(v + c);
                }
            }
        }
        return moves;
    }

}
public class Mime : Atom
{
    public override Vector2[] moves(Vector2 pos, Vector2 opos, BoardState b, int side)
    {
        if (b.lastmoved == null || b.lastmoved.side == side)
        {
            return new Vector2[0];
        }
        var moves = new List<Vector2>();
        foreach (var m in b.lastmoved.movement)
        {
            if (m.GetType() != GetType())
            {
                moves.AddRange(m.moves(pos, opos, b, side));
            }
        }
        return moves.ToArray();
    }
}
public class Swap : Atom
{
    public override Vector2[] moves(Vector2 pos, Vector2 opos, BoardState b, int side)
    {
        var moves = new List<Vector2>();
        foreach (var p in b.poss())
        {
            if (b.blocklevel(p,side) == 2 && p!=pos)
            {
                moves.Add(p - pos);
            }
        }
        return moves.ToArray();
    }
}
public class CopyCat : Modified
{
    public CopyCat(Atom a) : base(a) { }
    public override Vector2[] moves(Vector2 pos, Vector2 opos, BoardState b, int side)
    {
        var moves = new List<Vector2>();
        foreach (var v in ((Jumper)atom).offsets)
        {
            var bl = b.blocklevel(pos + v, side);
            if (bl==1||bl==2)
            {
                var p = b[v + pos];
                foreach (var m in p.movement)
                {
                    if (m.GetType() != GetType())
                    {
                        moves.AddRange(m.moves(pos, opos, b, side));
                    }
                }
            }
        }
        return moves.ToArray();
    }
}
public abstract class Modified: Atom
{
    public Atom atom;
    public Modified(Atom a)
    {
        atom = a;
    }
    public override Vector2[] moves(Vector2 pos, Vector2 opos, BoardState b, int side)
    {
        if (shortcircuit(pos, opos, b, side))
        {
            return new Vector2[0];
        }
        List<Vector2> nmoves = new List<Vector2>();
        foreach (Vector2 m in atom.moves(pos, opos, b, side))
        {
            if (valid(pos, opos, m, b, side)){
                nmoves.Add(m);
            }
        }
        return nmoves.ToArray();
    }
    public virtual bool shortcircuit(Vector2 pos, Vector2 opos, BoardState b, int side)
    {
        return false;
    }
    public virtual bool valid(Vector2 pos, Vector2 opos, Vector2 move, BoardState b, int side)
    {
        return true;
    }
}
class Forward: Modified
{
    bool reverse;
    public Forward(Atom a,bool r): base(a) { reverse = r; }
    public override bool valid(Vector2 pos, Vector2 opos, Vector2 move, BoardState b, int side)
    {
        return side==(reverse?1:2) ? move.y < 0 : move.y > 0;
    }
}
class Pawn: Modified
{
    public Pawn(Atom a): base(a) { }
    public override bool shortcircuit(Vector2 pos, Vector2 opos, BoardState b, int side)
    {
        return side==2 ? pos.y != b.height - 2 : pos.y != 1;
    }
}
class ModBL: Modified
{
    private int bl;
    public ModBL(Atom a, int forcebl): base(a)
    {
        bl = forcebl;
    }
    public override bool valid(Vector2 pos, Vector2 opos, Vector2 move, BoardState b, int side)
    {
        return b.blocklevel(pos + move, side) == bl;
    }
}
class TwixtChecker: Modified
{
    Func<int, bool> blcheck;
    public TwixtChecker(Atom a,Func<int,bool> check): base(a) { blcheck = check; }
    public override bool valid(Vector2 pos, Vector2 opos, Vector2 move, BoardState b, int side)
    {
        if (move.x==0 || move.y==0 || Math.Abs(move.x)== Math.Abs(move.y))
        {
            var bm = Math.Abs(move.x) > Math.Abs(move.y) ? Math.Abs(move.x) : Math.Abs(move.y);
            for (int n = 1; n < bm; n++)
            {
                if (blcheck(b.blocklevel(pos + (move / bm * n), side)))
                {
                    return false;
                }
            }
        }
        return true;
    }
}
class Outward : Modified
{
    public Outward(Atom a): base(a) { }
    public override bool valid(Vector2 pos, Vector2 opos, Vector2 move, BoardState b, int side)
    {
        return Vector2.Angle(move, opos-pos) >= 90;
    }
}
class LR: Modified
{
    bool r;
    public LR(Atom a, bool right): base(a)
    {
        r = right;
    }
    public override bool valid(Vector2 pos, Vector2 opos, Vector2 move, BoardState b, int side)
    {
        var ang = Vector2.Angle(move, opos - pos);
        return (Vector3.Cross(move, opos - pos).z < 0) == r && 0 < ang && ang < 180;
    }
}
class HV: Modified
{
    bool h;
    public HV(Atom a, bool hoz): base(a)
    {
        h = hoz;
    }
    public override bool valid(Vector2 pos, Vector2 opos, Vector2 move, BoardState b, int side)
    {
        return h ? Math.Abs(move.x) > Math.Abs(move.y) : Math.Abs(move.x) < Math.Abs(move.y);
    }
}
class Combo : Atom
{
    private Atom[] starts;
    private Atom[] ends;
    public Combo(Atom[] s, Atom[] e){
        starts=s;
        ends=e;
    }
    public override Vector2[] moves(Vector2 pos, Vector2 opos, BoardState b, int side)
    {
        List<Vector2> mvs = new List<Vector2>();
        foreach (Atom s in starts)
        {
            foreach (Vector2 mv in s.moves(pos, opos, b, side))
            {
                mvs.Add(mv);
                if (b.blocklevel(mv + pos,side) == 0)
                {
                    foreach (Atom e in ends)
                    {
                        foreach (Vector2 nvm in e.moves(pos + mv, pos, b, side))
                        {
                            mvs.Add(nvm + mv);
                        }
                    }
                }
            }
        }
        return mvs.ToArray();
    }
}
class MParser : object
{
    static Dictionary<string, Func<Atom>> atoms = new Dictionary<string, Func<Atom>>()
    {
        {"F",()=>new Jumper(Vector2.one)},
        {"W",()=>new Jumper(Vector2.up)},
        {"N",()=>new Jumper(new Vector2(1,2))},
        {"D",()=>new Jumper(new Vector2(0,2))},
        {"A",()=>new Jumper(new Vector2(2,2))},
        {"H",()=>new Jumper(new Vector2(0,3))},
        {"L",()=>new Jumper(new Vector2(1,3))},
        {"J",()=>new Jumper(new Vector2(2,3))},
        {"G",()=>new Jumper(new Vector2(3,3))},
        {"R",()=>new Rider(Vector2.up)},
        {"B",()=>new Rider(Vector2.one)},
        {"NN",()=>new Rider(new Vector2(1,2))},
        {"DD",()=>new Rider(new Vector2(0,2))},
        {"AA",()=>new Rider(new Vector2(2,2))},
        {"M", ()=>new Mime()},
        {"SW",()=> new Swap()},
        {"S",()=> new SRider(Vector2.up,false) },
        {"RR",()=> new SRider(Vector2.up,true) }
    };
    static Dictionary<string, Func<Atom, Modified>> mods = new Dictionary<string, Func<Atom, Modified>>()
    {
        {"f",a=>new Forward(a,false)},
        {"b",a=>new Forward(a,true)},
        {"p",a=>new Pawn(a) },
        {"ll",a=>new TwixtChecker(a,b=>b!=0) },
        {"j",a=>new TwixtChecker(a,b=>(b==0||b==3)) },
        {"o",a=>new Outward(a) },
        {"m",a=>new ModBL(a,0) },
        {"a",a=>new ModBL(a,1) },
        {"l",a=>new LR(a,false) },
        {"r",a=>new LR(a,true) },
        {"v",a=>new HV(a,false) },
        {"h",a=>new HV(a,true) },
        {"cc", a=> new CopyCat(a) }
    };
    public static Atom[] parse(string s)
    {
        if (s.Contains("-")){
            String[] cs = s.Split('-');
            return new Atom[1] { new Combo(parse(cs[0]), parse(cs[1])) };
        }
        char[] chars = s.ToCharArray();
        List<Atom> catoms=new List<Atom>();
        List<Func<Atom,Modified>> cmods = new List<Func<Atom, Modified>> ();
        for (int i=0;i<chars.Length;i++)
        {
            string sc = chars[i].ToString();
            string dc = i == chars.Length - 1 ? "" : sc + chars[i + 1].ToString();
            foreach (string tc in new string[2] { dc,sc}){
                if (atoms.ContainsKey(tc))
                {
                    if (tc == dc) { i++; }
                    Atom to_add = atoms[tc]();
                    string num = "";
                    while (i<chars.Length-1)
                    {
                        var c = chars[i + 1].ToString();
                        if ("0123456789".Contains(c))
                        {
                            num += c;
                            i++;
                        }else
                        {
                            break;
                        }
                    }
                    if (num != "")
                    {
                        to_add.limit = int.Parse(num);
                    }
                    cmods.Reverse();
                    foreach (Func<Atom, Modified> m in cmods)
                    {
                        to_add = m(to_add);
                    }
                    cmods.Clear();
                    catoms.Add(to_add);
                    break;
                }
                else if (mods.ContainsKey(tc))
                {
                    cmods.Add(mods[tc]);
                    if (tc == dc) { i++; }
                    break;
                }
            }
        }
        return catoms.ToArray();
    }
}