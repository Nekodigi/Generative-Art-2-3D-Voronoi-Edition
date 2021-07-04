using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Parabox.CSG;

public class Test : MonoBehaviour
{
    // Start is called before the first frame update
    private Material lineMaterial;
    ExampleConvexHull ec2;
    ExampleDelaunay ed2;
    ExampleVoronoi ev2;
    ExampleSphericalVoronoi esv2;
    List<List<float[]>>[] constraintss;
    ExampleConstrainedDelaunay ecd2;
    AStar astar = new AStar();
    public GameObject baseObj_;//set voronoi 3d ...mesh to this object
    public static GameObject baseObj;
    public GameObject castObj_;
    public static GameObject castObj;
    public GameObject obstacleObj_;
    public static GameObject obstacleObj;
    public AudioClip[] breakSEs;

    float[,] canvas = new float[100, 50];
    GenPolygon genPoly = new GenPolygon();
    public List<Vertex> vertices = new List<Vertex>();
    int numVertices = 300;
    float size = 5f;//0.1
    public static float off = 0;
    int index;

    float[] set(params float[] x)
    {
        return x;
    }

    private void Start()
    {
        lineMaterial = new Material(Shader.Find("Hidden/Internal-Colored"));
        lineMaterial.hideFlags = HideFlags.HideAndDontSave;
        lineMaterial.shader.hideFlags = HideFlags.HideAndDontSave;
        // Turn on alpha blending
        lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        // Turn backface culling off
        lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
        // Turn off depth writes
        lineMaterial.SetInt("_ZWrite", 0);
        lineMaterial.SetPass(0);

        baseObj = baseObj_;
        castObj = castObj_;
        obstacleObj = obstacleObj_;

        float h2 = 6.0f;//height / 2 5.0 in default (ortho)
        float w2 = 6.0f * Camera.main.aspect;
        for (int i = 0; i < numVertices; i++)
        {
            switch (3)
            {
                case 2:
                    vertices.Add(new Vertex(0, Random.Range(-w2, w2), Random.Range(-h2, h2)));//id will be assigned later
                    break;
                case 3:
                    vertices.Add(new Vertex(0, Random.Range(-size, size), Random.Range(-size, size), Random.Range(-size, size)));//id will be assigned later
                    break;
            }
        }


        ecd2 = new ExampleConstrainedDelaunay();
        ec2 = new ExampleConvexHull(2);
        ed2 = new ExampleDelaunay(2);
        ev2 = new ExampleVoronoi(3);//delaunay 3d automaticly generated.... fix... it
        //ev2.voronoi.Gen3DModel();
        //ev2.voronoi.Release3D();
        //ed2.delaunay.Gen3DModel();
        esv2 = new ExampleSphericalVoronoi();
        for (int i = 0; i < canvas.GetLength(0); i++)//y=5 in ortho x<10?
        {
            for (int j = 0; j < canvas.GetLength(1); j++)
            {
                canvas[i, j] = 1;//
            }
        }
    }
    void OnPostRender()
    {
        
        lineMaterial.SetPass(0);
        //float targetOff = Input.mousePosition.x / Camera.main.pixelWidth * 5.0f;
        float targetOff = 1;
        off = Mathf.Lerp(off, targetOff, 0.1f);

        /*if (index < ev2.voronoi.regions.Count)
        {
            ev2.voronoi.regions[index].clip();
        }
        index++;*/

        

        /*constraints.Add(set(-1 * 1f, 0));
        constraints.Add(set(0, 1 * 1));
        constraints.Add(set(1 * 1, 0));
        constraints.Add(set(0.2f * 1, -0.8f * 1));
        constraints.Add(set(0, -1 * 1));*/
        //constraints.Add(set(-0.8f, -0.4f));


        float time = Time.realtimeSinceStartup;
        /*Delaunay delaunay = new Delaunay(3);
        delaunay.Generate(vertices);
        Voronoi voronoi = new Voronoi(3);
        voronoi.Generate(delaunay);
        *///ev2.relax(0.9f);
        //ev2.show();
        //voronoi.relax();
        //voronoi.show();
        //voronoi.show();
        //Debug.Log(Time.realtimeSinceStartup-time);
        time = Time.realtimeSinceStartup;
        //delaunay.show();
        //Debug.Log("Time+"+(Time.realtimeSinceStartup - time));
        time = Time.realtimeSinceStartup;
        /*foreach (Vertex v in vertices)
        {
            v.update();
        }*/
        //Debug.Log("Time1+" + (Time.realtimeSinceStartup - time));
        time = Time.realtimeSinceStartup;

        //ecd2.Generate(constraints, false);
        //ecd2.canvas = canvas;

        //GL.LoadIdentity();
        //GL.MultMatrix(GetComponent<Camera>().worldToCameraMatrix);
        //GL.LoadProjectionMatrix(GetComponent<Camera>().projectionMatrix);
        //ev2.voronoi.toGraph();
        esv2.sVoronoi.toGraph();
        List<Vertex> vs = esv2.sVoronoi.vertices;
        Vertex current = vs[0];
        Random.InitState(System.DateTime.Now.Millisecond);
        Vertex end = vs[Random.Range(0, vs.Count)];//println(end.adj.size());
        astar.solve(current, end);
        time = Time.realtimeSinceStartup;
        GeomRender.stroke.a = 1;
        Debug.Log(Random.Range(0, vs.Count));
        astar.show();
        //esv2.sVoronoi.show();
        //ecd2.show();

        //delaunay.show();
        //Debug.Log("Time3+" + (Time.realtimeSinceStartup - time));
        time = Time.realtimeSinceStartup;
        //ec2.hull.show();
        //ev2.voronoi.show();
        //esv2.relax(0.5f);
        //ecd2.delaunay.show();
        //voronoi.show();
        //ecd2.delaunay.show();
        //GeomRender.stroke.a = 0;
        //ev2.voronoi.delaunay.show();
        //Debug.Log(ev2.delaunay.hull.dim);
        //ev2.voronoi.show();

        
        //esv2.Generate();
        //esv2.sVoronoi.show();
        //esv2.sVoronoi.hull.show();
        //ev2.relax(0.5f);
        //Debug.Log(ed2.delaunay.simplexes.Count);
        //ed2.delaunay.show();
        //esv2.relax(0.5f);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            /*foreach (Region region in ev2.voronoi.regions)
            {
                region.release();
            }*/
            /*foreach(Simplex s in ed2.delaunay.simplexes)
            {
                s.release();
            }*/
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                Transform objectHit = hit.transform;
                Debug.Log(hit.point);
                GameObject obstacle = Instantiate(obstacleObj, hit.point, Quaternion.identity);
                //ev2.voronoi.Release3D();
                //ed2.delaunay.Release3D();
                // Do something with the object that was hit by the raycast.
            }
            AudioSource auso = gameObject.GetComponent<AudioSource>();
            auso.clip = breakSEs[Random.Range(0, breakSEs.Length)];
            auso.Play();
            //Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            //constraints.Add(set(pos.x, pos.y));

            //ecd2 = new ExampleConstrainedDelaunay(constraints);
            //Debug.Log(pos);
        }
    }
}
