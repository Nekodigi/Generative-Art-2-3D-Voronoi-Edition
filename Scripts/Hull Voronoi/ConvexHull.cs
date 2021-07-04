using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class ConvexHull
{
    
    int dim;
    public List<Vertex> vertices = new List<Vertex>();
    public float[][] posStore;//store true position before scaling and set it after scaling because position must be match completely.
    public List<Simplex> simplexes = new List<Simplex>();
    public List<Polygon> polygons = new List<Polygon>();
    public float[] centroid;
    ObjectBuffer buffer;
    public bool useExtreme = false;//if use extreme, I can calculate speedy but, some time cause error(espacially few points)
    float scaleForCalc = 100.0f;//because input position scale is too small.

    public ConvexHull(int dim)
    {
        this.dim = dim;
        centroid = new float[dim];
    }

    public void show()
    {
        //stroke(360);
        foreach (Vertex vertex in vertices)
        {
            //GeomRender.point(vertex.pos);
        }
        //stroke(0, 100, 100);
        GeomRender.point(centroid);
        //stroke(0);
        //fill(360);
        foreach (Polygon poly in polygons)
        {
            poly.show();
        }
    }

    public void toGraph()
    {
        foreach (Simplex s in simplexes)
        {
            s.toGraph();
        }
    }

    //region GENERATE--------------------------------------------------------------
    #region Generate
    public void Generate(List<Vertex> input, bool assignIds = true, bool checkInput = false)
    {

        clear();//clear centroid, vertices, simplexes
        buffer = new ObjectBuffer(dim);

        if (input.Count < dim + 1) return;//points validation confilmation

        buffer.addInput(input, assignIds, checkInput);//register all input points

     posStore = new float[input.Count][];
        foreach (Vertex v in input)
        {
            posStore[v.id] = v.pos;
            v.pos = FVector.mult(v.pos, scaleForCalc);
        }   

        initConvexHull();//Please look source

        //Expand the convex hull and faces.
        while (buffer.unprocessedFaces.Count > 0)
        {
            Simplex currentFace = buffer.unprocessedFaces[0];
            buffer.currentVertex = currentFace.furthestVertex;

            updateCenter();//refer buffer.currentVertex
                           //the affected faces get tagged. the face has furthest point same side as normal.
            tagAffectedFaces(currentFace);//tagged face will delete and replace by new face

            //create the cone from the currentVertex and the affected faces horizon.
            if (!buffer.singularVertices.Contains(buffer.currentVertex) && createCone())
                commitCone();
            else
                handleSingular();

            //need to reset the tags
            for (int i = 0; i < buffer.affectedFaces.Count; i++) { buffer.affectedFaces[i].tag = 0; };
        }

        for (int i = 0; i < simplexes.Count; i++)
        {
            Simplex wrap = simplexes[i];
            wrap.tag = i;//set data simplexes
            wrap.calcCentroid();
            /*if (wrap.isNormalFlipped)
            {
                Vertex t = wrap.vertices[0];
                wrap.vertices[0] = wrap.vertices[2];
                wrap.vertices[2] = t;
            }*/
        }
        buffer = null;

        foreach (Vertex v in vertices)
        {
            //v.pos = FVector.mult(v.pos, 1.0f / scaleForCalc);
            v.pos = posStore[v.id];
        }
        polygons = HVUtils.simplex2Poly(simplexes);
    }

    void clear()
    {
        centroid = new float[dim];
        simplexes = new List<Simplex>();
        vertices = new List<Vertex>();
    }
    #endregion

    #region Initialization
    //region INITIALIZATION-------------------------------------
    //find the (dimension+1) initial points and create the simplexes
    void initConvexHull()
    {
        List<Vertex> initialPoints;
        if (useExtreme)
        {
            List<Vertex> extremes = findExtremes();//Bipolar vertex on each axis
            initialPoints = findInitialPoints((extremes));//Returns furthest extreme pair ...(Returns dimension+1 extremes)
        }
        else
        {
            initialPoints = findInitialPoints(buffer.inputVertices);//Returns furthest extreme pair ...(Returns dimension+1 extremes)
        }

        for (int i = 0; i < initialPoints.Count; i++)
        {
            buffer.currentVertex = initialPoints[i];
            updateCenter();//refer buffer.currentVertex
            vertices.Add(buffer.currentVertex);//add vertex to completed vertices
            buffer.inputVertices.Remove(buffer.currentVertex);//remove vertex from input vertices
        }
        //Create initial simplexes           //Record adjacent face at oppsite vertex of the face
        Simplex[] faces = initFaceDatabase();//Calculate normal distance between origin and first vertex. And flip the normal to point outward from the origin

        for (int i = 0; i < faces.Length; i++)
        {
            findBeyondVertices(faces[i]);
            if (faces[i].beyondVertices.Count == 0)
                simplexes.Add(faces[i]);
            else
                buffer.unprocessedFaces.Add(faces[i]);
        }
    }

    // Finds the extremes in all axis.
    List<Vertex> findExtremes()
    {
        List<Vertex> extremes = new List<Vertex>();
        List<int> ids = new List<int>();//for checking duplicate id

        for (int i = 0; i < dim; i++)
        {
            float min = float.PositiveInfinity, max = float.NegativeInfinity;
            int minInd = 0, maxInd = 0;

            for (int j = 0; j < buffer.inputVertices.Count; j++)
            {
                float v = buffer.inputVertices[j].pos[i];

                if (v < min)
                {
                    min = v;
                    minInd = j;
                }
                if (v > max)
                {
                    max = v;
                    maxInd = j;
                }
            }
            if (!HVUtils.contains(ids, minInd)) { extremes.Add(buffer.inputVertices[minInd]); ids.Add(minInd); }
            if (!HVUtils.contains(ids, maxInd)) { extremes.Add(buffer.inputVertices[maxInd]); ids.Add(maxInd); }
        }
        if (extremes.Count < dim + 1) Debug.LogError("couldn't find enough extremes");
        return extremes;
    }

    float getSqrDistSum(Vertex pivot, List<Vertex> initialPoints)
    {
        float sum = 0;
        for (int i = 0; i < initialPoints.Count; i++)
        {
            sum += FVector.sqrDist(initialPoints[i].pos, pivot.pos);
        }
        return sum;
    }

    List<Vertex> findInitialPoints(List<Vertex> extremes)
    {
        List<Vertex> initialPoints = new List<Vertex>();
        Vertex first = null; Vertex second = null;
        float maxDist = 0;

        for (int i = 0; i < extremes.Count - 1; i++)
        {
            Vertex a = extremes[i];
            for (int j = i + 1; j < extremes.Count; j++)
            {
                Vertex b = extremes[j];
                float dist = FVector.sqrDist(a.pos, b.pos);

                if (dist > maxDist)
                {
                    first = a;
                    second = b;
                    maxDist = dist;
                }
            }
        }

        initialPoints.Add(first);//record furthest extreme pair
        initialPoints.Add(second);

        for (int i = 2; i <= dim; i++)
        {
            float maximum = HVUtils.EPSILON;
            Vertex maxPoint = null;

            for (int j = 0; j < extremes.Count; j++)
            {
                Vertex extreme = extremes[j];
                if (initialPoints.Contains(extreme)) continue;

                float distS = getSqrDistSum(extreme, initialPoints);

                if (distS > maximum)
                {
                    maximum = distS;
                    maxPoint = extreme;
                }
            }

            if (maxPoint != null)
                initialPoints.Add(maxPoint);
            else
            {
                Debug.LogError("Singular input data error");
            }
        }
        return initialPoints;
    }

    //create the faces from (dimension + 1) vertices 
    Simplex[] initFaceDatabase()
    {
        Simplex[] faces = new Simplex[dim + 1];

        for (int i = 0; i < dim + 1; i++)
        {
            Vertex[] vertices_ = HVUtils.getNotIth(vertices, i).ToArray();//skips the i-th vertex. oppsite simplex
            Simplex newFace = new Simplex(dim);
            
            newFace.vertices = vertices_;

            Array.Sort(vertices_, new VertexIdComparer());
            calculateFacePlane(newFace);//Calculate normal, distance between origin and fast vertex. And flip the normal to point outward from centroid
            faces[i] = newFace;
        }
        //update the adjacency (check all pair of faces)
        for (int i = 0; i < dim; i++)
        {
            for (int j = i + 1; j < dim + 1; j++)
            {
                updateAdjacency(faces[i], faces[j]);//record adjacent face at oppsite vertex of the face
            }
        }
        return faces;
    }

    //Calculates the normal and offset of the hyper-plane given by the face's vertices.
    bool calculateFacePlane(Simplex face)
    {
        face.normal = FVector.calcNormal(face.vertices);

        if (float.IsNaN(face.normal[0]))
        {
            return false;
        }

        float offset = 0;
        float centerDistance = 0;
        offset = FVector.faceDist(face.vertices[0].pos, face);//distance between origin and face
        centerDistance = FVector.faceDist(centroid, face);
        face.offset = -offset;
        centerDistance -= offset;

        if (centerDistance > 0)
        {//flip normal
            face.normal = FVector.mult(face.normal, -1);
            face.offset = -face.offset;
            face.isNormalFlipped = true;
        }
        else face.isNormalFlipped = false;
        return true;//Finally, normal points outword from the centroid
    }

    void updateAdjacency(Simplex L, Simplex R)
    {
        Vertex[] Lv = L.vertices;
        Vertex[] Rv = R.vertices;
        int i;
        //reset marks on the face
        L.setAllVerticesTag(0);
        R.setAllVerticesTag(1);

        //find the 1st vertex is not touching R
        for (i = 0; i < dim; i++) if (Lv[i].tag == 0) break;

        //all vertices were touching
        if (i == dim) return;

        //check if only 1 vertex wasn't touching bacause we'll get face oppsite 1 vertex
        for (int j = i + 1; j < dim; j++) if (Lv[j].tag == 0) return;

        //if we are here, the two faces share an edge
        L.adjacent[i] = R;

        //update the adj. face on the other face - find the vertex that remains marked
        L.setAllVerticesTag(0);
        for (i = 0; i < dim; i++)
        {
            if (Rv[i].tag == 1) break;//because we'll get face oppsite 1 vertex
        }
        R.adjacent[i] = L;
    }

    //get vertices that positive side on face(same side as normal)
    void findBeyondVertices(Simplex face)
    {
        face.clearBeyond();

        for (int i = 0; i < buffer.inputVertices.Count; i++)
            isBeyond(face, buffer.inputVertices[i]);
    }

    #endregion

    #region Process
    //region PROCESS-----------------------------------------
    //tags all faces seem from the current vertex with 1
    void tagAffectedFaces(Simplex currentFace)
    {//https://www.flickr.com/photos/187510519@N02/49677924438/in/album-72157713549835228/
        buffer.affectedFaces.Clear();
        buffer.affectedFaces.Add(currentFace);
        Stack<Simplex> traverseStack = new Stack<Simplex>();
        traverseStack.Push(currentFace);
        currentFace.tag = 1;
        while (traverseStack.Count > 0)
        {
            Simplex top = traverseStack.Pop();

            for (int i = 0; i < dim; i++)
            {
                Simplex adjFace = top.adjacent[i];

                if (adjFace == null) Debug.LogError("(2) Adjacent Face should never be null");
                //check adjacent face isn't contained affectedFace, And current vertex is positive side of adjacent face
                if (adjFace.tag == 0 && FVector.faceDist(buffer.currentVertex.pos, adjFace) >= HVUtils.EPSILON)
                {
                    buffer.affectedFaces.Add(adjFace);
                    adjFace.tag = 1;
                    traverseStack.Push(adjFace);
                }
            }
        }
    }

    bool createCone()
    {
        int currentVertexIndex = buffer.currentVertex.id;
        buffer.coneFaces = new List<DeferredSimplex>();
        Simplex[] updateBuffer = new Simplex[dim];
        int[] updateIndices = new int[dim];
        //oldFaces = affectedFaces
        for (int fi = 0; fi < buffer.affectedFaces.Count; fi++)
        {
            Simplex oldFace = buffer.affectedFaces[fi];
            //find the faces that need to be updated
            int updateCount = 0;
            for (int i = 0; i < dim; i++)
            {
                Simplex af = oldFace.adjacent[i];

                if (af == null) Debug.LogError("(3) Adjacent Face should never be null");

                if (af.tag == 0)
                {//tag == 0 when oldFaces does not coutain af
                    updateBuffer[updateCount] = af;
                    updateIndices[updateCount] = i;
                    updateCount++;
                }
            }


            for (int i = 0; i < updateCount; i++)
            {
                Simplex adjacentFace = updateBuffer[i];
                int oldFaceAdjacentIndex = 0;
                for (int j = 0; j < dim; j++)
                {
                    if (oldFace == adjacentFace.adjacent[j])
                    {
                        oldFaceAdjacentIndex = j;//adjFace join oldFace with edge oppsite j th vertex
                        break;
                    }
                }
                //index of the face that corresponds to this adjacent face
                int forbidden = updateIndices[i];

                Simplex newFace = new Simplex(dim);
                Vertex[] vertices_ = newFace.vertices;//refer
                for (int j = 0; j < dim; j++)//vertices.count = dimension - 1
                    vertices_[j] = oldFace.vertices[j];

                int oldVertexIndex = vertices_[forbidden].id;

                int orderedPivotIndex;

                //correct the ordering for simplex connector
                if (currentVertexIndex < oldVertexIndex)
                {
                    orderedPivotIndex = 0;
                    for (int j = forbidden - 1; j >= 0; j--)
                    {
                        if (vertices_[j].id > currentVertexIndex) vertices_[j + 1] = vertices_[j];
                        else
                        {
                            orderedPivotIndex = j + 1;
                            break;
                        }
                    }
                }
                else
                {
                    orderedPivotIndex = dim - 1;
                    for (int j = forbidden + 1; j < dim; j++)
                    {
                        if (vertices_[j].id < currentVertexIndex) vertices_[j - 1] = vertices_[j];
                        else
                        {
                            orderedPivotIndex = j - 1;
                            break;
                        }
                    }
                }

                vertices_[orderedPivotIndex] = buffer.currentVertex;

                if (!calculateFacePlane(newFace))
                {//calculate face data, and check illigal input
                    return false;
                }

                buffer.coneFaces.Add(new DeferredSimplex(newFace, orderedPivotIndex, adjacentFace, oldFaceAdjacentIndex, oldFace));
            }
        }
        return true;
    }

    //Commits a cone and adds a vertex to the convex hull.
    void commitCone()
    {
        //add the current vertex
        vertices.Add(buffer.currentVertex);

        //fill the adjacency.
        for (int i = 0; i < buffer.coneFaces.Count; i++)
        {
            DeferredSimplex face = buffer.coneFaces[i];

            Simplex newFace = face.face;
            Simplex adjacentFace = face.pivot;
            Simplex oldFace = face.oldFace;
            int orderedPivotIndex = face.faceIndex;

            newFace.adjacent[orderedPivotIndex] = adjacentFace;//set adjacent each other
            adjacentFace.adjacent[face.pivotIndex] = newFace;
            //let there be a connection
            for (int j = 0; j < dim; j++)
            {
                if (j == orderedPivotIndex) continue;//because already set adjacent
                SimplexConnector connector = new SimplexConnector(newFace, j, dim);//connector is a class for searching the corresponding face at high speed with hash
                connectFace(connector);
            }

            //this could slightly help. Accelerate calculations by narrowing down choices
            if (adjacentFace.beyondVertices.Count < oldFace.beyondVertices.Count)
            {
                findBeyondVertices(newFace, adjacentFace.beyondVertices, oldFace.beyondVertices);
            }
            else
            {
                findBeyondVertices(newFace, oldFace.beyondVertices, adjacentFace.beyondVertices);
            }
           
            //this face will definitely lie on the hull
            if (newFace.beyondVertices.Count == 0)
            {
                simplexes.Add(newFace);
                buffer.unprocessedFaces.Remove(newFace);
                newFace.beyondVertices = new List<Vertex>();
            }
            else
            {//add the face to the list
                buffer.unprocessedFaces.Add(newFace);
            }
        }

        //delete the affected faces.
        for (int fi = 0; fi < buffer.affectedFaces.Count; fi++)
        {
            buffer.unprocessedFaces.Remove(buffer.affectedFaces[fi]);
        }
    }

    void connectFace(SimplexConnector connector)
    {
        int index = connector.hashCode % buffer.connector_table_size;
        List<SimplexConnector> list = buffer.connectorTable[index];
        //check foreach connector
        for (int i = 0; i < list.Count; i++)
        {
            SimplexConnector current = list[i];
            if (SimplexConnector.areConnectable(connector, current, dim))
            {
                list.Remove(current);
                SimplexConnector.connect(current, connector);
                return;
            }
        }
        list.Add(connector);
    }

    void findBeyondVertices(Simplex face, List<Vertex> beyond, List<Vertex> beyond1)
    {
        face.clearBeyond();
        Vertex v;

        for (int i = 0; i < beyond1.Count; i++)
            beyond1[i].tag = 1;

        buffer.currentVertex.tag = 0;

        for (int i = 0; i < beyond.Count; i++)
        {
            v = beyond[i];
            if (v == buffer.currentVertex) continue;
            v.tag = 0;//prevent duplicate check
            isBeyond(face, v);
        }

        for (int i = 0; i < beyond1.Count; i++)
        {
            v = beyond1[i];
            if (v.tag == 1) isBeyond(face, v);
        }
    }

    void handleSingular()
    {
        rollbackCenter();
        buffer.singularVertices.Add(buffer.currentVertex);

        //this means that all the affected faces must be on the hull and that all their "vertices beyond" are singular.
        for (int fi = 0; fi < buffer.affectedFaces.Count; fi++)
        {
            Simplex face = buffer.affectedFaces[fi];
            List<Vertex> bv = face.beyondVertices;
            for (int i = 0; i < bv.Count; i++)
            {
                buffer.singularVertices.Add(bv[i]);
            }

            simplexes.Add(face);
            buffer.unprocessedFaces.Remove(face);
            face.beyondVertices = new List<Vertex>();

        }
    }

    void isBeyond(Simplex face, Vertex v)
    {
        float dist = FVector.faceDist(v.pos, face);

        if (dist >= HVUtils.EPSILON)
        {
            if (dist > face.maxDist)
            {
                face.maxDist = dist;
                face.furthestVertex = v;
            }
            face.beyondVertices.Add(v);
        }
    }

    void updateCenter()
    {
        int count = vertices.Count + 1;
        centroid = FVector.mult(centroid, count - 1);
        centroid = FVector.div(FVector.add(centroid, buffer.currentVertex.pos), count);
    }

    void rollbackCenter()
    {
        int count = vertices.Count + 1;
        centroid = FVector.mult(centroid, count);
        centroid = FVector.div(FVector.sub(centroid, buffer.currentVertex.pos), count - 1);
    }
    #endregion
}
