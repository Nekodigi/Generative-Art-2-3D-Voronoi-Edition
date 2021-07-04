using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectBuffer
{
    static int connector_table_size_ = 2017;
    public int connector_table_size = 2017;
    public List<SimplexConnector>[] connectorTable = new List<SimplexConnector>[connector_table_size_];
    int dim;
    public Vertex currentVertex;
    public List<Vertex> inputVertices = new List<Vertex>();
    public float maxDist = float.NegativeInfinity;
    public Vertex furthestVertex;
    public List<Simplex> unprocessedFaces = new List<Simplex>();
    //To detect invalid input in advance and reduce processing
    public List<Vertex> singularVertices = new List<Vertex>();
    //faces that need to be change
    public List<Simplex> affectedFaces = new List<Simplex>();
    //To store unconfirmed cone face data
    public List<DeferredSimplex> coneFaces = new List<DeferredSimplex>();

    public ObjectBuffer(int dim)
    {
        this.dim = dim;
        for (int i = 0; i < connector_table_size; i++)
        {
            connectorTable[i] = new List<SimplexConnector>();
        }
    }

    public void addInput(List<Vertex> input, bool assignIds, bool checkInput)
    {
        inputVertices = new List<Vertex>(input);

        if (assignIds)
        {
            for (int i = 0; i < input.Count; i++)
            {
                inputVertices[i].id = i;
            }
        }


        //Check for duplicates
        if (checkInput)
        {
            HashSet<int> set = new HashSet<int>();

            for (int i = 0; i < input.Count; i++)
            {
                if (input[i] == null) Debug.LogError("Input has a null vertex");
                if (input[i].dim != dim) Debug.LogError("Input vertex is not the correct dimension" + input[i].dim);
                if (set.Contains(input[i].id)) Debug.LogError("Input vertex id is not unique" + input[i].id);
                else set.Add(input[i].id);
            }
        }
    }
}

public class DeferredSimplex
{
    public Simplex face;
    public Simplex pivot;
    public Simplex oldFace;
    public int faceIndex;
    public int pivotIndex;

    public DeferredSimplex(Simplex face, int faceIndex, Simplex pivot, int pivotIndex, Simplex oldFace)
    {
        this.face = face;
        this.faceIndex = faceIndex;
        this.pivot = pivot;
        this.pivotIndex = pivotIndex;
        this.oldFace = oldFace;
    }
}