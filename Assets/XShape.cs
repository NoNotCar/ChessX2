using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XShape : ShapeCard {
    public int size;
    public override Vector2[] shape(int w, int h)
    {
        var loci = new Vector2[] { new Vector2(-0.5f, (h-1) / 2f), new Vector2(w-0.5f, (h - 1) / 2f), new Vector2((w - 1) / 2f, -0.5f), new Vector2((w - 1) / 2f, h-0.5f) };
        var lvoids = new List<Vector2>();
        for (int x = 0; x < w; x++)
        {
            for(int y=0;y<h; y++)
            {
                foreach (var l in loci)
                {
                    if(Math.Abs(x-l.x)+ Math.Abs(y - l.y) < size)
                    {
                        lvoids.Add(new Vector2(x, y));
                    }
                }
            }
        }
        return lvoids.ToArray();
    }
}
