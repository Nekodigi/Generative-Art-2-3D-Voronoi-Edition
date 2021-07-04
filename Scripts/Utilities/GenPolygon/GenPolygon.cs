using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenPolygon//Generate Polygon from raster
{
    float threshold = 0.5f;//

    float rescale;//scale down from pixel scale to world scale
    Vector2 off = new Vector2(-10, -5);
    float[,] canvas = new float[400, 200];
    int ix, iy;
    List<Vector2> points = new List<Vector2>();
    Vector2 a, b, c, d;
    int marchdir = 0;//0= ^up 1= >right 2= \/down 3= <left 
    bool[,] checkout;
    public int[,] binaryCanvas;
    int[,] labels;
    int labelCount;
    ConnectedComponentLabeliing ccl = new ConnectedComponentLabeliing();

    public GenPolygon()
    {
    }

    public List<List<Vector2>>[] marchingSquare(float[,] canvas, bool reverse = false)//base on child are set
    {
        this.canvas = canvas;
        this.binaryCanvas = new int[canvas.GetLength(0), canvas.GetLength(1)];
        this.rescale = canvas.GetLength(1) / 5.0f / 2.0f;
        
        fillEdge();
        checkout = new bool[canvas.GetLength(0), canvas.GetLength(1)];
        for(int i=0; i<canvas.GetLength(0); i++)
        {
            for(int j=0; j<canvas.GetLength(1); j++)
            {
                binaryCanvas[i, j] = canvas[i, j] < threshold ? 0 : 1;
            }
        }

        this.labels = ccl.Generate(binaryCanvas);
        this.labelCount = ccl.currentLabel;
        List<List<Vector2>>[] polygons = new List<List<Vector2>>[labelCount];

        for (int i = 0; i < canvas.GetLength(0) * canvas.GetLength(1); i++)
        {
            ix = i % canvas.GetLength(0);
            iy = i / canvas.GetLength(0);


            if (ix == canvas.GetLength(0) - 1 || iy == canvas.GetLength(1) - 1) continue;
            int c00 = binaryCanvas[ix, iy];//00, 10
            int c10 = binaryCanvas[ix+1, iy];//01, 11
            int c11 = binaryCanvas[ix+1, iy+1];
            int c01 = binaryCanvas[ix, iy+1];

            if (checkout[ix, iy] == false && c00 == 0 && c10 == 0 && c01 == 0 && c11 == 1)
            {
                //scanLoop
                if (reverse)
                {
                    marchdir = 3;
                }
                else
                {
                    marchdir = 0;
                }
                List<Vector2> polygon = scanLoop();
                if (polygons[labels[ix + 1, iy + 1] - 1] == null) polygons[labels[ix + 1, iy + 1] - 1]=new List<List<Vector2>>();
                polygons[labels[ix+1, iy+1]-1].Add(polygon);
                /*GL.Begin(GL.LINES);
                foreach (Vector2 point in polygon)
                {
                    //Debug.Log(point/40.0f);
                    GeomRender.vertex(point / 20.0f - new Vector2(10, 5));
                }
                GL.End();*/
            }
            else if (checkout[ix, iy] == false && c00 == 1 && c10 == 1 && c01 == 1 && c11 == 0)
            {
                //scanLoop
                if (reverse)
                {
                    marchdir = 0;
                }
                else
                {
                    marchdir = 3;
                }
                List<Vector2> polygon = scanLoop();
                if (polygons[labels[ix, iy] - 1] == null) polygons[labels[ix, iy] - 1] = new List<List<Vector2>>();
                polygons[labels[ix, iy]-1].Add(polygon);
               
                /*GL.Begin(GL.LINES);
                foreach (Vector2 point in polygon)
                {
                    GeomRender.vertex(point / 20.0f - new Vector2(10, 5));
                }
                GL.End();*/
            }
        }
        return polygons;
    }

    List<Vector2> scanLoop()
    {
        int six = ix;
        int siy = iy;
        int safety = 0;
        points = new List<Vector2>();
        while (true)
        {
            float x = ix + 0.5f;
            float y = iy + 0.5f;
            //red as brightness
            float a_val = canvas[ix, iy];
            float b_val = canvas[ix + 1, iy];
            float c_val = canvas[ix + 1, iy + 1];
            float d_val = canvas[ix, iy + 1];
            int at = binaryCanvas[ix, iy];//00, 10
            int bt = binaryCanvas[ix + 1, iy] ;//01, 11
            int ct = binaryCanvas[ix + 1, iy + 1];
            int dt = binaryCanvas[ix, iy + 1];

            int state = getState(at, bt, ct, dt);

            float amt = (threshold - a_val) / (b_val - a_val);
            a = new Vector2(Mathf.Lerp(x, x + 1, amt), y)/rescale+off;

            amt = (threshold - b_val) / (c_val - b_val);
            b = new Vector2(x + 1, Mathf.Lerp(y, y + 1, amt)) / rescale + off;

            amt = (threshold - d_val) / (c_val - d_val);
            c = new Vector2(Mathf.Lerp(x, x + 1, amt), y + 1) / rescale + off;

            amt = (threshold - a_val) / (d_val - a_val);
            d = new Vector2(x, Mathf.Lerp(y, y + 1, amt)) / rescale + off;

            switch (state)
            {
                case 1:
                    if (marchdir == 1) marchDir(2);
                    else if (marchdir == 0) marchDir(3);
                    break;
                case 2:
                    checkout[ix, iy] = true;
                    if (marchdir == 0) marchDir(1);
                    else if (marchdir == 3) marchDir(2);
                    break;
                case 3:
                    if (marchdir == 1) marchDir(1);
                    else if (marchdir == 3) marchDir(3);
                    break;
                case 4:
                    if (marchdir == 2) marchDir(1);
                    else if (marchdir == 3) marchDir(0);
                    break;
                case 5:  //render both but use only one at once
                    checkout[ix, iy] = true;
                    if (marchdir == 2) marchDir(3);
                    else if (marchdir == 1) marchDir(0);
                    else if (marchdir == 0) marchDir(1);
                    else if (marchdir == 3) marchDir(2);
                    break;
                case 6:
                    if (marchdir == 0) marchDir(0);
                    else if (marchdir == 2) marchDir(2);
                    break;
                case 7:
                    checkout[ix, iy] = true;
                    if (marchdir == 2) marchDir(3);
                    else if (marchdir == 1) marchDir(0);
                    break;
                case 8:
                    checkout[ix, iy] = true;
                    if (marchdir == 2) marchDir(3);
                    else if (marchdir == 1) marchDir(0);
                    break;
                case 9:
                    if (marchdir == 0) marchDir(0);
                    else if (marchdir == 2) marchDir(2);
                    break;
                case 10:
                    if (marchdir == 2) marchDir(1);
                    else if (marchdir == 3) marchDir(0);
                    else if (marchdir == 1) marchDir(2);
                    else if (marchdir == 0) marchDir(3);
                    break;
                case 11:
                    if (marchdir == 2) marchDir(1);
                    else if (marchdir == 3) marchDir(0);
                    break;
                case 12:
                    if (marchdir == 1) marchDir(1);
                    else if (marchdir == 3) marchDir(3);
                    break;
                case 13:
                    checkout[ix, iy] = true;
                    if (marchdir == 0) marchDir(1);
                    else if (marchdir == 3) marchDir(2);
                    break;
                case 14:
                    if (marchdir == 1) marchDir(2);
                    else if (marchdir == 0) marchDir(3);
                    break;
            }
            if (ix == six && iy == siy) break;
            if (safety++ >= 10000)
            {
                Debug.LogWarning("not safety" + ix + ":" + iy);
                return null;
            }
        }
        return points;
    }

    void marchDir(int val)
    {
        marchdir = val;
        switch (val)
        {
            case 0:
                iy -= 1;
                points.Add(a);
                break;
            case 1:
                ix += 1;
                points.Add(b);
                break;
            case 2:
                iy += 1;
                points.Add(c);
                break;
            case 3:
                ix -= 1;
                points.Add(d);
                break;
        }
    }

    void fillEdge()
    {//fill edge with black to make calculation simple
        for (int i = 0; i < canvas.GetLength(0); i++)
        {
            canvas[i, 0] = 0;
            canvas[i, canvas.GetLength(1) - 1] = 0;
        }
        for (int j = 0; j < canvas.GetLength(1); j++)
        {
            canvas[0, j] = 0;
            canvas[canvas.GetLength(0) - 1, j] = 0;
        }
    }

    int getState(int a, int b, int c, int d)
    {
        return a * 8 + b * 4 + c * 2 + d * 1;
    }
}