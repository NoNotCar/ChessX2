using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class RandomRow : Row
{
    public float maxvalue;
    public GameObject king;
    public int multi = 1;
    // Use this for initialization
    void Awake()
    {
        System.Random rnd = new System.Random(Random.Range(0, 100));
        List<GameObject> prefabs = new List<GameObject>();
        prefabs.AddRange(Resources.LoadAll<GameObject>("Piecefabs"));
        double max = 0;
        double min = 999;
        var bw = GameObject.Find("BoardState").GetComponent<Board>().width;
        var vp = new List<KeyValuePair<double, GameObject>>();
        var ToRemove = new List<GameObject>();
        foreach (GameObject p in prefabs)
        {
            var ps = p.GetComponent<PieceScript>();
            if (ps.royal || !ps.spawnable)
            {
                ToRemove.Add(p);
            }
            else
            {
                var v = ps.value;
                if (max < v) { max = v; }
                if (min > v) { min = v; }
                vp.Add(new KeyValuePair<double, GameObject>(v, p));
            }
        }
        prefabs.RemoveAll(p => ToRemove.Contains(p));
        vp.Shuffle(rnd);
        int tries = 0;
        while (true)
        {
            tries++;
            var cr = new List<GameObject>();
            double cv = 0;
            while (cr.Count < bw - 2)
            {
                var np = prefabs[rnd.Next(prefabs.Count)];
                for (int n = 0; n < multi && cr.Count < bw - 2; n++)
                {
                    cr.Add(np);
                    cv += np.GetComponent<PieceScript>().value;
                }
                if (cv > maxvalue - min)
                {
                    break;
                }
            }
            for (int n = 0; n < vp.Count; n++)
            {
                var ci = vp[n]; //LOL VPN
                if (ci.Key == maxvalue - cv)
                {
                    cr.Add(ci.Value);
                    cr.Add(king);
                    cr.Shuffle(rnd);
                    backfabs = cr.ToArray();
                    return;
                }
            }
            if (tries == 10000)
            {
                Debug.Log("FAILED TO GENERATE ROW");
                break;
            }
        }
    }
}
public static class ListModifier : object
{
    public static void Swap<T>(this IList<T> list, int i, int j)
    {
        var temp = list[i];
        list[i] = list[j];
        list[j] = temp;
    }
    public static void Shuffle<T>(this IList<T> list, System.Random rnd)
    {
        for (var i = 0; i < list.Count; i++)
        {
            list.Swap(i, rnd.Next(i, list.Count));
        }
    }
    public static void DeDuplicate<T>(this IList<T> list)
    {
        var seen = new List<T>();
        var remove = new List<T>();
        foreach (T item in list)
        {
            (seen.Contains(item) ? remove : seen).Add(item);
        }
        foreach (T item in remove)
        {
            list.Remove(item);
        }
    }
}
