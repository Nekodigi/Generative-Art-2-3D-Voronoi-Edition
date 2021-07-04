using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vertex
{
    public int dim;
    public int id;
    public int tag;
    public float[] pos;//position
    public float[] vel;//velocity
    public Color col = Color.HSVToRGB(0.66f, Random.Range(0, 1.0f), 0.5f);
    //for a star
    public List<Vertex> adj = new List<Vertex>();//graph
    public Vertex previous;//for a star
    public float h;
    public float g;
    //for polygon offset
    public float value;
    public bool merged;
    public Vertex next, prev;

    public Vertex(int id, params float[] pos)
    {
        dim = pos.Length;
        this.pos = pos;
        this.id = id;
        float angle = Random.Range(0.0f, Mathf.PI*2);
        this.vel = FVector.set(Mathf.Cos(angle), Mathf.Sin(angle));
    }

    public Vertex(int id, Vector3 v) : this(id, FVector.set(v)) { }

    public void update()
    {
        pos = FVector.add(pos, FVector.mult(vel, 0.01f));
        float aspect = Camera.main.aspect;
        float h2 = 6.0f;
        float w2 = h2 * aspect;//width devided by 2, height divided by 2 = 5.0 in default
        if(pos[0] < -w2 || w2 < pos[0])
        {
            vel[0] = -vel[0];
            pos = FVector.add(pos, FVector.mult(vel, 0.02f));
        }
        if (pos[1] < -h2 || h2 < pos[1])
        {
            vel[1] = -vel[1];
            pos = FVector.add(pos, FVector.mult(vel, 0.02f));
        }
    }

    public void vertexRelax(List<Vertex> targets, float r=0.01f, float force=1)
    {
        foreach (Vertex target in targets)
        {
            if (target == this) continue;
            float[] diff = FVector.sub(pos, target.pos);
            float dist = 0;
            if (diff.Length == 2) dist = diff[0] * diff[0] + diff[1] * diff[1];
            else dist = diff[0] * diff[0] + diff[1] * diff[1] + diff[2] * diff[2];
            if (dist < 2 * r * 2 * r)
            {
                dist = Mathf.Sqrt(dist);
                float rdist = 2 * r - dist;
                pos = FVector.add(pos, FVector.setMag(diff, Utils.map(rdist, 0, 2 * r, 0, force)));
            }
        }
    }



    public float getF()
    {
        return h + g;
    }

    public void addAdj(Vertex v)
    {
        if (!HVUtils.hasItem(v, adj))
        {
            adj.Add(v);
        }
    }
}

public class VertexIdComparer : IComparer<Vertex>
{
    public int Compare(Vertex v0, Vertex v1)
    {
        return v0.id.CompareTo(v1.id);
    }
}