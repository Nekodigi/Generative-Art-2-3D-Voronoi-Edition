using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphericalVoronoi
{
    public List<Vertex> vertices;
    public List<Polygon> polygons = new List<Polygon>();
    public ConvexHull hull = new ConvexHull(3);

    public void Generate(ConvexHull hull)
    {
        this.hull = hull;
        polygons = new List<Polygon>();
        vertices = new List<Vertex>();
        foreach (Simplex simplex in hull.simplexes)
        {//calculation all circumCenter
            simplex.calcCircumCenter();
            vertices.Add(simplex.circumC);
        }
        foreach (Vertex v in hull.vertices)
        {//calculate all polygon
            Polygon polygon = new Polygon(v);
            Simplex current = null;
            foreach (Simplex simplex in hull.simplexes)
            {//pick up one of simplex which contain v
                if (HVUtils.hasItem(v, simplex.vertices))
                {
                    current = simplex;
                    break;
                }
            }
            Simplex[] adjHasV = HVUtils.getAdjHasVertex(current, v);//get adjacent around v
            Simplex end = adjHasV[0];
            Simplex prev = current;
            current = adjHasV[1];
            polygon.vertices.Add(end.circumC);
            polygon.vertices.Add(prev.circumC);
            int safety = 0;
            while (safety < 10000 && current != end)
            {//add vertex while going around v
                adjHasV = HVUtils.getAdjHasVertex(current, v);
                if (adjHasV[0] != prev)
                {//to avoid backing
                    prev = current;
                    polygon.vertices.Add(prev.circumC);
                    current = adjHasV[0];
                }
                else
                {
                    prev = current;
                    polygon.vertices.Add(prev.circumC);
                    current = adjHasV[1];
                }
                safety++;
            }
            if (safety == 10000) Debug.LogError("not safety");
            polygons.Add(polygon);
        }
    }

    public void show()
    {
        foreach (Vertex vertex in hull.vertices)
        {
            //GeomRender.point(vertex.pos);
        }
        //noStroke();
        foreach (Polygon polygon in polygons)
        {
            polygon.show();
        }
    }

    public void toGraph()
    {
        foreach (Polygon poly in polygons)
        {
            poly.toGraph();
        }
    }
}
