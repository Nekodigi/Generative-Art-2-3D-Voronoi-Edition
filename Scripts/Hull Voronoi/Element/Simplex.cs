using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Parabox.CSG;
using GK;

public class Simplex
{
    public int dim;
    // The simplexs adjacent to this simplex
    // For 2D a simplex will be a segment and it with have two adjacent segments joining it.
    // For 3D a simplex will be a triangle and it with have three adjacent triangles joining it.
    public Simplex[] adjacent;
    // The vertices that make up the simplex.
    // For 2D a face will be 2 vertices making a line.
    // For 3D a face will be 3 vertices making a triangle.
    public Vertex[] vertices;
    public List<Vertex> beyondVertices;//vertices that positive side this face (same side as normal)
    public Vertex furthestVertex;
    public float maxDist;
    // The simplexs normal.
    public float[] normal;
    public Vertex circumC;//circumCenter
    public float circumR;//circumRadius
    public bool isNormalFlipped;
    // The simplexs centroid.
    public float[] centroid;
    // The simplexs offset from the origin.
    public float offset;
    public int tag;
    // Start is called before the first frame update
    GameObject obj;

    public Simplex(int dim)
    {
        if (dim < 2 || dim > 4) { Debug.LogError("Invalid number of dimension for Simplex:" + dim); }
        this.dim = dim;
        adjacent = new Simplex[dim];
        normal = new float[dim];
        centroid = new float[dim];
        vertices = new Vertex[dim];
    }

    public void toGraph()
    {
        for (int i = 0; i < vertices.Length; i++)
        {
            Vertex a = vertices[i];
            Vertex b = vertices[(i + 1) % vertices.Length];
            a.addAdj(b);
            b.addAdj(a);
        }
    }

    public void calcCentroid()
    {
        centroid = FVector.avg(HVUtils.extractPos(vertices));
    }

    public void ChangeOrientation()
    {
        Vertex tv = vertices[1];
        vertices[1] = vertices[2];
        vertices[2] = tv;
        Simplex ts = adjacent[1];
        adjacent[1] = adjacent[2];
        adjacent[2] = ts;
    }

    public void setAllVerticesTag(int tag)
    {
        for (int i = 0; i < dim; i++)
        {
            vertices[i].tag = tag;
        }
    }

    public void clearBeyond()
    {
        beyondVertices = new List<Vertex>();
        maxDist = float.NegativeInfinity;
        furthestVertex = null;
    }

    public void Release()
    {
        if (obj != null)
        {
            //obj.GetComponent<Rigidbody>().useGravity = true;
            obj.GetComponent<Rigidbody>().isKinematic = false;
        }
    }

