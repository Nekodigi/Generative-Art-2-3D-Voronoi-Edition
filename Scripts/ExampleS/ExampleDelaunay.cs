using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExampleDelaunay
{
    int numVertices = 30;
    float size = 5.0f;
    public Delaunay delaunay;
    public List<Vertex> vertices = new List<Vertex>();
    public List<Polygon> polygons = new List<Polygon>();
    int seed = 0;
    public ExampleDelaunay(int dim)
    {
        //Random.RangeSeed(seed);
        for (int i = 0; i < numVertices; i++)
        {
            switch (dim)
            {
                case 2:
                    vertices.Add(new Vertex(0, Random.Range(-size, size), Random.Range(-size, size)));//id will be assigned later
                    break;
                case 3:
                    vertices.Add(new Vertex(0, Random.Range(-size, size), Random.Range(-size, size), Random.Range(-size, size)));//id will be assigned later
                    break;
            }
        }
        int count = 0;
        foreach(Vertex v in vertices)
        {
            v.id = count++;
        }
        

        delaunay = new Delaunay(dim);
        delaunay.Generate(vertices);
        polygons = HVUtils.simplex2Poly(delaunay.simplexes);
    }

    public void show()
    {
        foreach (Vertex vertex in vertices)
        {
            GeomRender.point(vertex.pos);
        }
        GeomRender.point(delaunay.centroid);
        foreach (Polygon polygon in polygons)
        {
            polygon.show();
        }
    }
}
