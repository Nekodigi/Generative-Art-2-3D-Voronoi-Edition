using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Polygon
{
    public List<Vertex> vertices = new List<Vertex>();
    public Color col = Color.HSVToRGB(0.66f, Random.Range(0, 1.0f), 0.5f);
    public Vertex baseVertex;
    public Simplex simplex;
    Vector3[] offsetV;
    List<Vector3> resV = null;//result vertices
    Vector3[] normal;
    float thickness;
    int n;
    float off = 0;

    public Polygon() { }

    public Polygon(Vertex baseVertex_)
    {
        baseVertex = baseVertex_;
        Random.InitState(baseVertex.id);
        col = Color.HSVToRGB(0.66f, Random.Range(0.0f, 1f), 0.5f);
    }

    public Polygon(Simplex simplex)
    {
        this.vertices.AddRange(simplex.vertices);
        this.simplex = simplex;
        if (simplex.dim == 3)//in 2d
        {
            int seed = simplex.vertices[0].id + simplex.vertices[1].id + simplex.vertices[2].id;
            Random.InitState(seed);
            col = Color.HSVToRGB(0.66f, Random.Range(0.0f, 1f), 0.5f);
            //col = Color.HSVToRGB(0.66f, (seed/100)%1, 1);
            float[] centroid = Centroid();
            //col = Color.HSVToRGB(0.66f, Mathf.PerlinNoise(centroid[0], centroid[1])*2-0.5f, 1);//Random.Range(0, 1.0f)
        }
    }

    public void toGraph()
    {
        for (int i = 0; i < vertices.Count; i++)
        {
            Vertex a = vertices[i];
            Vertex b = vertices[(i + 1) % vertices.Count];
            a.addAdj(b);
            b.addAdj(a);
        }
    }

    public void show(bool doOffset=false)
    {

        //GeomRender.thickness = 0.05f;
        if (doOffset)
        {
            off = HullVoronoiMain.off;
            Vector3 centor = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            centor.z = 0;
            float dst = Vector3.Distance(FVector.toVec(Centroid()), centor) / 4.0f;
            if (Mathf.Clamp(off - dst, 0, 1.0f) != 0)
                offset(Mathf.Clamp(off - dst, 0, 1.0f));
            col.a = Mathf.Clamp(-off + dst + 0.5f, 0, 1f);
            //col.a = 1f;
        }

        GeomRender.close = true;
        GeomRender.fill = col;
        GeomRender.stroke.a = col.a;
        //GeomRender.stroke.a = 0.2f;

        if(resV != null)
        GeomRender.convex(FVector.set(resV.ToArray()));
        else GeomRender.convex(HVUtils.extractPos(vertices.ToArray()));
    }

    #region offset
    
    void offset(float thickness)
    {
       // float fixedThickness = isClockwise() ? thickness : -thickness;
       // fixedThickness = thickness;
        this.thickness = thickness;
        //Debug.Log(fixedThickness);
        if (isClockwise())
        {
            vertices.Reverse();
        }
        offsetV = new Vector3[vertices.Count];
        for(int i=0; i<vertices.Count; i++)
        {
            Vertex v = vertices[i];
            offsetV[i] = FVector.toVec(v.pos);
        }
        n = vertices.Count;
        calcNormal();
        int u = n - 1;
        for (int i = 0; i < n; i++)
        {
            Vector3 na = normal[u]; Vector3 nb = normal[i];
            Vector3 bis = (na + nb).normalized;
            float l = thickness * Mathf.Sqrt(2) / Mathf.Sqrt(1 + Vector3.Dot(na, nb));
            Vector3 pt = bis * l;
            //float[] t = {pt.x, pt.y};
            //offsetV.set(u, Vector3.Add(vertices[u).pos, t)));
            offsetV[u] += pt;
            u = i;
        }
        solveSelfIntersection();
    }

    bool isClockwise()//work only 2d
    {
        float sum = 0;
        int u = vertices.Count - 1;
        for (int i = 0; i < vertices.Count; i++)
        {
            float[] v = vertices[u].pos;
            float[] v2 = vertices[i].pos;
            sum += (v[0] * v2[1] - v2[0] * v[1]);
            u = i;
        }
        if (sum > 0) return true;
        else return false;
    }

    bool isClockwise(List<Vector3> vertices_)
    {
        float sum = 0;
        int u = vertices_.Count - 1;
        for (int i = 0; i < vertices_.Count; i++)
        {
            Vector3 v = vertices_[u];
            Vector3 v2 = vertices_[i];
            sum += (v.x * v2.y - v2.x * v.y);
            u = i;
        }
        //println(sum);
        if (sum > 0) return true;
        else return false;
    }

    static int compareDist(Vertex a, Vertex b)
    {
        return a.value > b.value ? 1 : -1;
    }

    void solveSelfIntersection()
    {
        resV = new List<Vector3>();
        List<Vertex> offsetV_ = new List<Vertex>();

        for (int i = 0; i < offsetV.Length; i++)
        {
            Vertex A = new Vertex(0, offsetV[i]);
            Vector3 B = offsetV[(i + 1) % offsetV.Length];
            A.id = i;
            offsetV_.Add(A);
            List<Vertex> temp = new List<Vertex>();
            for (int j = 0; j < offsetV.Length; j++)
            {//we might improve this
                Vector3 C = offsetV[j];
                Vector3 D = offsetV[(j + 1) % offsetV.Length];
                Vector3 p_ = HVUtils.intersection(FVector.toVec(A.pos), B, C, D);
                Vertex p = new Vertex(0, p_);
                if (p_ != Vector3.zero)
                {
                    //ellipse(p.x*scale, p.y*scale, 10, 10);
                    //resV.Add(p);
                    temp.Add(p);

                    p.value = Vector3.Distance(p_, FVector.toVec(A.pos));
                    //print(p.value);
                }
            }
            temp.Sort(compareDist);
            offsetV_.AddRange(temp);
        }
        Vertex prev = offsetV_[offsetV_.Count - 1];
        int count = 0;
        foreach (Vertex v in offsetV_)
        {
            v.id = count++;
            v.merged = false;
            prev.next = v;
            v.prev = prev;
            prev = v;
        }

        for (int i = 0; i < offsetV_.Count; i++)
        {
            Vertex v = offsetV_[i];
            for (int j = 0; j < offsetV_.Count; j++)
            {
                if (i == j) continue;
                Vertex v_ = offsetV_[j];
                if (v.merged != true && Mathf.Abs(v.pos[0] - v_.pos[0]) < HVUtils.EPSILON && Mathf.Abs(v.pos[1] - v_.pos[1]) < HVUtils.EPSILON)
                {
                    v_.merged = true;
                    v.merged = true;
                    Vertex t = v.next;
                    v.next = v_.next;
                    v_.next = t;
                }
            }
        }
        bool[] checked_ = new bool[offsetV_.Count];

        while (true)
        {
            int id = -1;
            List<Vector3> preResV = new List<Vector3>();
            for (int i = 0; i < checked_.Length; i++)
            {
                if (checked_[i] == false) {
                    id = i;
                    break;
                }
            }
            if (id == -1) break;
            Vertex start = offsetV_[id];
            Vertex current = start;
            int safety = 0;
            while (true && safety++ < 100)
            {
                preResV.Add(FVector.toVec(current.pos));
                checked_[current.id] = true;
                current = current.next;
                if (current == start) break;
                }//(isClockwise)
                
                if (isClockwise(preResV)) preResV = new List<Vector3>();
                resV.AddRange(preResV);
            }
            if (!isValid(resV))resV = new List<Vector3>();
        }

    bool isValid(List<Vector3> target)
    {//check valid. !we can use this for only convex full
        bool result = true;
        for (int i = 0; i < vertices.Count; i++)
        {
            Vector3 v = FVector.toVec(vertices[i].pos);
            Vector3 n = normal[i];
            foreach (Vector3 v2 in target)
            {
                float dst = Vector3.Dot(v, n) + thickness - Vector3.Dot(v2, n);
                if (dst > HVUtils.EPSILON) return false;
            }
        }
        return result;
    }

    void calcNormal()
    {
        int u = n - 1;
        normal = new Vector3[n];
        for (int i = 0; i < n; i++)
        {
            float[] ndir = FVector.normalize(FVector.sub(vertices[i].pos, vertices[u].pos));
            normal[i] = new Vector3(ndir[1], -ndir[0]);
            u = i;
        }
    }
    #endregion

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
