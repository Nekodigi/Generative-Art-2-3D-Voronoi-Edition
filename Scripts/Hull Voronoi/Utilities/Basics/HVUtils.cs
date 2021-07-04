using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class HVUtils : MonoBehaviour
{
    public static float EPSILON = 0.0001f;

    public static float Det2(float x1, float x2, float y1, float y2)
    {
        return (x1 * y2 - y1 * x2);
    }

    public static float[][] extractPos(params Vertex[] vertices)
    {
        float[][] result = new float[vertices.Length][];
        for (int i = 0; i < vertices.Length; i++)
        {
            result[i] = vertices[i].pos;
        }
        return result;
    }

    public static List<Vertex> getNotIth(List<Vertex> vs, int i)
    {
        List<Vertex> result = new List<Vertex>();
        for (int j = 0; j < vs.Count; j++)
        {
            if (j == i) { continue; }
            result.Add(vs[j]);
        }
        return result;
    }

    public static Simplex[] getAdjHasVertex(Simplex simplex, Vertex v)
    {
        Simplex[] result = new Simplex[simplex.adjacent.Length - 1];
        for (int j = 0, k = 0; j < simplex.adjacent.Length; j++)
        {
            if (simplex.adjacent[j] != null && hasItem(v, simplex.adjacent[j].vertices))
            {
                result[k++] = simplex.adjacent[j];
            }
            else continue;
        }
        return result;
    }



    public static bool hasItem(Vertex v, Vertex[] vertices)
    {
        foreach (Vertex v_ in vertices)
        {
            if (v_ == v) return true;
        }
        return false;
    }

    public static bool hasItem(Vertex v, List<Vertex> vertices)
    {
        return hasItem(v, vertices.ToArray());
    }

    public static float[] sphereSampling(float u, float theta)
    {//https://mathworld.wolfram.com/SpherePointPicking.html
        float x = Mathf.Sqrt(1 - u * u) * Mathf.Cos(theta);
        float y = Mathf.Sqrt(1 - u * u) * Mathf.Sin(theta);
        float[] result = { x, y, u };
        return result;
    }

    public static List<Polygon> simplex2Poly(List<Simplex> simplexes)
    {
        List<Polygon> result = new List<Polygon>();
        foreach (Simplex simplex in simplexes)
        {
            result.Add(new Polygon(simplex));
        }
        return result;
    }

    public static bool contains(List<int> ids, int id)
    {
        foreach (int i in ids)
        {
            if (id == i) return true;
        }
        return false;
    }

    public static bool contains(Vertex[] vertices, Vertex vertex)
    {
        for (int i = 0; i < vertices.Length; i++)
        {
            if (vertices[i] == vertex) return true;
        }
        return false;
    }

    public static float lineDist(Vector3 c, Vector3 a, Vector3 b)
    {
        Vector3 ap = c - a;
        Vector3 ab = b - a;
        float l = Vector3.Distance(a, b);
        float scala = Vector3.Dot(ap, ab.normalized);
        if (scala <= 0)
        {
            return Vector3.Distance(c, a);
        }
        else if (scala >= l)
        {
            return Vector3.Distance(c, b);
        }
        else
        {
            ab *= scala;
            Vector3 normalPoint = a + ab;
            return Vector3.Distance(c, normalPoint);
        }
    }

    // Is a point intersecting with a polygon?
    //
    //The list describing the polygon has to be sorted either clockwise or counter-clockwise because we have to identify its edges
    //TODO: May sometimes fail because of floating point precision issues
    public static bool PointPolygon(List<Vector3> polygonPoints, Vector3 point)
    {
        //Step 1. Find a point outside of the polygon
        //Pick a point with a x position larger than the polygons max x position, which is always outside
        Vector3 maxXPosVertex = polygonPoints[0];

        for (int i = 1; i < polygonPoints.Count; i++)
        {
            if (polygonPoints[i].x > maxXPosVertex.x)
            {
                maxXPosVertex = polygonPoints[i];
            }
        }

        //The point should be outside so just pick a number to move it outside
        //Should also move it up a little to minimize floating point precision issues
        //This is where it fails if this line is exactly on a vertex
        Vector3 pointOutside = maxXPosVertex + new Vector3(1f, 0.01f);


        //Step 2. Create an edge between the point we want to test with the point thats outside
        Vector3 l1_p1 = point;
        Vector3 l1_p2 = pointOutside;

        //Debug.DrawLine(l1_p1.XYZ(), l1_p2.XYZ());


        //Step 3. Find out how many edges of the polygon this edge is intersecting with
        int numberOfIntersections = 0;

        for (int i = 0; i < polygonPoints.Count; i++)
        {
            //Line 2
            Vector3 l2_p1 = polygonPoints[i];

            int iPlusOne = ClampListIndex(i + 1, polygonPoints.Count);

            Vector3 l2_p2 = polygonPoints[iPlusOne];

            //Are the lines intersecting?
            if (_Intersections.LineLine(l1_p1, l1_p2, l2_p1, l2_p2, true))
            {
                numberOfIntersections += 1;
            }
        }


        //Step 4. Is the point inside or outside?
        bool isInside = true;

        //The point is outside the polygon if number of intersections is even or 0
        if (numberOfIntersections == 0 || numberOfIntersections % 2 == 0)
        {
            isInside = false;
        }

        return isInside;
    }

    //Clamp list indices
    //Will even work if index is larger/smaller than listSize, so can loop multiple times
    public static int ClampListIndex(int index, int listSize)
    {
        index = ((index % listSize) + listSize) % listSize;

        return index;
    }

    public static Vector3 intersection(Vector3 p1s, Vector3 p1e, Vector3 p2s, Vector3 p2e)
    {
        float x1 = p1s.x; float y1 = p1s.y;
        float x2 = p1e.x; float y2 = p1e.y;
        float x3 = p2s.x; float y3 = p2s.y;
        float x4 = p2e.x; float y4 = p2e.y;
        float den = (x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4);
        if (den == 0)
        {
            return Vector3.zero;//use positive infinity instead of null
        }

        float t = ((x1 - x3) * (y3 - y4) - (y1 - y3) * (x3 - x4)) / den;
        float u = -((x1 - x2) * (y1 - y3) - (y1 - y2) * (x1 - x3)) / den;
        if (t > 0 && t < 1 && u > 0 && u < 1)
        {
            Vector3 pt = new Vector3();
            pt.x = x1 + t * (x2 - x1);
            pt.y = y1 + t * (y2 - y1);
            return pt;
        }
        else
        {
            return Vector3.zero;
        }
    }
}