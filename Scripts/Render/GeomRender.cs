using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeomRender : MonoBehaviour
{
    public static float thickness = 0.05f;
    public static Color stroke = new Color(1, 1, 1, 1);
    public static float strokea = 1;
    public static Color fill = new Color(1, 1, 1, 0.5f);
    public static float filla = 1;
    public static bool close;

    static void common()
    {
        stroke.a = strokea;
        fill.a = filla;
    }
    public static void line(float[] a, float[] b)
    {
        common();
       
        //GL.Begin(GL.QUADS);
        if (a.Length == 2)
        {
            GL.Begin(GL.QUADS);
            //stroke.a = 0;
            stroke = Color.HSVToRGB(0.66f, 0.5f, 1);
            stroke.a = 1;
            GL.Color(stroke);

            float[] dir = FVector.normalize(FVector.sub(b, a));
            float[] ver = {-dir[1], dir[0] };//vertical vector

            //vertex(a);
            //vertex(b);

            GL.Vertex3(a[0] - ver[0] * thickness, a[1] - ver[1] * thickness, 0);
            GL.Vertex3(a[0] + ver[0] * thickness, a[1] + ver[1] * thickness, 0);
            
            GL.Vertex3(b[0] + ver[0] * thickness, b[1] + ver[1] * thickness, 0);
            GL.Vertex3(b[0] - ver[0] * thickness, b[1] - ver[1] * thickness, 0);
        }
        else
        {
            GL.Begin(GL.LINES);
            GL.Color(stroke);
            //a = FVector.div(a, 100);
            vertex(a);
            vertex(b);
        }
        GL.End();
    }

    public static void vertex(Vector2 a)
    {
        float[] t = {a.x, a.y };
        vertex(t);
    }

    public static void vertex(float[] a)
    {
        if (a.Length == 2)
        {
            GL.Vertex3(a[0], a[1], 0);
        }
        else
        {
            GL.Vertex3(a[0], a[1], a[2]);
        }
    }

    public static void convex(params float[][] vertices)//convex
    {
        common();
        if (fill.a != 0)
        {
            GL.Begin(GL.TRIANGLES);
            GL.Color(fill);
            for (int i=1; i<vertices.Length-1; i++)
            {
                vertex(vertices[0]);
                vertex(vertices[i]);
                vertex(vertices[i+1]);
            }
            GL.End();
        }
        if (stroke.a != 0)
        {
            GL.Begin(GL.LINES);
            GL.Color(stroke);
            if (close)
            {
                int u = vertices.Length - 1;
                for (int i = 0; i < vertices.Length; i++)
                {
                    vertex(vertices[i]);
                    vertex(vertices[u]);
                    u = i;
                }
            }
            else
            {
                int u = 0;
                for (int i = 1; i < vertices.Length; i++)
                {
                    vertex(vertices[i]);
                    vertex(vertices[u]);
                    u = i;
                }
            }
            GL.End();
        }
    }

    public static void point(float[] a)
    {
        common();
        GL.Begin(GL.LINES);
        if (a.Length == 2)
        {
            GL.Vertex3(a[0] + 0.05f, a[1], 0);
            GL.Vertex3(a[0], a[1], 0);
        }
        else
        {
            GL.Vertex3(a[0] + 0.05f, a[1], a[2]);
            GL.Vertex3(a[0], a[1], a[2]);
        }
        GL.End();
    }
}
