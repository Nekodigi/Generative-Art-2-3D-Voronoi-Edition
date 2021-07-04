using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utils : MonoBehaviour
{
    public static float map(float x, float xmin, float xmax, float ymin, float ymax)
    {//xmin<=x<=xmax -> ymin<=x<=ymax
        return (x - xmin) / (xmax - xmin) * (ymax - ymin) + ymin;
    }
}
