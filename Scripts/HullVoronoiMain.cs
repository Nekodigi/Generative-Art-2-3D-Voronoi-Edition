using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Advertisements;
using System.Threading;

public class HullVoronoiMain : MonoBehaviour
{
    
    int numVertices = 300;
    float size = 4f;//0.1
    float relaxF = 0.5f;
    float noiseS = 1;
    float curl2df = 0.05f;
    float spamtime = 0;

    public List<Vertex> vertices = new List<Vertex>();

    private Material lineMaterial;

    ConvexHull hull;
    Delaunay delaunay;
    Voronoi voronoi;
    SphericalVoronoi sVoronoi;

    AStar astar = new AStar();
    ExampleConstrainedDelaunay ecd2;
    List<List<float[]>>[] constraintss;
    float[,] canvas = new float[100, 50];
    GenPolygon genPoly = new GenPolygon();

    public GameObject baseObj_;//set voronoi 3d ...mesh to this object
    public static GameObject baseObj;
    public GameObject castObj_;
    public static GameObject castObj;
    public GameObject obstacleObj_;
    public static GameObject obstacleObj;
    public AudioClip[] breakSEs;

    public static float off = 0;
    int index;

    public Dropdown Ddimension;//UI
    public Dropdown Dtype;
    public GameObject Orelax;
    public Toggle Trelax;
    public GameObject Oaster;
    public Toggle Tastar;
    public GameObject Ohole;
    public Toggle Thole;
    public GameObject Ocell;
    public Toggle Tcell;
    public Button Bregenerate;
    public Slider Snum;
    public GameObject Otoolbox;
    public CanvasGroup Ctoolbox;

    float toolboxTimer = 0;

    Camera cam;

    // Start is called before the first frame update
    void Start()
    {
        Advertisement.Initialize("4138659", false);
        lineMaterial = new Material(Shader.Find("Hidden/Internal-Colored"));
        //lineMaterial.hideFlags = HideFlags.HideAndDontSave;
        //lineMaterial.shader.hideFlags = HideFlags.HideAndDontSave;
        // Turn on alpha blending
        //lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        //lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        // Turn backface culling off
        lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
        // Turn off depth writes
        //lineMaterial.SetInt("_ZWrite", 0);
        //lineMaterial.SetPass(0);


        baseObj = baseObj_;
        castObj = castObj_;
        obstacleObj = obstacleObj_;
        Debug.Log(castObj.GetComponent<MeshFilter>().mesh.vertices);

    }

