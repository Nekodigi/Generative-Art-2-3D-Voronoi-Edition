using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HEData
{
    public List<HEVertex> vertices = new List<HEVertex>();
    public List<HEFace> faces = new List<HEFace>();
    public List<HalfEdge> edges = new List<HalfEdge>();
    public ObjectBuffer buffer;

    //simplex should be triangle
    public HEData(List<Simplex> triangles)
    {
        buffer = new ObjectBuffer(3);

        Geometory.OrientTrianglesClockwise(triangles);

        foreach (Simplex t in triangles)
        {
            HEVertex v1 = new HEVertex(t.vertices[0].pos);
            HEVertex v2 = new HEVertex(t.vertices[1].pos);
            HEVertex v3 = new HEVertex(t.vertices[2].pos);

            HalfEdge he1 = new HalfEdge(v1);
            HalfEdge he2 = new HalfEdge(v2);
            HalfEdge he3 = new HalfEdge(v3);

            he1.nextEdge = he2;
            he2.nextEdge = he3;
            he3.nextEdge = he1;

            he1.prevEdge = he3;
            he2.prevEdge = he1;
            he3.prevEdge = he2;

            //The vertex needs to know of an edge going from it
            v1.edge = he2;
            v2.edge = he3;
            v3.edge = he1;

            //The face the half-edge is connected to
            HEFace face = new HEFace(he1);

            //Each edge needs to know of the face connected to this edge
            he1.face = face;
            he2.face = face;
            he3.face = face;


            //Add everything to the lists
            edges.Add(he1);
            edges.Add(he2);
            edges.Add(he3);

            faces.Add(face);

            vertices.Add(v1);
            vertices.Add(v2);
            vertices.Add(v3);
        }

        //Step 4. Find the half-edges going in the opposite direction of each edge we have 
        //Is there a faster way to do this because this is the bottleneck?
        foreach (HalfEdge e in edges)
        {
            HEVertex goingToVertex = e.v;
            HEVertex goingFromVertex = e.prevEdge.v;

            foreach (HalfEdge eOther in edges)
            {
                //Dont compare with itself
                if (e == eOther)
                {
                    continue;
                }

                //Is this edge going between the vertices in the opposite direction
                if (FVector.equal(goingFromVertex.pos, eOther.v.pos) && FVector.equal(goingToVertex.pos, eOther.prevEdge.v.pos))
                {
                    e.oppositeEdge = eOther;

                    break;
                }
            }
        }
    }

    public List<HalfEdge> GetUniqueEdges()
    {
        List<HalfEdge> uniqueEdges = new List<HalfEdge>();

        foreach (HalfEdge e in edges)
        {
            float[] p1 = e.v.pos;
            float[] p2 = e.prevEdge.v.pos;

            bool isInList = false;

            for (int j = 0; j < uniqueEdges.Count; j++)
            {
                HalfEdge testEdge = uniqueEdges[j];

                float[] p1_test = testEdge.v.pos;
                float[] p2_test = testEdge.prevEdge.v.pos;

                if ((FVector.equal(p1, p1_test) && FVector.equal(p2, p2_test)) || (FVector.equal(p2, p1_test) && FVector.equal(p1, p2_test)))
                {
                    isInList = true;

                    break;
                }
            }

            if (!isInList)
            {
                uniqueEdges.Add(e);
            }
        }

        return uniqueEdges;
    }

    public bool contains(List<Vertex> posIds, float[] target)
    {
        foreach (Vertex posId in posIds)
        {
            if (posId.pos[0] == target[0] && posId.pos[1] == target[1])
            {
                return true;
            }
        }
        return false;
    }

    public int indexOf(List<Vertex> posIds, float[] target)
    {
        for (int i = 0; i < posIds.Count; i++)
        {
            Vertex posId = posIds[i];
            if (posId.pos[0] == target[0] && posId.pos[1] == target[1])
            {
                return i;
            }
        }
        return -1;
    }

    public SimplexVertices toSimplexes()
    {//!simplex don't linked
        List<Simplex> triangles = new List<Simplex>();
        List<Vertex> posIds = new List<Vertex>();//so blute force
        int i = 0;
        foreach (HEVertex vertex in vertices)
        {
            if (!contains(posIds, vertex.pos))
            {
                posIds.Add(new Vertex(i++, vertex.pos));
            }
        }
        foreach (HEFace face in faces)
        {
            Simplex t = new Simplex(3);
            float[] p1 = face.edge.v.pos;
            float[] p2 = face.edge.nextEdge.v.pos;
            float[] p3 = face.edge.nextEdge.nextEdge.v.pos;//println(indexOf(posIds, p1));
            t.vertices[0] = posIds[indexOf(posIds, p1)];
            t.vertices[1] = posIds[indexOf(posIds, p2)];
            t.vertices[2] = posIds[indexOf(posIds, p3)];

            triangles.Add(t);
            for (int j = 0; j < 3; j++)
            {
                SimplexConnector connector = new SimplexConnector(t, j, 3);//connector is a class for searching the corresponding face at high speed with hash
                connectFace(connector);
            }
        }



        return new SimplexVertices(triangles, posIds);
    }

    public void connectFace(SimplexConnector connector)
    {
        int index = connector.hashCode % buffer.connector_table_size;
        List<SimplexConnector> list = buffer.connectorTable[index];
        //check foreach connector
        for (int i = 0; i < list.Count; i++)
        {
            SimplexConnector current = list[i];
            if (SimplexConnector.areConnectable(connector, current, 3))
            {
                list.Remove(current);
                SimplexConnector.connect(current, connector);
                return;
            }
        }
        list.Add(connector);
    }
}

public class SimplexVertices
{
    public List<Simplex> simplexes = new List<Simplex>();
    public List<Vertex> vertices = new List<Vertex>();

    public SimplexVertices(List<Simplex> simplexes, List<Vertex> vertices)
    {
        this.simplexes = simplexes;
        this.vertices = vertices;
    }
}


public class HalfEdge
{//Half edge
    public HEVertex v;
    public HEFace face;
    public HalfEdge nextEdge;
    public HalfEdge oppositeEdge;
    public HalfEdge prevEdge;

    public HalfEdge(HEVertex v)
    {
        this.v = v;
    }
}

public class HEVertex
{
    public float[] pos;
    public HalfEdge edge;

    public HEVertex(float[] pos)
    {
        this.pos = pos;
    }
}

public class HEFace
{
    public HalfEdge edge;

    public HEFace(HalfEdge edge)
    {
        this.edge = edge;
    }
}