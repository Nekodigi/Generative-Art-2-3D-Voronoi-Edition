using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Geometory : MonoBehaviour
{
    public static float[] CalculateCircleCenter(float[] a, float[] b, float[] c)
    {
        //Make sure the triangle a-b-c is counterclockwise
        if (!IsTriangleOrientedClockwise(a, b, c))
        {
            //Swap two vertices to change orientation
            float[] t = a;
            a = b;
            b = t;

            //Debug.Log("Swapped vertices");
        }


        //The area of the triangle
        float X_1 = b[0] - a[0];
        float X_2 = c[0] - a[0];
        float Y_1 = b[1] - a[1];
        float Y_2 = c[1] - a[1];

        float A = 0.5f * HVUtils.Det2(X_1, Y_1, X_2, Y_2);

        //Debug.Log(A);


        //The center coordinates:
        //float L_10 = MyVector2.Magnitude(b - a);
        //float L_20 = MyVector2.Magnitude(c - a);

        //float L_10_square = L_10 * L_10;
        //float L_20_square = L_20 * L_20;

        float L_10_square = FVector.sqrMag(FVector.sub(b, a));
        float L_20_square = FVector.sqrMag(FVector.sub(c, a));

        float one_divided_by_4_A = 1f / (4f * A);

        float x = a[0] + one_divided_by_4_A * ((Y_2 * L_10_square) - (Y_1 * L_20_square));
        float y = a[1] + one_divided_by_4_A * ((X_1 * L_20_square) - (X_2 * L_10_square));

        float[] center = { x, y };

        return center;
    }

    public static bool ShouldFlipEdge(float[] a, float[] b, float[] c, float[] d)
    {
        bool shouldFlipEdge = false;

        //Use the circle test to test if we need to flip this edge
        //We should flip if d is inside a circle formed by a, b, c
        Intersection.IntersectionCases intersectionCases = Intersection.PointCircle(a, b, c, d);

        if (intersectionCases == Intersection.IntersectionCases.IsInside)
        {
            //Are these the two triangles forming a convex quadrilateral? Otherwise the edge cant be flipped
            if (IsQuadrilateralConvex(a, b, c, d))
            {
                //If the new triangle after a flip is not better, then dont flip
                //This will also stop the algorithm from ending up in an endless loop
                Intersection.IntersectionCases intersectionCases2 = Intersection.PointCircle(b, c, d, a);

                if (intersectionCases2 == Intersection.IntersectionCases.IsOnEdge || intersectionCases2 == Intersection.IntersectionCases.IsInside)
                {
                    shouldFlipEdge = false;
                }
                else
                {
                    shouldFlipEdge = true;
                }
            }
        }

        return shouldFlipEdge;
    }

    public static bool IsEdgeInListOfEdges(List<HalfEdge> edges, float[] p1, float[] p2)
    {
        for (int i = 0; i < edges.Count; i++)
        {
            //The vertices positions of the current triangle
            float[] e_p1 = edges[i].v.pos;
            float[] e_p2 = edges[i].prevEdge.v.pos;

            //Check if edge has the same coordinates as the constrained edge
            //We have no idea about direction so we have to check both directions
            if (AreTwoEdgesTheSame(p1, p2, e_p1, e_p2))
            {
                return true;
            }
        }

        return false;
    }

    public static bool AreTwoEdgesTheSame(float[] e1_p1, float[] e1_p2, float[] e2_p1, float[] e2_p2)
    {
        //Is e1_p1 part of this constraint?
        if ((FVector.equal(e1_p1, e2_p1) || FVector.equal(e1_p1, e2_p2)))
        {
            //Is e1_p2 part of this constraint?
            if ((FVector.equal(e1_p2, e2_p1) || FVector.equal(e1_p2, e2_p2)))
            {
                return true;
            }
        }

        return false;
    }

    public static List<Simplex> OrientTrianglesClockwise(List<Simplex> triangles)
    {
        //Convert to list or we will no be able to update the orientation
        //List<Simplex> trianglesList = new List<Simplex>(triangles);

        for (int i = 0; i < triangles.Count; i++)
        {
            Simplex t = triangles[i];

            if (!IsTriangleOrientedClockwise(t.vertices[0].pos, t.vertices[1].pos, t.vertices[2].pos))
            {
                t.ChangeOrientation();

                //triangles[i] = t;

                //Debug.Log("Changed orientation");
            }
        }
        return triangles;
    }

    public static bool IsQuadrilateralConvex(float[] a, float[] b, float[] c, float[] d)
    {
        bool isConvex = false;

        //Convex if the convex hull includes all 4 points - will require just 4 determinant operations
        //In this case we dont kneed to know the order of the points, which is better
        //We could split it up into triangles, but still messy because of interior/exterior angles
        //Another version is if we know the edge between the triangles that form a quadrilateral
        //then we could measure the 4 angles of the edge, add them together (2 and 2) to get the interior angle
        //But it will still require 8 magnitude operations which is slow
        //From: https://stackoverflow.com/questions/2122305/convex-hull-of-4-points
        bool abc = IsTriangleOrientedClockwise(a, b, c);
        bool abd = IsTriangleOrientedClockwise(a, b, d);
        bool bcd = IsTriangleOrientedClockwise(b, c, d);
        bool cad = IsTriangleOrientedClockwise(c, a, d);

        if (abc && abd && bcd & !cad)
        {
            isConvex = true;
        }
        else if (abc && abd && !bcd & cad)
        {
            isConvex = true;
        }
        else if (abc && !abd && bcd & cad)
        {
            isConvex = true;
        }
        //The opposite sign, which makes everything inverted
        else if (!abc && !abd && !bcd & cad)
        {
            isConvex = true;
        }
        else if (!abc && !abd && bcd & !cad)
        {
            isConvex = true;
        }
        else if (!abc && abd && !bcd & !cad)
        {
            isConvex = true;
        }


        return isConvex;
    }

    //based on this site https://stackoverflow.com/questions/1165647/how-to-determine-if-a-list-of-polygon-points-are-in-clockwise-order
    public static bool isPolygonOrientedClokwise(List<float[]> ps)
    {//assume 2d case
        float sum = 0;
        for (int i = 0; i < ps.Count; i++)
        {
            float[] A = ps[i];
            float[] B = ps[(i + 1) % ps.Count];
            sum += (B[0] - A[0]) * (B[1] + A[1]);
        }
        return sum >= 0;
    }

    public static List<float[]> changePolygonOrient(List<float[]> ps)
    {
        List<float[]> result = new List<float[]>();
        for (int i = ps.Count - 1; i >= 0; i--)
        {
            result.Add(ps[i]);
        }
        return result;
    }

    public static bool IsTriangleOrientedClockwise(float[] p1, float[] p2, float[] p3)
    {
        bool isClockWise = true;

        float determinant = p1[0] * p2[1] + p3[0] * p1[1] + p2[0] * p3[1] - p1[0] * p3[1] - p3[0] * p2[1] - p2[0] * p1[1];

        if (determinant > 0f)
        {
            isClockWise = false;
        }

        return isClockWise;
    }
}
