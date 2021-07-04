using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Voronoi
{
    public int dim;
    public List<Region> regions = new List<Region>();//used for 3d
    public List<Polygon> polygons = new List<Polygon>();//used for 2d
    public List<Vertex> vertices;
    float sizeLimit = 10.0f;//because too big shape seem strange.

    public Delaunay delaunay;
    public Voronoi(int dim)
    {
        this.dim = dim;
        delaunay = new Delaunay(dim);
    }

    #region Generate
    public void Generate(Delaunay delaunay_)
    {
        this.delaunay = delaunay_;
        vertices = new List<Vertex>();
        foreach (Simplex simplex in this.delaunay.simplexes)
        {//calculation all circumCenter
            simplex.calcCircumCenter();
            vertices.Add(simplex.circumC);
        }
        foreach (Simplex simplex in this.delaunay.simplexes)
        {
            foreach (Simplex adj in simplex.adjacent)
            {
                //if (adj.circumC == null) adj.calcCircumCenter();//not match with voronoi
                if (adj.circumC != null)
                {
                    simplex.circumC.addAdj(adj.circumC);
                    adj.circumC.addAdj(simplex.circumC);
                }
            }
        }
        if (dim == 2) Generate2D();
        else if (dim == 3) Generate3D();
    }

    void Generate3D()
    {
        List<Simplex> around = new List<Simplex>();//simplex around vertices
        for (int i = 0; i < delaunay.vertices.Count; i++)
        {
            Region region = new Region(dim);
            around.Clear();
            Vertex vertex = delaunay.vertices[i];
            region.baseVertex = vertex;
            for (int j = 0; j < delaunay.simplexes.Count; j++)
            {
                Simplex simplex = delaunay.simplexes[j];
                if (HVUtils.contains(simplex.vertices, vertex))
                {
                    around.Add(simplex);
                }
            }
            if (around.Count > 0)
            {
                for (int j = 0; j < around.Count; j++)
                {
                    Simplex simplex = around[j];
                    for (int k = 0; k < simplex.adjacent.Length; k++)
                    {
                        Simplex adjFace = simplex.adjacent[k];
                        adjFace.calcCircumCenter();
                        if (around.Contains(adjFace))
                        {
                            if (adjFace.circumC == null) adjFace.calcCircumCenter();
                            Simplex edge = new Simplex(2);
                            edge.vertices[0] = new Vertex(0, simplex.circumC.pos);
                            edge.vertices[1] = new Vertex(0, adjFace.circumC.pos);
                            region.edges.Add(edge);
                            //region.vertices.Add(new Vertex(0, resize(simplex.circumC.pos, dim)));
                        }
                    }
                }

            }
            if (around.Count > 0)
            {
                for (int j = 0; j < around.Count; j++)
                {
                    Simplex simplex = around[j]; simplex.calcCircumCenter();
                    region.vertices.Add(new Vertex(0, FVector.resize(simplex.circumC.pos, dim)));
                }
            }
            try
            {//to miss convex hull singular input error
                region.calc();
            }
            catch (Exception e)
            {
                //miss this error
            }
            regions.Add(region);
        }
    }

    public void Gen3DModel()
    {
        foreach(Region region in regions)
        {
            region.Generate3D();
        }
    }

    public void Release3D()
    {
        foreach (Region region in regions)
        {
            region.Release();
        }
    }

    void Generate2D()
    {
        polygons = new List<Polygon>();
        foreach (Vertex v in delaunay.vertices)
        {//calculate all polygon
            Polygon polygon = new Polygon(v);
            Simplex current = null;
            foreach (Simplex simplex in delaunay.simplexes)
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
            if (end== null || end.circumC == null) continue;
            if (prev==null||prev.circumC == null) continue; if (FVector.sqrDist(end.circumC.pos, v.pos) > sizeLimit * sizeLimit) continue;
            polygon.vertices.Add(end.circumC); if (FVector.sqrDist(prev.circumC.pos, v.pos) > sizeLimit * sizeLimit) continue;
            polygon.vertices.Add(prev.circumC);//
            bool breakTag = false;
            int safety = 0;
            while (safety < 10000 && current != end)
            {//add vertex while going around v
                if (current == null) break;
                adjHasV = HVUtils.getAdjHasVertex(current, v);
                if (FVector.sqrDist(prev.circumC.pos, v.pos) > sizeLimit * sizeLimit) { breakTag = true; break; };
                if (adjHasV[0] != prev)
                {//to avoid backing
                    prev = current; if (prev.circumC == null) { breakTag = true; break; };
                    polygon.vertices.Add(prev.circumC);
                    current = adjHasV[0];
                }
                else
                {
                    prev = current; if (prev.circumC == null) { breakTag = true; break; };
                    polygon.vertices.Add(prev.circumC);
                    current = adjHasV[1];
                }
            }
            if (safety == 10000) Debug.LogError("not safety");
            if (breakTag == true) continue;
            polygons.Add(polygon);
        }
    }
    #endregion

    public void show(bool doOffset=false)
    {
        foreach (Vertex vertex in delaunay.vertices)
        {
            //GeomRender.point(vertex.pos);
        }
        if (dim == 2)
        {
            foreach (Polygon poly in polygons)
            {
                poly.show(doOffset);
            }
        }
        else if (dim == 3)
        {
            foreach (Region region in regions)
            {
                region.show();
            }
            //voronoi.regions.get(int(float(frameCount)/10%voronoi.regions.size())).show();//for easy to understand
        }
    }

    public void toGraph()
    {
        if (dim == 2)
        {
            foreach (Polygon poly in polygons)
            {
                poly.toGraph();
            }
        }
        else if (dim == 3)
        {
            //automaticly calculated because vertex might change in process...
        }
    }
}
