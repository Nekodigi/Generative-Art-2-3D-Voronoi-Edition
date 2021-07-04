using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddConstraint : MonoBehaviour
{
    public static HEData AddConstraints(HEData triangleData, List<float[]> constraints, bool shouldRemoveTriangles)
    {
        List<HalfEdge> uniqueEdges = triangleData.GetUniqueEdges();
        for (int i = 0; i < constraints.Count; i++)
        {
            float[] c_p1 = constraints[i];
            float[] c_p2 = constraints[(i + 1) % constraints.Count];

            if (Geometory.IsEdgeInListOfEdges(uniqueEdges, c_p1, c_p2))
            {
                continue;
            }

            Queue<HalfEdge> intersectingEdges = FindIntersectingEdges_BruteForce(uniqueEdges, c_p1, c_p2);
            List<HalfEdge> newEdges = RemoveIntersectingEdges(c_p1, c_p2, intersectingEdges);

            //Step 4. Try to restore delaunay triangulation 
            //Because we have constraints we will never get a delaunay triangulation
            RestoreDelaunayTriangulation(c_p1, c_p2, newEdges);
        }
        if (shouldRemoveTriangles)
        {
            RemoveSuperfluousTriangles(triangleData, constraints);
        }

        return triangleData;
    }

    //Is an edge between p1 and p2 a constraint?
    public static bool IsEdgeAConstraint(float[] p1, float[] p2, List<float[]> constraints)
    {
        for (int i = 0; i < constraints.Count; i++)
        {
            float[] c_p1 = constraints[i];
            float[] c_p2 = constraints[(i + 1) % constraints.Count];

            if (Geometory.AreTwoEdgesTheSame(p1, p2, c_p1, c_p2))
            {
                return true;
            }
        }

        return false;
    }

    //
    // Remove all triangles that are inside the constraint
    //

    //This assumes the vertices in the constraint are ordered clockwise
    public static void RemoveSuperfluousTriangles(HEData triangleData, List<float[]> constraints)
    {
        //This assumes we have at least 3 vertices in the constraint because we cant delete triangles inside a line
        if (constraints.Count < 3)
        {
            return;
        }

        List<HEFace> trianglesToBeDeleted = FindTrianglesWithinConstraint(triangleData, constraints);

        //Delete the triangles
        foreach (HEFace t in trianglesToBeDeleted)
        {
            DeleteTriangleFace(t, triangleData, true);
        }
    }

    public static List<HalfEdge> RemoveIntersectingEdges(float[] v_i, float[] v_j, Queue<HalfEdge> intersectingEdges)
    {
        List<HalfEdge> newEdges = new List<HalfEdge>();

        int safety = 0;

        //While some edges still cross the constrained edge, do steps 3.1 and 3.2
        while (intersectingEdges.Count > 0)
        {
            safety += 1;

            if (safety > 100000)
            {
                Debug.LogError("Stuck in infinite loop when fixing constrained edges");
                break;
            }

            //Step 3.1. Remove an edge from the list of edges that intersects the constrained edge
            HalfEdge e = intersectingEdges.Dequeue();

            //The vertices belonging to the two triangles
            float[] v_k = e.v.pos;
            float[] v_l = e.prevEdge.v.pos;
            float[] v_3rd = e.nextEdge.v.pos;
            //The vertex belonging to the opposite triangle and isn't shared by the current edge
            float[] v_opposite_pos = e.oppositeEdge.nextEdge.v.pos;

            //Step 3.2. If the two triangles don't form a convex quadtrilateral
            //place the edge back on the list of intersecting edges (because this edge cant be flipped) 
            //and go to step 3.1
            if (!Geometory.IsQuadrilateralConvex(v_k, v_l, v_3rd, v_opposite_pos))
            {
                intersectingEdges.Enqueue(e);
                continue;
            }
            else
            {
                //Flip the edge like we did when we created the delaunay triangulation
                Triangle.FlipTriangleEdge(e);

                //The new diagonal is defined by the vertices
                float[] v_m = e.v.pos;
                float[] v_n = e.prevEdge.v.pos;

                //If this new diagonal intersects with the constrained edge, add it to the list of intersecting edges
                if (Intersection.IsCrossingEdge(v_i, v_j, v_m, v_n))
                {
                    intersectingEdges.Enqueue(e);
                }
                //Place it in the list of newly created edges
                else
                {
                    newEdges.Add(e);
                }
            }
        }

        return newEdges;
    }

    //
    // Try to restore the delaunay triangulation by flipping newly created edges
    //

    //This process is similar to when we created the original delaunay triangulation
    //This step can maybe be skipped if you just want a triangulation and Ive noticed its often not flipping any triangles
    public static void RestoreDelaunayTriangulation(float[] c_p1, float[] c_p2, List<HalfEdge> newEdges)
    {
        int safety = 0;

        int flippedEdges = 0;

        //Repeat 4.1 - 4.3 until no further swaps take place
        while (true)
        {
            safety += 1;

            if (safety > 100000)
            {
                Debug.LogError("Stuck in endless loop when delaunay after fixing constrained edges");

                break;
            }

            bool hasFlippedEdge = false;

            //Step 4.1. Loop over each edge in the list of newly created edges
            for (int j = 0; j < newEdges.Count; j++)
            {
                HalfEdge e = newEdges[j];

                //Step 4.2. Let the newly created edge be defined by the vertices
                float[] v_k = e.v.pos;
                float[] v_l = e.prevEdge.v.pos;

                //If this edge is FVector.equal to the constrained edge, then skip to step 4.1
                //because we are not allowed to flip a constrained edge
                if ((FVector.equal(v_k, c_p1) && FVector.equal(v_l, c_p2)) || (FVector.equal(v_l, c_p1) && FVector.equal(v_k, c_p2)))
                {
                    continue;
                }

                //Step 4.3. If the two triangles that share edge v_k and v_l don't satisfy the delaunay criterion,
                //so that a vertex of one of the triangles is inside the circumcircle of the other triangle, flip the edge
                //The third vertex of the triangle belonging to this edge
                float[] v_third_pos = e.nextEdge.v.pos;
                //The vertice belonging to the triangle on the opposite side of the edge and this vertex is not a part of the edge
                float[] v_opposite_pos = e.oppositeEdge.nextEdge.v.pos;

                //Test if we should flip this edge
                if (Geometory.ShouldFlipEdge(v_l, v_k, v_third_pos, v_opposite_pos))
                {
                    //Flip the edge
                    hasFlippedEdge = true;

                    Triangle.FlipTriangleEdge(e);

                    flippedEdges += 1;
                }
            }

            //We have searched through all edges and havent found an edge to flip, so we cant improve anymore
            if (!hasFlippedEdge)
            {
                //Debug.Log("Found a constrained delaunay triangulation in " + flippedEdges + " flips");

                break;
            }
        }
    }

    public static Queue<HalfEdge> FindIntersectingEdges_BruteForce(List<HalfEdge> uniqueEdges, float[] c_p1, float[] c_p2)
    {
        //Should be in a queue because we will later plop the first in the queue and add edges in the back of the queue 
        Queue<HalfEdge> intersectingEdges = new Queue<HalfEdge>();

        //Loop through all edges and see if they are intersecting with the constrained edge
        for (int i = 0; i < uniqueEdges.Count; i++)
        {
            //The edges the triangle consists of
            HalfEdge e = uniqueEdges[i];

            //The position the edge is going to
            float[] e_p1 = e.v.pos;
            //The position the edge is coming from
            float[] e_p2 = e.prevEdge.v.pos;

            //Is this edge intersecting with the constraint?
            if (Intersection.IsCrossingEdge(e_p1, e_p2, c_p1, c_p2))
            {
                //If so add it to the queue of edges
                intersectingEdges.Enqueue(e);
            }
        }

        return intersectingEdges;
    }

    public static List<HEFace> FindTrianglesWithinConstraint(HEData triangleData, List<float[]> constraints)
    {
        List<HEFace> trianglesToDelete = new List<HEFace>();


        //Step 1. Find a triangle with an edge that shares an edge with the first constraint edge in the list 
        //Since both are clockwise we know we are "inside" of the constraint, so this is a triangle we should delete
        HEFace borderTriangle = null;

        float[] c_p1 = constraints[0];
        float[] c_p2 = constraints[1];

        //Search through all triangles
        foreach (HEFace t in triangleData.faces)
        {
            //The edges in this triangle
            HalfEdge e1 = t.edge;
            HalfEdge e2 = e1.nextEdge;
            HalfEdge e3 = e2.nextEdge;

            //Is any of these edges a constraint? If so we have find the first triangle
            if (FVector.equal(e1.v.pos, c_p2) && FVector.equal(e1.prevEdge.v.pos, c_p1))
            {
                borderTriangle = t;

                break;
            }
            if (FVector.equal(e2.v.pos, c_p2) && FVector.equal(e2.prevEdge.v.pos, c_p1))
            {
                borderTriangle = t;

                break;
            }
            if (FVector.equal(e3.v.pos, c_p2) && FVector.equal(e3.prevEdge.v.pos, c_p1))
            {
                borderTriangle = t;

                break;
            }
        }

        if (borderTriangle == null)
        {
            return null;
        }



        //Step 2. Find the rest of the triangles within the constraint by using a flood fill algorithm

        //Maybe better to first find all the other border triangles?

        //We know this triangle should be deleted
        trianglesToDelete.Add(borderTriangle);

        //Store the triangles we flood filling in this queue
        Queue<HEFace> trianglesToCheck = new Queue<HEFace>();

        //Start at the triangle we know is within the constraints
        trianglesToCheck.Enqueue(borderTriangle);

        int safety = 0;

        while (true)
        {
            safety += 1;

            if (safety > 100000)
            {
                Debug.LogError("Stuck in infinite loop when looking for triangles within constraint");

                break;
            }

            //Stop if we are out of neighbors
            if (trianglesToCheck.Count == 0)
            {
                break;
            }

            //Pick the first triangle in the list and investigate its neighbors
            HEFace t = trianglesToCheck.Dequeue();

            //Investigate the triangles on the opposite sides of these edges
            HalfEdge e1 = t.edge;
            HalfEdge e2 = e1.nextEdge;
            HalfEdge e3 = e2.nextEdge;

            //A triangle is a neighbor within the constraint if:
            //- The neighbor is not an outer border meaning no neighbor exists
            //- If we have not already visited the neighbor
            //- If the edge between the neighbor and this triangle is not a constraint
            if (e1.oppositeEdge != null &&
                !trianglesToDelete.Contains(e1.oppositeEdge.face) &&
                !trianglesToCheck.Contains(e1.oppositeEdge.face) &&
                !IsEdgeAConstraint(e1.v.pos, e1.prevEdge.v.pos, constraints))//not to search beyond constraint
            {
                trianglesToCheck.Enqueue(e1.oppositeEdge.face);

                trianglesToDelete.Add(e1.oppositeEdge.face);
            }
            if (e2.oppositeEdge != null &&
                !trianglesToDelete.Contains(e2.oppositeEdge.face) &&
                !trianglesToCheck.Contains(e2.oppositeEdge.face) &&
                !IsEdgeAConstraint(e2.v.pos, e2.prevEdge.v.pos, constraints))
            {
                trianglesToCheck.Enqueue(e2.oppositeEdge.face);

                trianglesToDelete.Add(e2.oppositeEdge.face);
            }
            if (e3.oppositeEdge != null &&
                !trianglesToDelete.Contains(e3.oppositeEdge.face) &&
                !trianglesToCheck.Contains(e3.oppositeEdge.face) &&
                !IsEdgeAConstraint(e3.v.pos, e3.prevEdge.v.pos, constraints))
            {
                trianglesToCheck.Enqueue(e3.oppositeEdge.face);

                trianglesToDelete.Add(e3.oppositeEdge.face);
            }
        }

        return trianglesToDelete;
    }

    public static void DeleteTriangleFace(HEFace t, HEData data, bool shouldSetOppositeToNull)
    {
        //Update the data structure
        //In the half-edge data structure there's an edge going in the opposite direction
        //on the other side of this triangle with a reference to this edge, so we have to set these to null
        HalfEdge t_e1 = t.edge;
        HalfEdge t_e2 = t_e1.nextEdge;
        HalfEdge t_e3 = t_e2.nextEdge;

        //If we want to remove the triangle and create a hole
        //But sometimes we have created a new triangle and then we cant set the opposite to null
        if (shouldSetOppositeToNull)
        {
            if (t_e1.oppositeEdge != null)
            {
                t_e1.oppositeEdge.oppositeEdge = null;
            }
            if (t_e2.oppositeEdge != null)
            {
                t_e2.oppositeEdge.oppositeEdge = null;
            }
            if (t_e3.oppositeEdge != null)
            {
                t_e3.oppositeEdge.oppositeEdge = null;
            }
        }


        //Remove from the data structure

        //Remove from the list of all triangles
        data.faces.Remove(t);

        //Remove the edges from the list of all edges
        data.edges.Remove(t_e1);
        data.edges.Remove(t_e2);
        data.edges.Remove(t_e3);

        //Remove the vertices
        data.vertices.Remove(t_e1.v);
        data.vertices.Remove(t_e2.v);
        data.vertices.Remove(t_e3.v);
    }
}
