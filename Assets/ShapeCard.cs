using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShapeCard : MonoBehaviour {

	public virtual Vector2[] shape (int w, int h)
    {
        return new Vector2[0];
    }
}
