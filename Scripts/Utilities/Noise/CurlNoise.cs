using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurlNoise : MonoBehaviour
{
    public static OpenSimplexNoise snoise = new OpenSimplexNoise();
    public static Vector3 curlNoise(float x_, float y_, float z_, float woff)
    {
        float EPSILON = HVUtils.EPSILON;
        //PVector A = noiseVec(x_, y_, z_, woff);
        Vector3 px0 = noiseVec(x_ - EPSILON, y_, z_, woff);
        Vector3 px1 = noiseVec(x_ + EPSILON, y_, z_, woff);
        Vector3 py0 = noiseVec(x_, y_ - EPSILON, z_, woff);
        Vector3 py1 = noiseVec(x_, y_ + EPSILON, z_, woff);
        Vector3 pz0 = noiseVec(x_, y_, z_ - EPSILON, woff);
        Vector3 pz1 = noiseVec(x_, y_, z_ + EPSILON, woff);

        float x = (py1.z - py0.z) - (pz1.y - pz0.y);
        float y = (pz1.x - pz0.x) - (px1.z - px0.z);
        float z = (px1.y - px0.y) - (py1.x - py0.x);
        return new Vector3(x, y, z)/(EPSILON * 2);
    }

    public static Vector3 noiseVec(float x_, float y_, float z_, float woff)
    {
        float x = (float)snoise.eval(x_, y_, z_, woff);
        float y = (float)snoise.eval(x_ , y_ , z_, woff + 100);
        float z = (float)snoise.eval(x_ , y_ , z_, woff + 200);
        return new Vector3(x, y, z);
    }
}
