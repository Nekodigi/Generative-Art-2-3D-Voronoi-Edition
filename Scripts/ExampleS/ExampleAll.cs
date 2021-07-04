using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExampleAll : MonoBehaviour
{
    int dim = 2;//2=2d, 3=3d, 4=spherical
    ConvexHull hull;
    Delaunay delaunay;
    Voronoi voronoi;
    ConvexHull sHull;
    SphericalVoronoi sVoronoi;
    //action relax//it will disable a star don't do when astar=true
    bool astar = false;//find shortest path
    //action constrain reset astar when update
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
