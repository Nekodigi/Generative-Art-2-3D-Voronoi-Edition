using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMain : MonoBehaviour
{
    Vector3 pmousePos;//previous mouse position
    Vector3 angleTarget = new Vector3(0, 0);
    Vector3 angle = new Vector3(0, 0);
    float distTarget = 10;
    float dist = 10;
    Quaternion rotationTarget = new Quaternion();
    public static bool freeze = false;
    public static GameObject thisObj;
    // Start is called before the first frame update

        private void Awake()
    {
        thisObj = gameObject;
    }
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (!freeze)
        {
            if (Input.GetMouseButtonDown(0))
            {
                pmousePos = Input.mousePosition;
            }

            Quaternion temp = transform.rotation;
            transform.rotation = rotationTarget;
            if (Input.GetMouseButton(0))
            {
                Vector3 diff = (Input.mousePosition - pmousePos) / Camera.main.pixelHeight * 100f;
                pmousePos = Input.mousePosition;
                angleTarget += new Vector3(diff.x, diff.y);//add lerp
                transform.Rotate(Vector3.up, diff.x);//rotate quarternion to rotate in any angles.
                transform.Rotate(Vector3.left, diff.y);
            }
            rotationTarget = transform.rotation;
            transform.rotation = temp;
            transform.rotation = Quaternion.Lerp(transform.rotation, rotationTarget, 0.1f);
            angle = transform.eulerAngles;
            float phi = -angle.y / 180 * Mathf.PI - Mathf.PI / 2;
            float theta = -angle.x / 180 * Mathf.PI + Mathf.PI / 2;
            transform.position = new Vector3(dist * Mathf.Cos(phi) * Mathf.Sin(theta), dist * Mathf.Cos(theta), dist * Mathf.Sin(phi) * Mathf.Sin(theta));
            distTarget *= 1.0f - Input.mouseScrollDelta.y * 0.1f;
            dist = Mathf.Lerp(dist, distTarget, 0.1f);
        }
    }

    public static void Freeze()
    {
        thisObj.transform.position = new Vector3(0, 0, -10);
        thisObj.transform.eulerAngles = new Vector3(0, 0, 0);
        freeze = true;
    }
}
