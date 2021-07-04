using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Delaunay
{
    public int dim;
    public List<Vertex> vertices = new List<Vertex>();
    public List<Simplex> simplexes = new List<Simplex>();
    public List<Polygon> polygons = new List<Polygon>();
    public float[] centroid;
    public ConvexHull hull;//3D convex hull shadow is 2D delaunay triangulation
    public Delaunay(int dim)
    {
        this.dim = dim;
        hull = new ConvexHull(dim + 1);
    }

    public void Generate(List<Vertex> input)
    {
        if (input.Count <= dim + 1) return;

        for (int i = 0; i < input.Count; i++)
        {
            float lenSq = FVector.sqrMag(input[i].pos);

            input[i].pos = FVector.append(input[i].pos, lenSq);
        }

        hull.Generate(input);//input reference are kept
        centroid = FVector.resize(hull.centroid, dim);

        for (int i = 0; i < input.Count; i++)
        {
            input[i].pos = FVector.resize(input[i].pos, dim);
        }

        vertices = input;

        for (int i = 0; i < hull.simplexes.Count; i++)
        {
            Simplex simplex = hull.simplexes[i];
            if (simplex.normal[dim] >= 0)
            {//delete simplex reference
                for (int j = 0; j < simplex.adjacent.Length; j++)
                {
                    if (simplex.adjacent[j] != null)
                    {
                        simplex.adjacent[j] = null;
                    }
                }
            }
            else
            {//select valid face
                simplexes.Add(simplex);
            }
        }
        
        polygons = HVUtils.simplex2Poly(simplexes);
    }

    public void Gen3DModel()//dim must be 3
    {
        foreach (Simplex simplex in simplexes)
        {
            simplex.Generate3D();
        }
    }

    public void Release3D()
    {
        foreach(Simplex simplex in simplexes)
        {
            simplex.Release();
        }
    }

    public void show(bool doOffset = false)
    {
        foreach (Vertex vertex in vertices)
        {
            //GeomRender.point(vertex.pos);
        }
        GeomRender.point(centroid);
        foreach (Polygon polygon in polygons)
        {
            polygon.show(doOffset);
        }
    }

    public void show(int[,] binaryCanvas)
    {
        float scale = binaryCanvas.GetLength(1) / 5.0f / 2.0f;
        foreach (Polygon polygon in polygons)
        {
            polygon.simplex.calcCentroid();
            int i = Mathf.FloorToInt((polygon.simplex.centroid[0] + 10.0f) * scale);
            int j = Mathf.FloorToInt((polygon.simplex.centroid[1] + 5.0f) * scale);
            if (binaryCanvas[i, j] == 0)
            {
                polygon.show();
            }
        }
    }

    public void toGraph()
    {
        foreach (Simplex s in simplexes)
        {
            s.toGraph();
        }
    }

    public void toGraph(int[,] binaryCanvas)
    {
        float scale = binaryCanvas.GetLength(1) / 5.0f / 2.0f;
        foreach (Simplex s in simplexes)
        {
            s.calcCentroid();
            int i = Mathf.FloorToInt((s.centroid[0] + 10.0f) * scale);
            int j = Mathf.FloorToInt((s.centroid[1] + 5.0f) * scale);
            if (binaryCanvas[i, j] == 0)
            {
                s.toGraph();
            }
        }
    }
}
