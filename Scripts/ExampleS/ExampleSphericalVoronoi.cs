using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExampleSphericalVoronoi
{
    public int numVertices = 300;
    public float size = 4.0f;    public SphericalVoronoi sVoronoi = new SphericalVoronoi();
    public List<Vertex> vertices = new List<Vertex>();
    public ConvexHull hull = new ConvexHull(3);
    int seed = 0;

    public ExampleSphericalVoronoi()
    {
        //Random.RangeSeed(seed);
        /*for (int i = 0; i < numVertices; i++)//99
        {
            vertices.Add(new Vertex(0, FVector.mult(HVUtils.sphereSampling(Random.Range(-1.0f, -0.99f), Random.Range(0, Mathf.PI*2)), size)));
        }
        for (int i = 0; i < 30; i++)
        {
            vertices.Add(new Vertex(0, FVector.mult(HVUtils.sphereSampling(Random.Range(-1.0f, 1.0f), Random.Range(0, Mathf.PI * 2)), size)));
        }*/
        for (int i = 0; i < numVertices; i++)
        {
            vertices.Add(new Vertex(0, FVector.mult(HVUtils.sphereSampling(Random.Range(-1.0f, 1.0f), Random.Range(0, Mathf.PI*2)), size)));
        }

        hull.Generate(vertices);
        sVoronoi.Generate(hull);
    }

    public void Generate()
    {
        float noiseS = 1;//noise scale
        foreach(Vertex v in vertices)
        {
            Vector3 vec = CurlNoise.curlNoise(v.pos[0]/noiseS, v.pos[1]/noiseS, v.pos[2]/noiseS, 0);
            v.pos = FVector.add(v.pos, FVector.setMag(FVector.set(vec), 0.05f));
            v.pos = FVector.setMag(v.pos, size);
        }
        hull.Generate(vertices);
        sVoronoi.Generate(hull);
        relax(0.5f);
        vertexRelax();
    }

    void vertexRelax()
    {
        
        for (int i = 0; i < vertices.Count; i++)
        {
            Vertex v = vertices[i];
            v.vertexRelax(vertices);
        }
    }

    public void relax(float fac)
    {
        foreach (Polygon polygon in sVoronoi.polygons)
        {
            polygon.relax(fac);
        }
        for (int i = vertices.Count - 1; i >= 0; i--)
        {
            Vertex v = vertices[i];
            v.pos = FVector.setMag(v.pos, size);
        }
        //hull.Generate(vertices);
        //sVoronoi.Generate(hull);
    }
}
