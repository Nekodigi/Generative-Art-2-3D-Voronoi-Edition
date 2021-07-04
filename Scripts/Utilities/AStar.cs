using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AStar
{
    public Vertex current;
    public Vertex end;
    public List<Vertex> openSet = new List<Vertex>();
    public List<Vertex> closedSet = new List<Vertex>();
    public List<List<Vertex>> path = new List<List<Vertex>>();
    List<Vertex> path_ = new List<Vertex>();
    public bool solving = false;

    public void solve(Vertex current, Vertex end)
    {
        solving = true;
        openSet = new List<Vertex>();
        closedSet = new List<Vertex>();
        this.current = current;
        openSet.Add(current);
        this.end = end;
        int safety = 0;
        while (solving&&safety++ < 1000) A_Star_Step();
        //Debug.Log(safety);//not so better...
        
    }

    public void A_Star_Step()
    {
        if (openSet.Count > 0)
        {
            Vertex winner = openSet[0];
            foreach (Vertex node in openSet)
            {
                if (node.getF() < winner.getF())
                {
                    winner = node;
                }
            }

            current = winner;
            openSet.Remove(current);
            closedSet.Add(current);
            foreach (Vertex neighbor in current.adj)
            {
                if (!closedSet.Contains(neighbor))
                {
                    float tempG = neighbor.g = current.g + heuristic(neighbor, current);
                    if (openSet.Contains(neighbor))
                    {
                        if (tempG < neighbor.g)
                        {
                            neighbor.g = tempG;
                        }
                    }
                    else
                    {
                        neighbor.g = tempG;
                        openSet.Add(neighbor);
                    }
                    neighbor.h = heuristic(neighbor, end);
                    neighbor.previous = current;
                }
            }
            if (current == end)
            {
                //Debug.Log("DONE");
                solving = false;

                path_ = new List<Vertex>();
                Vertex temp = current;
                path_.Add(temp);
                while (temp.previous != null)
                {
                    path_.Add(temp.previous);
                    temp = temp.previous;
                }
                path.Add(path_);
            }
        }
        else
        {
            //Debug.Log("NO SOLUTION");
            solving = false;
        }

        
    }

    public void show()
    {
        GeomRender.close = false;
        GeomRender.fill.a = 0;
        foreach (List<Vertex> path_ in path)
        {
            GeomRender.thickness = 1 * 0.002f;
            if (path_.Count <= 2) continue;
            float[] p01 = FVector.div(FVector.add(path_[0].pos, path_[1].pos), 2);
            GeomRender.line(path_[0].pos, p01);
            for (int i = 0; i < path_.Count - 2; i++)
            {
                GeomRender.thickness = i * 0.002f;
                for (float j=0; j<1; j += 0.2f)
                {
                    float[] a = FVector.div(FVector.add(path_[i].pos, path_[i+1].pos), 2);
                    float[] b = path_[i+1].pos;
                    float[] c = FVector.div(FVector.add(path_[i+1].pos, path_[i + 2].pos), 2);
                    //float[] p = bezier(a, b, c, j);
                    //float[] p2 = bezier(a, b, c, j+0.1f);
                    float[] p = bezier(a, b, c, j-0.1f);
                    float[] p2 = bezier(a, b, c, j + 0.2f + 0.1f);
                    GeomRender.line(p, p2);
                }
                //GeomRender.line(path_[i].pos, path_[i+1].pos);
            }
            float[] pe = FVector.div(FVector.add(path_[path_.Count - 2].pos, path_[path_.Count - 1].pos), 2);//last one edge
            GeomRender.line(pe, path_[path_.Count - 1].pos);
            //GeomRender.convex(HVUtils.extractPos(path_.ToArray()));
        }
        
    }

    float[] bezier(float[] a_, float[] b_, float[] c_, float t)
    {
        float[] a = FVector.add(FVector.mult(a_, 1 - t), FVector.mult(b_, t));
        float[] b = FVector.add(FVector.mult(b_, 1 - t), FVector.mult(c_, t));
        return FVector.add(FVector.mult(a, 1 - t), FVector.mult(b, t));
    }

    float heuristic(Vertex a, Vertex b, int heurType = 0)
    {
        switch (heurType)
        {
            case 0:
                return (a.pos[0] - b.pos[0]) * (a.pos[0] - b.pos[0]) + (a.pos[1] - b.pos[1]) * (a.pos[1] - b.pos[1]);
            case 1:
                return FVector.dist(a.pos, b.pos);
            case 2:
                return Mathf.Abs(a.pos[0] - b.pos[0]) + Mathf.Abs(a.pos[1] - b.pos[1]);
            default:
                return 1;
        }
    }
}
