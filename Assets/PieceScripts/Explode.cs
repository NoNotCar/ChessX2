using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explode : PScript {
    int radius = 1;
    public override void execute(Vector2 ppos, BoardState b)
    {
        for(int x =(int)ppos.x-radius;x<=(int)ppos.x + radius; x++)
        {
            for (int y = (int)ppos.y - radius; y <= (int)ppos.y + radius; y++)
            {
                var pos = new Vector2(x, y);
                if (b.in_board(pos)) {
                    b.Destroy(pos);
                }
            }
        }
    }
}
