using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Intersection : MonoBehaviour
{
    public enum IntersectionCases
    {
        IsInside,
        IsOnEdge,
        NoIntersection
    }

    public static IntersectionCases PointCircle(float[] a, float[] b, float[] c, float[] testPoint)
    {
        //Center of circle
        float[] circleCenter = Geometory.CalculateCircleCenter(a, b, c);

        //The radius sqr of the circle
        float radiusSqr = FVector.sqrDist(a, circleCenter);

        //The distance sqr from the point to the circle center
        float distPointCenterSqr = FVector.sqrDist(testPoint, circleCenter);

        //Add/remove a small value becuse we will never be exactly on the edge because of floating point precision issues
        //Mutiply epsilon by two because we are using sqr root???
        if (distPointCenterSqr < radiusSqr - HVUtils.EPSILON * 2f)
        {
            return IntersectionCases.IsInside;
        }
        else if (distPointCenterSqr > radiusSqr + HVUtils.EPSILON * 2f)
        {
            return IntersectionCases.NoIntersection;
        }
        else
        {
            return IntersectionCases.IsOnEdge;
        }
    }

    //pre check
    public static bool isIntersect(float[] s1, float[] e1, float[] s2, float[] e2)
    {
        float det0 = (e1[0] - s1[0]) * (s2[1] - e1[1]) - (e1[1] - s1[1]) * (s2[0] - e1[0]);
        float det1 = (e1[0] - s1[0]) * (e2[1] - e1[1]) - (e1[1] - s1[1]) * (e2[0] - e1[0]);
        return det0 < 0 && det1 >= 0 || det0 >= 0 && det1 < 0;
    }

    public static float[] intersection(float[] p1s, float[] p1e, float[] p2s, float[] p2e, bool check)
    {
        if (check && !isIntersect(p1s, p1e, p2s, p2e)) return null;
        float x1 = p1s[0]; float y1 = p1s[1];
        float x2 = p1e[0]; float y2 = p1e[1];
        float x3 = p2s[0]; float y3 = p2s[1];
        float x4 = p2e[0]; float y4 = p2e[1];
        //float vx1 = x1 - x2;float vy1 = y1 - y2;
        //float vx2 = x3 - x4;float vy2 = y3 - y4;
        float den = (x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4);
        //float den = vx1 * vy2 - vy1 * vx2;
        if (den == 0)
        {
            return null;
        }

        float t = ((x1 - x3) * (y3 - y4) - (y1 - y3) * (x3 - x4)) / den;
        float u = -((x1 - x2) * (y1 - y3) - (y1 - y2) * (x1 - x3)) / den;
        if (t > 0 && t < 1 && u > 0)
        {
            float[] pt = new float[2];
            pt[0] = x1 + t * (x2 - x1);
            pt[1] = y1 + t * (y2 - y1);
            return pt;
        }
        else
        {
            return null;
        }
        //return intersectionV2(p1s, p1s.sub(p1e), p2s, p2s.sub(p2e));
    }
    //is edge clossing edge
    public static bool IsCrossingEdge(float[] s1, float[] e1, float[] s2, float[] e2)
    {
        if (FVector.equal(s1, s2) || FVector.equal(s1, e2) || FVector.equal(e1, s2) || FVector.equal(e1, e2)) return false;
        return intersection(s1, e1, s2, e2, true) != null;
    }
}
