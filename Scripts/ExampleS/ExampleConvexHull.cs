using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExampleConvexHull
{
    int numVertices = 100;
    float size = 4.0f;
    public ConvexHull hull;
    public List<Vertex> vertices = new List<Vertex>();
    int seed = 0;

    public ExampleConvexHull(int dim)
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
                case 4:
                    vertices.Add(new Vertex(0, Random.Range(-size, size), Random.Range(-size, size), Random.Range(-size, size), Random.Range(-size, size)));//id will be assigned later
                    break;
            }
        }

        hull = new ConvexHull(dim);
        hull.Generate(vertices);
    }
}
