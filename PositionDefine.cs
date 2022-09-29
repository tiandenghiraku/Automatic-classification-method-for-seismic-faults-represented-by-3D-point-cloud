using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionDefine : MonoBehaviour
{
    public Vector2 AxisRotation;
    public Vector3 CamPos;
    public Transform Axis1, Axis2, TCamera;
    public string Title;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Axis1.localEulerAngles = Vector3.up * AxisRotation.x;
        Axis2.localEulerAngles = Vector3.right * AxisRotation.y;
        TCamera.localPosition = CamPos;
    }
    public void Set(Vector2 AR,Vector3 Pos)
    {
        AxisRotation = AR;
        CamPos = Pos;
    }
}
