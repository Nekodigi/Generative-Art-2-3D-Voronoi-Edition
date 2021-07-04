using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExampleVoronoi
{
    int dim;
    int numVertices = 100;
    float size = 5f;//0.1
    public Voronoi voronoi;
    public List<Vertex> vertices = new List<Vertex>();
    int seed = 0;
    Camera cam;

    public Delaunay delaunay;
    public ExampleVoronoi(int dim)
    {
        this.dim = dim;
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
        delaunay = new Delaunay(dim);
        delaunay.Generate(vertices);
        voronoi = new Voronoi(dim);
        voronoi.Generate(delaunay);
    }

    void resetVertex()
    {
        vertices = new List<Vertex>();
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
    }

    public void show()
    {
        foreach (Simplex s in delaunay.simplexes)
        {
            foreach (Simplex s2 in s.adjacent)
            {
                if (s == null || s2 == null) continue;
                s.calcCircumCenter();
                s2.calcCircumCenter();
                    GeomRender.line(s.circumC.pos, s2.circumC.pos);//here!!!!!!!
            }
        }
    }

    public void relax(float fac)
    {
        if(dim == 2)
        {
            relax2D(fac);
        }else if(dim == 3)
        {
            relax3D(fac);
        }
    }

    public void relax3D(float fac)
    {
        foreach(Region region in voronoi.regions)
        {
            region.relax(fac);
        }
        try
        {
            delaunay = new Delaunay(dim);
            delaunay.Generate(vertices);
            voronoi = new Voronoi(dim);
            voronoi.Generate(delaunay);
        }
        catch (System.Exception e)
        {
            resetVertex();
            //relax(fac);
        }
    } 

    public void relax2D(float fac)
    {
        foreach (Polygon polygon in voronoi.polygons)
        {
            polygon.relax(fac);
        }
        for (int i = vertices.Count - 1; i >= 0; i--)
        {
            Vertex v = vertices[i];
            //float[] t = { constrain(v.pos[0], -origin.x - 100, -origin.x + width + 100), constrain(v.pos[1], -origin.y - 100, -origin.y + height + 100) };
            cam = Camera.main;
            float height = cam.orthographicSize;//height/2
            float width = height * cam.aspect;
            float[] t = {Mathf.Clamp(v.pos[0], -width-1, width+1), Mathf.Clamp(v.pos[1], -height-1, height+1) };
            v.pos = t;
        }
        try
        {
            delaunay = new Delaunay(dim);
            delaunay.Generate(vertices);
            voronoi = new Voronoi(dim);
            voronoi.Generate(delaunay);
        }
        catch (System.Exception e)
        {
            resetVertex();
            //relax(fac);
        }
    }
}
