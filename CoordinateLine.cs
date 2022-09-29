using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoordinateLine : MonoBehaviour
{
    public int X, Y, Z,Resolution;
    public LineRenderer LineInstant;
    public Transform TCamera;
    Transform XAxis, XAxisReverse, YAxis, ZAxis;
    Transform XAxisL, YAxisL, ZAxisL;
    Transform XYAxis, XZAxis, YXAxis, YZAxis, ZXAxis,ZYAxis;
    public GameObject TextMeshInstant;
    public List<TextMesh> Y_XTexts, Z_XTexts, Y_Z0Texts, Y_ZMTexts, Z_Y0Texts,Z_YMTexts;
    TextMesh XName, YName, ZName;
    public float NamePosAdjust;
    public int NameFontSize;
    public string XAxisName, YAxisName, ZAxisName;
    public bool Dont0;
    // Start is called before the first frame update
    void Start()
    {
        if (Resolution == 0)
        {
            Debug.LogError("Res==0");
        }
        else
        {
            Y_XTexts = new List<TextMesh>();
            Z_XTexts = new List<TextMesh>();
            Y_Z0Texts = new List<TextMesh>();
            Y_ZMTexts=new List<TextMesh>();
            Z_Y0Texts = new List<TextMesh>();
            Z_YMTexts = new List<TextMesh>();
            Init();
        }
    }

    // Update is called once per frame
    void Update()
    {
        bool ToX = TCamera.forward.x > 0;
        XAxis.position = Vector3.right * (ToX ? X : 0);
        XAxisReverse.position = Vector3.right * (!ToX ? X : 0);
        /*Quaternion LookQuaternion = Quaternion.LookRotation(ToX ? Vector3.right : Vector3.left, Vector3.left);
        foreach (TextMesh Texts in XTexts)
        {
            Texts.transform.rotation = LookQuaternion;
        }
        */
        bool ToY = TCamera.forward.y > 0;
        bool ToZ = TCamera.forward.z > 0;

        YAxis.position = Vector3.up * (ToY ? Y : 0);
        Quaternion YLook = Quaternion.LookRotation(ToY ? Vector3.up : Vector3.down, Vector3.left);

        ZAxis.position = Vector3.forward * (ToZ ? Z : 0);
        Quaternion ZLook = Quaternion.LookRotation(ToZ ? Vector3.forward : Vector3.back, Vector3.left);

        foreach (TextMesh Texts in Y_Z0Texts)
        {
            Texts.gameObject.SetActive(ToZ);
            Texts.transform.rotation = YLook;
            Texts.anchor = !ToY ? TextAnchor.MiddleRight : TextAnchor.MiddleLeft;
            Texts.alignment = !ToY ? TextAlignment.Right : TextAlignment.Left;
        }
        foreach (TextMesh Texts in Y_ZMTexts)
        {
            Texts.gameObject.SetActive(!ToZ);
            Texts.transform.rotation = YLook;
            Texts.anchor = ToY ? TextAnchor.MiddleRight : TextAnchor.MiddleLeft;
            Texts.alignment = ToY ? TextAlignment.Right : TextAlignment.Left;
        }
        foreach (TextMesh Texts in Y_XTexts)
        {
            Texts.transform.position = new Vector3(Texts.transform.position.x, Texts.transform.position.y, ToZ ? Z : 0);
            Texts.transform.rotation = ZLook;
            Texts.anchor = !ToX ? TextAnchor.UpperCenter : TextAnchor.LowerCenter;
        }
        
        foreach (TextMesh Texts in Z_Y0Texts)
        {
            Texts.gameObject.SetActive(ToY);
            Texts.transform.rotation = ZLook;
            Texts.anchor = ToZ ? TextAnchor.MiddleRight : TextAnchor.MiddleLeft;
            Texts.alignment = ToZ ? TextAlignment.Right : TextAlignment.Left;
        }
        foreach (TextMesh Texts in Z_YMTexts)
        {
            Texts.gameObject.SetActive(!ToY);
            Texts.transform.rotation = ZLook;
            Texts.anchor = !ToZ ? TextAnchor.MiddleRight : TextAnchor.MiddleLeft;
            Texts.alignment = !ToZ ? TextAlignment.Right : TextAlignment.Left;
        }
        foreach (TextMesh Texts in Z_XTexts)
        {
            Texts.transform.position = new Vector3(Texts.transform.position.x, ToY ? Y : 0, Texts.transform.position.z);
            Texts.transform.rotation = YLook;
            Texts.anchor = !ToX ? TextAnchor.UpperCenter : TextAnchor.LowerCenter;
        }
        YName.transform.position = new Vector3(ToX ? -NamePosAdjust * X : (1 + NamePosAdjust) * X, Y / 2, ToZ ? Z : 0);
        YName.anchor = !ToX ? TextAnchor.UpperCenter : TextAnchor.LowerCenter;
        YName.transform.rotation = ZLook;
        YName.fontSize = NameFontSize;

        ZName.transform.position = new Vector3(ToX ? -NamePosAdjust*X : (1+NamePosAdjust)*X, ToY ? Y : 0, Z/2);
        ZName.anchor= !ToX ? TextAnchor.UpperCenter : TextAnchor.LowerCenter;
        ZName.transform.rotation = YLook;
        ZName.fontSize = NameFontSize;

        XName.anchor = ToY==ToZ ? TextAnchor.MiddleLeft : TextAnchor.MiddleRight;
        XName.transform.position = new Vector3(X / 2, ToY ? Y : 0, !ToZ ? (1 + NamePosAdjust) * Z : -NamePosAdjust * Z);
        XName.transform.rotation = YLook;
        XName.fontSize = NameFontSize;
    }

    void Init()
    {
        X = ReSetByResolution(X, Resolution);
        Y = ReSetByResolution(Y, Resolution);
        Z = ReSetByResolution(Z, Resolution);

        XAxis = new GameObject("XAxis").transform;
        XAxisReverse = new GameObject("XAxisReverse").transform;
        YAxis = new GameObject("YAxis").transform;
        ZAxis = new GameObject("ZAxis").transform;

        XAxisL = new GameObject("XAxisL").transform;
        XAxisL.parent = XAxis;
        YAxisL = new GameObject("YAxisL").transform;
        YAxisL.parent = YAxis;
        ZAxisL = new GameObject("ZAxisL").transform;
        ZAxisL.parent = ZAxis;

        XAxis.parent = YAxis.parent = ZAxis.parent = transform;

        XAxis.localPosition = YAxis.localPosition = ZAxis.localPosition = Vector3.zero;


        XYAxis = new GameObject("XYAxis").transform;
        XZAxis = new GameObject("XZAxis").transform;
        XYAxis.parent = XZAxis.parent = XAxis;

        YXAxis = new GameObject("YXAxis").transform;
        YZAxis = new GameObject("YZAxis").transform;
        YXAxis.parent = YZAxis.parent = YAxis;

        ZXAxis = new GameObject("ZXAxis").transform;
        ZYAxis = new GameObject("ZYAxis").transform;
        ZXAxis.parent = ZYAxis.parent = ZAxis;

        LineInstant.positionCount = 2;
        LineInstant.SetPosition(0, Vector3.zero);
        LineInstant.SetPosition(1, Vector3.zero);

        for (int ix = 0; ix <= X; ix += Resolution)
        {
            Vector3 Pos0 = new Vector3(ix, 0, 0);
            Vector3 Pos1 = new Vector3(ix, Y, 0);
            Vector3 Pos2 = new Vector3(ix, 0, Z);

            SetLine(Pos0, Pos1, Pos2, YAxisL, ZAxisL);

            TextMesh Y0 = Instantiate(TextMeshInstant).GetComponent<TextMesh>();
            TextMesh Z0 = Instantiate(TextMeshInstant).GetComponent<TextMesh>();
            TextMesh YMax = Instantiate(TextMeshInstant).GetComponent<TextMesh>();
            TextMesh ZMax = Instantiate(TextMeshInstant).GetComponent<TextMesh>();

            Y0.transform.parent = YMax.transform.parent = ZXAxis;
            Z0.transform.parent = ZMax.transform.parent = YXAxis;

            Y0.text = Z0.text = YMax.text = ZMax.text = Ignore0(ix);

            Y0.transform.position= Pos0;
            Z0.transform.position= Pos0;
            YMax.transform.position = Pos1;
            ZMax.transform.position = Pos2;

            Z_Y0Texts.Add(Y0);
            Y_Z0Texts.Add(Z0);
            Z_YMTexts.Add(YMax);
            Y_ZMTexts.Add(ZMax);
        }
        for (int iy = 0; iy <= Y; iy += Resolution)
        {
            Vector3 Pos0 = new Vector3(0, iy, 0);
            Vector3 Pos1 = new Vector3(X, iy, 0);
            Vector3 Pos2 = new Vector3(0, iy, Z);

            SetLine(Pos0, Pos1, Pos2, XAxisL, ZAxisL);

            TextMesh Current = Instantiate(TextMeshInstant).GetComponent<TextMesh>();
            Current.transform.position = Pos0;
            Current.text = Ignore0(iy);
            Current.transform.parent = XAxisReverse;
            Y_XTexts.Add(Current);
        }
        for (int iz = 0; iz <= Z; iz += Resolution)
        {
            Vector3 Pos0 = new Vector3(0, 0, iz);
            Vector3 Pos1 = new Vector3(X, 0, iz);
            Vector3 Pos2 = new Vector3(0, Y, iz);

            SetLine(Pos0, Pos1, Pos2,XAxisL,YAxisL);

            TextMesh Current = Instantiate(TextMeshInstant).GetComponent<TextMesh>();
            Current.transform.position = Pos0;
            Current.text = Ignore0(iz);
            Current.transform.parent = XAxisReverse;
            Z_XTexts.Add(Current);
        }

        XName = Instantiate(TextMeshInstant).GetComponent<TextMesh>();
        XName.text = XAxisName;
        //XName.transform.position = Vector3.forward * Z / 2;
        //XName.transform.parent = YAxis;

        YName = Instantiate(TextMeshInstant).GetComponent<TextMesh>();
        YName.text = YAxisName;
        //YName.transform.position = Vector3.up * Y / 2;
        //YName.transform.parent = ZAxis;

        ZName = Instantiate(TextMeshInstant).GetComponent<TextMesh>();
        ZName.text = ZAxisName;
        //ZName.transform.parent = XAxisReverse;
        //ZName.transform.position = Vector3.forward * Z / 2;
        //ZName.transform.parent = YAxis;
    }
    void SetLine(Vector3 Pos0, Vector3 Pos1, Vector3 Pos2, Transform Parent2, Transform Parent1)
    {
        LineRenderer CurrentLine1 = Instantiate(LineInstant);
        LineRenderer CurrentLine2 = Instantiate(LineInstant);

        CurrentLine1.SetPosition(0, Pos0);
        CurrentLine1.SetPosition(1, Pos1);

        CurrentLine2.SetPosition(0, Pos0);
        CurrentLine2.SetPosition(1, Pos2);

        CurrentLine1.transform.parent = Parent1;
        CurrentLine2.transform.parent = Parent2;
    }
    int ReSetByResolution(int N,int Resolution)
    {
        return Resolution * ((int)Mathf.Ceil((float)N / Resolution));
    }
    string Ignore0(int I)
    {
        if (Dont0 && I == 0)
        {
            Debug.Log("S");
            return "";
        }
        else return I.ToString();
    }
}
