using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Parabox.CSG;
using GK;
public class Region
{
    public int dim;
    public List<Simplex> edges = new List<Simplex>();
    public List<Vertex> vertices = new List<Vertex>();
    public ConvexHull hull;
    public Color col = Color.HSVToRGB(1, Random.Range(0, 1.0f), 1);
    GameObject obj;
    public Vertex baseVertex;

    public Region(int dim)
    {
        this.dim = dim;
        hull = new ConvexHull(dim);
    }

    public void calc()
    {
        hull.Generate(vertices);
        //col.a = 0.0f;
        foreach (Polygon poly in hull.polygons)
        {
            poly.col = col;
        }
    }

    public void Release()
    {
        if(obj != null)
        obj.GetComponent<Rigidbody>().isKinematic = false;
    }

    public void Generate3D()
    { 

        var chc = new ConvexHullCalculator();//used external library because not working when CSG
        var verts = new List<Vector3>();
        var tris = new List<int>();
        var normals = new List<Vector3>();
        var points = new List<Vector3>();
        foreach(Vertex v in vertices)
        {
            if (FVector.sqrMag(v.pos) > 100) {
                MonoBehaviour.Destroy(obj);
                
                return; }
            points.Add(new Vector3(v.pos[0], v.pos[1], v.pos[2]));
        }

            chc.GenerateHull(points, true, ref verts, ref tris, ref normals);

        obj = MonoBehaviour.Instantiate(HullVoronoiMain.baseObj, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;


        var mesh = new Mesh();
        mesh.SetVertices(verts);
        mesh.SetTriangles(tris, 0);
        mesh.SetNormals(normals);

        obj.GetComponent<MeshFilter>().sharedMesh = mesh;
        obj.GetComponent<MeshCollider>().sharedMesh = mesh;
        //clip();
    }

    public void clip()
    {
        if (obj == null) return;
        try
        {
            CSG_Model result = Boolean.Intersect(HullVoronoiMain.castObj, obj);

            //use Brutal method to change triangle orientation(assume always convex hull)
            var chc = new ConvexHullCalculator();//used external library because not working when CSG
            var verts = new List<Vector3>();
            var tris = new List<int>();
            var normals = new List<Vector3>();
            var points = result.mesh.vertices;

            chc.GenerateHull(new List<Vector3>(result.mesh.vertices), true, ref verts, ref tris, ref normals);

            //obj = GameObject.Instantiate(Test.baseObj, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;


            var mesh = new Mesh();
            mesh.SetVertices(verts);
            mesh.SetTriangles(tris, 0);
            mesh.SetNormals(normals);

            obj.GetComponent<MeshFilter>().sharedMesh = mesh;
            obj.GetComponent<MeshCollider>().sharedMesh = mesh;
        }
        catch (System.Exception e)
        {
            //Debug.Log("EXCEPT");
            //Debug.Log(obj);
            //obj.GetComponent<MeshFilter>().sharedMesh = new Mesh();
            //obj.GetComponent<MeshCollider>().sharedMesh = new Mesh();
            MonoBehaviour.Destroy(obj);
            //MonoBehaviour.Destroy(obj2);
        }
        //obj.GetComponent<MeshCollider>().sharedMesh = result.mesh;
        //Debug.Log("clip");
    }

    public void show()
    {
        //for(Vertex vertex : vertices){
        //  point(vertex.pos);
        //}
        col.a = 0.1f;
        GeomRender.fill = col;
        GeomRender.stroke.a = 0;
        hull.show();//don't show edge because it's surface is triangle.
        GeomRender.stroke.a = 1;
        foreach(Simplex edge in edges)//show edge
        {
            edge.show();
        }
    }

    #region Relaxation
    float[] Centroid()
    {
        return FVector.avg(HVUtils.extractPos(vertices.ToArray()));
    }

    public void relax(float fac)
    {
        float[] npos;
        if (baseVertex.pos.Length == 2)
        {
            npos = Centroid();
        }
        else
        {
            npos = FVector.avg(HVUtils.extractPos(vertices.ToArray()));
        }
        baseVertex.pos = FVector.lerp(npos, baseVertex.pos, fac);
    }
    #endregion
}
