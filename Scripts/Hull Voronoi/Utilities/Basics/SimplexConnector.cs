using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SimplexConnector
{
    public int[] vertices;//vertex indices.
    public int hashCode;//the hash code computed from indices.
    public Simplex face;
    public int edgeIndex;//the edge to be connected

    public SimplexConnector(Simplex face, int edgeIndex, int dim)
    {
        vertices = new int[dim - 1];
        this.face = face;
        this.edgeIndex = edgeIndex;
        hashCode = 31;

        for (int i = 0, c = 0; i < dim; i++)
        {
            if (i != edgeIndex)
            {
                int v = face.vertices[i].id;
                vertices[c++] = v;
                hashCode += (23 * hashCode + v);
            }
        }
        
        Array.Sort(vertices);

        hashCode = Mathf.Abs(hashCode);
    }

    public static bool areConnectable(SimplexConnector a, SimplexConnector b, int dim)
    {
        if (a.hashCode != b.hashCode) return false;

        int n = dim - 1;
        for (int i = 0; i < n; i++)
        {
            if (a.vertices[i] != b.vertices[i]) return false;
        }
        return true;
    }

    public static void connect(SimplexConnector a, SimplexConnector b)
    {
        a.face.adjacent[a.edgeIndex] = b.face;
        b.face.adjacent[b.edgeIndex] = a.face;
    }
}