    private void Update()
    {
        spamtime -= Time.deltaTime;
        toolboxTimer += Time.deltaTime;
        if (Input.GetAxis("Mouse X") != 0) {
            if (toolboxTimer <= 0.2f) ;
            else if (toolboxTimer <= 3.8f) toolboxTimer = 0.2f;
            else if (toolboxTimer > 3.8f && toolboxTimer <= 4.0f) toolboxTimer = (4.0f - toolboxTimer);
            else toolboxTimer = 0f;
        }
        if(Input.mousePosition.y < 0)
        {
            toolboxTimer = 4;
        }
        if (Ddimension.value == 1 && Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                if (Dtype.value == 0)
                {
                    delaunay.Release3D();
                }
                else
                {
                    voronoi.Release3D();
                }
                Transform objectHit = hit.transform;
                //Debug.Log(hit.point);
                GameObject obstacle = Instantiate(obstacleObj, hit.point, Quaternion.identity);
                AudioSource auso = gameObject.GetComponent<AudioSource>();
                auso.clip = breakSEs[Random.Range(0, breakSEs.Length)];
                auso.Play();
            }
        }
        if (toolboxTimer <= 0.2) Ctoolbox.alpha = toolboxTimer / 0.2f;
        else if (toolboxTimer > 3.8 && toolboxTimer <= 4.0) Ctoolbox.alpha = (4.0f - toolboxTimer) / 0.2f;
        else if (toolboxTimer <= 4.0) Ctoolbox.alpha = 1;
        else Ctoolbox.alpha = 0;
    }

    void OnPostRender()
    {
        if (vertices.Count == 0) ResetAll();
        lineMaterial.SetPass(0);
        //float targetOff = Input.mousePosition.x / Camera.main.pixelWidth * 5.0f;
        float targetOff = 1;
        off = Mathf.Lerp(off, targetOff, 0.1f);
        

        switch (Ddimension.value)
        {
            case 0:
                try
                {
                    delaunay = new Delaunay(2);
                    delaunay.Generate(vertices);
                    voronoi = new Voronoi(2);
                    voronoi.Generate(delaunay);
                }
                catch (System.Exception e)
                {
                    ResetAll();
                    //Debug.Log(e);
                    //relax(fac);
                }//generate

                if (Trelax.isOn)//update vertices
                {
                    foreach (Vertex v in vertices)
                    {
                        Vector3 vec = CurlNoise.curlNoise(v.pos[0] / noiseS, v.pos[1] / noiseS, 0, 0);
                        v.pos = FVector.add(v.pos, FVector.setMag(FVector.set(vec), curl2df));
                        v.vertexRelax(vertices);
                    }
                    /*for (int i = vertices.Count - 1; i >= 0; i--)//it make singular input!!! need some action next
                    {
                        Vertex v = vertices[i];
                        //float[] t = { constrain(v.pos[0], -origin.x - 100, -origin.x + width + 100), constrain(v.pos[1], -origin.y - 100, -origin.y + height + 100) };
                        cam = Camera.main;
                        float height = cam.orthographicSize;//height/2
                        float width = height * cam.aspect;
                        float[] t = { Mathf.Clamp(v.pos[0], -width - 1f, width + 1f), Mathf.Clamp(v.pos[1], -height - 1f, height + 1f) };
                        v.pos = t;
                    }*/
                    
                    foreach (Polygon polygon in voronoi.polygons)
                    {
                        polygon.relax(0.5f);
                    }
                }
                else if (Tastar.isOn)
                {

                }else
                {
                    foreach (Vertex v in vertices)
                    {
                        //Vector3 vec = CurlNoise.curlNoise(v.pos[0] / noiseS, v.pos[1] / noiseS, 0, 0);
                        //v.pos = FVector.add(v.pos, FVector.setMag(FVector.set(vec), curl2df));
                        v.update();
                    }
                }

                if (Tastar.isOn)
                {
                    List<Vertex> vs = new List<Vertex>();
                    switch (Dtype.value)
                    {
                        case 0:
                            delaunay.toGraph();
                            vs = delaunay.vertices;
                            break;
                        case 1:
                            voronoi.toGraph();
                            vs = voronoi.vertices;
                            break;
                    }
                    Vertex current = vs[0];
                    Random.InitState(System.DateTime.Now.Millisecond);
                    Vertex end = vs[Random.Range(0, vs.Count)];//println(end.adj.size());
                    astar.solve(current, end);
                    GeomRender.stroke.a = 1;
                    //Debug.Log(Random.Range(0, vs.Count));
                    astar.show();
                }else if (Thole.isOn)
                {
                    if (Input.GetMouseButton(0))
                    {

                        Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition) * canvas.GetLength(1) / 5.0f / 2.0f; ;
                        pos.z = 0;
                        float power = 0.5f;
                        float r = 5f;
                        for (int i = 0; i < canvas.GetLength(0); i++)//y=5 in ortho x<10?
                        {
                            for (int j = 0; j < canvas.GetLength(1); j++)
                            {
                                float dist = Vector3.Distance(pos, new Vector2(i - canvas.GetLength(0) / 2, j - canvas.GetLength(1) / 2));
                                canvas[i, j] = -Mathf.Clamp(1 - dist / r, 0, 1f) * power + canvas[i, j];//
                            }
                        }
                    }

                    List<List<Vector2>>[] polygonss = genPoly.marchingSquare(canvas, false);
                    //(polygons.Count);

                    constraintss = new List<List<float[]>>[polygonss.Length];
                    int count = 0;
                    foreach (List<List<Vector2>> polygons_ in polygonss)
                    {
                        List<List<float[]>> constraints = new List<List<float[]>>();
                        foreach (List<Vector2> polygon in polygons_)
                        {
                            //if (polygons.Count > 0)
                            //{
                            List<float[]> constraint = new List<float[]>();
                            foreach (Vector2 v in polygon)
                            {
                                float[] v_ = {v.x, v.y };
                                constraint.Add(v_);
                            }
                            constraints.Add(constraint);
                            //}
                        }
                        constraintss[count++] = constraints;
                    }

                    List<Polygon> polygons = new List<Polygon>();
                    List<Vertex> vertices_ = new List<Vertex>();

                    foreach (List<List<float[]>> constraints in constraintss)
                    {
                        vertices_ = new List<Vertex>(vertices);
                        foreach (List<float[]> constraint in constraints)
                        {
                            foreach (float[] v in constraint)
                            {
                                vertices_.Add(new Vertex(0, v));
                            }
                        }
                        Delaunay delaunay = new Delaunay(2);
                        delaunay.Generate(vertices_);
                        HEData he = new HEData(delaunay.simplexes);
                        foreach (List<float[]> constraint in constraints)//heavy...
                        {
                            he = AddConstraint.AddConstraints(he, constraint, true);
                        }
                        //convet to simplex and to poly
                        SimplexVertices constrained = he.toSimplexes();

                        delaunay.simplexes = constrained.simplexes;
                        delaunay.polygons = HVUtils.simplex2Poly(delaunay.simplexes);

                        polygons.AddRange(HVUtils.simplex2Poly(constrained.simplexes));
                    }


                    foreach (Polygon poly in polygons)
                    {
                        poly.show();
                    }
                }

                if (!Tastar.isOn && !Thole.isOn)//render
                {
                    switch (Dtype.value)
                    {
                        case 0:
                            delaunay.show(Input.GetMouseButton(0));
                            break;
                        case 1:
                            voronoi.show(Input.GetMouseButton(0));
                            break;
                    }
                }

                break;
            case 1:
                try
                {
                    switch (Dtype.value)
                    {
                        case 0:
                            /*if (index == 0)
                            {
                                foreach (Simplex s in delaunay.simplexes)
                                {
                                    s.clip();
                                }
                            }
                            index++;*/
                            if (index < delaunay.simplexes.Count)
                            {
                                delaunay.simplexes[index].clip();
                            }
                            index++;
                            break;
                        case 1:
                            if (index < voronoi.regions.Count)
                            {
                                voronoi.regions[index].clip();
                            }
                            index++;
                            break;
                    }
                }
                catch
                {
                    
                }
                break;
            case 2:
                hull.Generate(vertices);
                sVoronoi.Generate(hull);

                if (Trelax.isOn)
                {
                    foreach (Vertex v in vertices)
                    {
                        Vector3 vec = CurlNoise.curlNoise(v.pos[0] / noiseS, v.pos[1] / noiseS, v.pos[2] / noiseS, 0);
                        v.pos = FVector.add(v.pos, FVector.setMag(FVector.set(vec), 0.05f));
                        v.pos = FVector.setMag(v.pos, size);
                    }
                    hull.Generate(vertices);
                    sVoronoi.Generate(hull);
                    foreach (Polygon polygon in sVoronoi.polygons)
                    {
                        polygon.relax(relaxF);
                    }
                    for (int i = vertices.Count - 1; i >= 0; i--)
                    {
                        Vertex v = vertices[i];
                        v.pos = FVector.setMag(v.pos, size);
                    }
                    foreach (Vertex v in vertices)
                    {
                        v.vertexRelax(vertices);
                    }

                    switch (Dtype.value)
                    {
                        case 0:
                            hull.show();
                            break;
                        case 1:
                            sVoronoi.show();
                            break;
                    }
                }
                else if(Tastar.isOn)//astar
                {
                    List<Vertex> vs = new List<Vertex>();
                    switch (Dtype.value)
                    {
                        case 0:
                            hull.toGraph();
                            vs = hull.vertices;
                            break;
                        case 1:
                            sVoronoi.toGraph();
                            vs = sVoronoi.vertices;
                            break;
                    }
                    Vertex current = vs[0];
                    Random.InitState(System.DateTime.Now.Millisecond);
                    Vertex end = vs[Random.Range(0, vs.Count)];//println(end.adj.size());
                    astar.solve(current, end);
                    GeomRender.stroke.a = 1;
                    //Debug.Log(Random.Range(0, vs.Count));
                    astar.show();
                }
                break;
        }

    }

    public void ResetAll()
    {
        if (spamtime > 0) return;
        spamtime = 1f;
        
        numVertices = (int)Mathf.Pow(10, Snum.value);
        GeomRender.filla = 1;
        GeomRender.strokea = 0;
        index = 0;//for clip

        float h2 = 6.0f;//height / 2 5.0 in default (ortho)
        float w2 = 6.0f * Camera.main.aspect;
        vertices = new List<Vertex>();
        astar = new AStar();
        for (int i = 0; i < canvas.GetLength(0); i++)//y=5 in ortho x<10?
        {
            for (int j = 0; j < canvas.GetLength(1); j++)
            {
                canvas[i, j] = 1;//
            }
        }
        foreach(GameObject obj in GameObject.FindGameObjectsWithTag("Fragment"))    
        {
            Destroy(obj);
        }
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Obstacle"))
        {
            Destroy(obj);
        }

        switch (Ddimension.value)
        {
            case 0://2d
                CameraMain.Freeze(); Camera.main.orthographic = true;
                if (Trelax.isOn)
                {
                    float distr = 0.1f;
                    for (int i = 0; i < numVertices; i++)
                    {
                        //vertices.Add(new Vertex(0, Random.Range(-w2, w2), Random.Range(-h2, h2)));//id will be assigned later
                        vertices.Add(new Vertex(0, Random.Range(-distr, distr), Random.Range(-distr, distr)));//id will be assigned later
                    }
                }
                else
                {
                    for (int i = 0; i < numVertices; i++)
                    {
                        vertices.Add(new Vertex(0, Random.Range(-w2, w2), Random.Range(-h2, h2)));//id will be assigned later
                    }
                }
                break;
            case 1://3d
                CameraMain.freeze = false;Camera.main.orthographic = false;
                if (Dtype.value == 0) numVertices = Mathf.Min(50, numVertices);//won't work regenerate?
                if (Dtype.value == 1) numVertices = Mathf.Min(300, numVertices);
                for (int i = 0; i < numVertices; i++)
                {
                    vertices.Add(new Vertex(0, Random.Range(-size, size), Random.Range(-size, size), Random.Range(-size, size)));//id will be assigned later
                }
                try
                {
                    delaunay = new Delaunay(3);
                    delaunay.Generate(vertices);
                    voronoi = new Voronoi(3);
                    voronoi.Generate(delaunay);
                    switch (Dtype.value)
                    {
                        case 0:
                            delaunay.Gen3DModel();
                            break;
                        case 1:
                            voronoi.Gen3DModel();
                            break;
                    }
                }
                catch
                { }
                break;
            case 2://spherical
                CameraMain.freeze = false; Camera.main.orthographic = false;
                GeomRender.filla = 0.8f;
                GeomRender.strokea = 0.6f;
                hull = new ConvexHull(3);
                sVoronoi = new SphericalVoronoi();
                if (Trelax.isOn)
                {
                    for (int i = 0; i < numVertices; i++)//99
                    {
                        vertices.Add(new Vertex(0, FVector.mult(HVUtils.sphereSampling(Random.Range(-1.0f, -0.99f), Random.Range(0, Mathf.PI * 2)), size)));
                    }
                    for (int i = 0; i < 30; i++)
                    {
                        vertices.Add(new Vertex(0, FVector.mult(HVUtils.sphereSampling(Random.Range(-1.0f, 1.0f), Random.Range(0, Mathf.PI * 2)), size)));
                    }
                }
                else
                {
                    for (int i = 0; i < numVertices; i++)
                    {
                        vertices.Add(new Vertex(0, FVector.mult(HVUtils.sphereSampling(Random.Range(-1.0f, 1.0f), Random.Range(0, Mathf.PI * 2)), size)));
                    }
                }
                break;
        }
    }
    public void UpdateToggle()
    {
        if (Advertisement.IsReady())
        {
            Advertisement.Show();
        }
        //Debug.Log(Ddimension.value);
        //Debug.Log(Dtype.value);
        switch (Ddimension.value)
        {
            case 0:
                switch (Dtype.value)
                {
                    case 0:
                        Orelax.SetActive(true);
                        Oaster.SetActive(true);
                        Ohole.SetActive(true);
                        Ocell.SetActive(true);
                        break;
                    case 1:
                        Orelax.SetActive(true);
                        Oaster.SetActive(true);
                        Ohole.SetActive(false);
                        Ocell.SetActive(true);
                        break;
                }
                break;
            case 1:
                Orelax.SetActive(false);
                Oaster.SetActive(false);
                Ohole.SetActive(false);
                Ocell.SetActive(false);
                break;
            case 2:
                Orelax.SetActive(true);
                Oaster.SetActive(true);
                Ohole.SetActive(false);
                Ocell.SetActive(false);
                break;
        }
        ResetAll();
    }

    
}
