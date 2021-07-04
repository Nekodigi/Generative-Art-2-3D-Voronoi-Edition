using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PolygonDraw : MonoBehaviour
{
    Texture2D canvas_;
    float[,] canvas = new float[100, 50];
    GenPolygon gp = new GenPolygon();
    private Material lineMaterial;
    public GameObject debugObj;
    // Start is called before the first frame update
    void Start()
    {
        lineMaterial = new Material(Shader.Find("Hidden/Internal-Colored"));
        lineMaterial.hideFlags = HideFlags.HideAndDontSave;
        // Turn on alpha blending
        lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        // Turn backface culling off
        lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
        // Turn off depth writes
        lineMaterial.SetInt("_ZWrite", 0);
        lineMaterial.SetPass(0);


        canvas_ = new Texture2D(400, 200, TextureFormat.ARGB32, false);
        Color[] colors = new Color[canvas_.width * canvas_.height];
        canvas_.SetPixels(colors);
    }

    private void Update()
    {


        //Debug.Log(pos.x + ":" + pos.y);
        //float time = Time.realtimeSinceStartup;

        //Debug.Log("Pro1"+(Time.realtimeSinceStartup - time));
        //time = Time.realtimeSinceStartup;
        //Debug.Log("Pro2" + (Time.realtimeSinceStartup - time));
        //time = Time.realtimeSinceStartup;
        /*ConnectedComponentLabeliing ccl = new ConnectedComponentLabeliing();
        int[,] labels = ccl.Generate(gp.binaryCanvas);
        for (int i = 0; i < canvas.GetLength(0); i++)
        {
            for (int j = 0; j < canvas.GetLength(1); j++)
            {
                canvas_.SetPixel(i, j, Color.HSVToRGB(labels[i,j]/10.0f, 1, 1));
            }
        }
        canvas_.Apply();
        debugObj.GetComponent<RawImage>().texture = canvas_;*/
    }

    // Update is called once per frame
    void OnPostRender()
    {
        lineMaterial.SetPass(0);

        if (Input.GetMouseButton(0))
        {

            Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition) * canvas.GetLength(1)/5.0f/2.0f;
            pos.z = 0;
            float power = 0.5f;
            float radius = 5f;
            for (int i = 0; i < canvas.GetLength(0); i++)//y=5 in ortho x<10?
            {
                for (int j = 0; j < canvas.GetLength(1); j++)
                {
                    float dist = Vector3.Distance(pos, new Vector2(i - canvas.GetLength(0) / 2, j - canvas.GetLength(1) / 2));
                    canvas[i, j] = Mathf.Clamp(1 - dist / radius, 0, 1f) * power + canvas[i, j];//
                }
            }
        }

        List<List<Vector2>>[] polygonss = gp.marchingSquare(canvas);
        //print(polygonss.Count);
        foreach(List<List<Vector2>> polygons in polygonss)
        foreach(List<Vector2> polygon in polygons)
        {
            GL.Begin(GL.LINES);
            foreach(Vector2 point in polygon)
            {
                GL.Vertex(point);
            }
            GL.End();
        }
    }

    
}
