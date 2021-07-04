using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExampleConstrainedDelaunay 
{
    public Delaunay delaunay;
    public List<Vertex> vertices = new List<Vertex>();
    public List<Vertex> baseVertices = new List<Vertex>();
    public List<Polygon> polygons = new List<Polygon>();
    int seed = 0;
    public List<List<float[]>>[] constraintss;
    public SimplexVertices constrained;
    public int numVertices = 200;
    public float size = 10.0f;
    public float[,] canvas;
    public float threshold = 0.5f;
    int dim;

    public ExampleConstrainedDelaunay()
    {
        dim = 2;
        //Random.RangeSeed(seed);
        for (int i = 0; i < numVertices; i++)
        {
            switch (dim)
            {
                case 2:
                    baseVertices.Add(new Vertex(0, Random.Range(-size, size), Random.Range(-size, size)));//id will be assigned later
                    break;
                case 3:
                    baseVertices.Add(new Vertex(0, Random.Range(-size, size), Random.Range(-size, size), Random.Range(-size, size)));//id will be assigned later
                    break;
            }
        }
        
    }

    public void Generate(List<List<float[]>>[] constraintss, bool shouldRemoveTriangles = true)
    {
        this.constraintss = constraintss;
        polygons = new List<Polygon>();
        foreach (List<List<float[]>> constraints in constraintss)
        {
            vertices = new List<Vertex>(baseVertices);
            foreach (List<float[]> constraint in constraints)
            {
                foreach (float[] v in constraint)
                {
                    vertices.Add(new Vertex(0, v));
                }
            }
            delaunay = new Delaunay(dim);
            delaunay.Generate(vertices);
            HEData he = new HEData(delaunay.simplexes);
            foreach (List<float[]> constraint in constraints)//heavy...
            {
                he = AddConstraint.AddConstraints(he, constraint, shouldRemoveTriangles);
            }
            //convet to simplex and to poly
            constrained = he.toSimplexes();

            delaunay.simplexes = constrained.simplexes;
            delaunay.polygons = HVUtils.simplex2Poly(delaunay.simplexes);

            polygons.AddRange(HVUtils.simplex2Poly(constrained.simplexes));
        }
    }

    public void toGraph()
    {
        foreach (Simplex s in constrained.simplexes)
        {
            s.toGraph();
        }
    }

    public void show()//hide unnessesary triangle after processing.. should change this
    {
        foreach (Vertex vertex in vertices)
        {
            GeomRender.point(vertex.pos);
        }
        //stroke(255, 0, 0);
        //point(delaunay.centroid);
        float scale = canvas.GetLength(1) / 5.0f / 2.0f;
        foreach (Polygon poly in polygons)
        {
            //poly.simplex.calcCentroid();
            //int i = Mathf.FloorToInt((poly.simplex.centroid[0]+10.0f)*scale);
            //int j = Mathf.FloorToInt((poly.simplex.centroid[1]+5.0f)*scale);
            //if (canvas[i,j] < threshold)
            //{
                poly.show();
            //}
        }

        /*foreach (Simplex s in delaunay.simplexes)
        {
            foreach (Simplex s2 in s.adjacent)
            {
                if (s == null || s2 == null) continue;
                s.calcCircumCenter();
                s2.calcCircumCenter();
                if (FVector.dist(s.circumC.pos, s2.circumC.pos) < 3)
                    GeomRender.line(s.circumC.pos, s2.circumC.pos);//here!!!!!!!
            }
        }*/
    }
}