    public void Generate3D()
    {
        var verts_ = new Vector3[4];
        var verts = new Vector3[12];
        obj = GameObject.Instantiate(HullVoronoiMain.baseObj, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;

        verts_[0] = new Vector3(vertices[0].pos[0], vertices[0].pos[1], vertices[0].pos[2]);
        verts_[1] = new Vector3(vertices[1].pos[0], vertices[1].pos[1], vertices[1].pos[2]);
        verts_[2] = new Vector3(vertices[2].pos[0], vertices[2].pos[1], vertices[2].pos[2]);
        verts_[3] = new Vector3(vertices[3].pos[0], vertices[3].pos[1], vertices[3].pos[2]);

        var tris_ = new int[]{
            0, 1, 2,
            3, 2, 1,
            2, 3, 0,
            1, 0, 3
        };
        calcCentroid();
        if (faceDistCentroid(0, 1, 2) > 0) { tris_[0] = 2; tris_[2] = 0; }
        if (faceDistCentroid(3, 2, 1) > 0) { tris_[3] = 1; tris_[5] = 3; }
        if (faceDistCentroid(2, 3, 0) > 0) { tris_[6] = 0; tris_[8] = 2; }
        if (faceDistCentroid(1, 0, 3) > 0) { tris_[9] = 3; tris_[11] = 1; }

        var tris = new int[12];

        int count = 0;
        foreach (int i in tris_)
        {
            tris[count] = count;
            verts[count++] = verts_[i];
        }
        var mesh = new Mesh();
        mesh.vertices = verts;
        mesh.triangles = tris;
        //mesh.SetNormals(normals);
        mesh.RecalculateNormals();

        obj.GetComponent<MeshFilter>().sharedMesh = mesh;
        obj.GetComponent<MeshCollider>().sharedMesh = mesh;
        //clip();
    }

    public void clip()
    {
        try
        {
            CSG_Model result = Boolean.Intersect(obj, HullVoronoiMain.castObj);
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
            //Debug.Log(e);
            //Debug.Log("EXCEPT");
            obj.GetComponent<MeshFilter>().sharedMesh = new Mesh();
            obj.GetComponent<MeshCollider>().sharedMesh = new Mesh();
            //MonoBehaviour.Destroy(obj);
            //MonoBehaviour.Destroy(obj2);
        }
        //obj.GetComponent<MeshCollider>().sharedMesh = result.mesh;
        //Debug.Log("clip");
    }

    public float faceDistCentroid(int fa, int fb, int fc)//facec distance from centroid,<input> face vertex index
    {
        float[] normal = FVector.calcNormal(vertices[fa], vertices[fb], vertices[fc]);
        float off = -FVector.dot(normal, vertices[fa].pos);
        float faceDist = off + FVector.dot(normal, centroid);
        return faceDist;
    }
    public float faceDistCentroid(Vector3 centroid_, Vector3[] vertices_, int fa, int fb, int fc)//facec distance from centroid,<input> face vertex index
    {
        float[] normal = FVector.calcNormal(new Vertex(0, vertices_[fa]), new Vertex(0, vertices_[fb]), new Vertex(0, vertices_[fc]));
        float off = -FVector.dot(normal, FVector.set(vertices_[fa]));
        float faceDist = off + FVector.dot(normal, FVector.set(centroid_));
        return faceDist;
    }

    public void show()
    {
        switch (vertices.Length)
        {
            case 2:
                
                GeomRender.line(vertices[0].pos, vertices[1].pos);
                break;
            case 3:
                GeomRender.close = true;
                GeomRender.convex(HVUtils.extractPos(vertices));
                break;
            case 4:
                GeomRender.close = true;
                GeomRender.convex(vertices[0].pos, vertices[1].pos, vertices[2].pos );
                GeomRender.convex(vertices[1].pos, vertices[2].pos, vertices[3].pos);
                GeomRender.convex(vertices[2].pos, vertices[3].pos, vertices[0].pos);
                GeomRender.convex(vertices[0].pos, vertices[1].pos, vertices[3].pos);
                break;
        }
    }

    public void calcCircumCenter()
    {
        switch (dim)
        {
            case 3://triangle
                calcCircumTriangle();
                break;
            case 4://tetrahedra
                calcCircumTetra();
                break;
        }
    }

    public void calcCircumTriangle()
    {//https://ja.wikipedia.org/wiki/%E5%A4%96%E6%8E%A5%E5%86%86
        float[] A = vertices[0].pos;
        float[] B = vertices[1].pos;
        float[] C = vertices[2].pos;
        float a = FVector.dist(B, C);
        float b = FVector.dist(C, A);
        float c = FVector.dist(A, B);
        float t1 = a * a * (b * b + c * c - a * a);
        float t2 = b * b * (c * c + a * a - b * b);
        float t3 = c * c * (a * a + b * b - c * c);
        float[] circumC_ = FVector.div(FVector.add(FVector.add(FVector.mult(A, t1), FVector.mult(B, t2)), FVector.mult(C, t3)), t1 + t2 + t3);
        circumC = new Vertex(0, circumC_);
        circumR = FVector.dist(circumC_, A);
    }

    public void calcCircumTetra()
    {//https://math.stackexchange.com/questions/2414640/circumsphere-of-a-tetrahedron
        float[] v0 = vertices[0].pos;
        float[] v1 = vertices[1].pos;
        float[] v2 = vertices[2].pos;
        float[] v3 = vertices[3].pos;
        float[] u1 = FVector.sub(v1, v0);
        float[] u2 = FVector.sub(v2, v0);
        float[] u3 = FVector.sub(v3, v0);
        float sqrl01 = FVector.sqrDist(v0, v1);
        float sqrl02 = FVector.sqrDist(v0, v2);
        float sqrl03 = FVector.sqrDist(v0, v3);
        float[] u23c = FVector.cross(u2, u3);
        float[] u31c = FVector.cross(u3, u1);
        float[] u12c = FVector.cross(u1, u2);
        float[] t1 = FVector.add(FVector.add(FVector.mult(u23c, sqrl01), FVector.mult(u31c, sqrl02)), FVector.mult(u12c, sqrl03));
        float t2 = FVector.dot(FVector.mult(u1, 2), u23c);
        float[] circumC_ = FVector.add(v0, FVector.div(t1, t2));
        circumC = new Vertex(0, circumC_);
        circumR = FVector.dist(circumC_, v0);
    }
}
