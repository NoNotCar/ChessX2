using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Donut : ShapeCard {
    public int size;
    public override Vector2[] shape(int w, int h)
    {
        var voids = new Vector2[(int)Math.Pow(size * 2 + 1, 2)];
        var centre = new Vector2((w - 1) / 2, (h - 1) / 2);
        for(var n=0;n<Math.Pow(size * 2 + 1, 2); n++)
        {
            voids[n] = centre + new Vector2(-size + n % (size * 2 + 1), -size + n / (size * 2 + 1));
        }
        return voids;
    }
}
