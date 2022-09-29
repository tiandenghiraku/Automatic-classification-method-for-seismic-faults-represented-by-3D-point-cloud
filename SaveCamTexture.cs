using UnityEngine;
using System;
using System.IO;
public class SaveCamTexture : MonoBehaviour
{
    public Camera cam;
    public RenderTexture rt;
    public Transform Axis1, Axis2;

    public PositionDefine PD;
    public PosDefineInfo[] SaveQueue;
    public Transform WorldCoord;
    bool StartSave;
    int SaveIter = 0;
    int PosSetFrames = -1;
    int Wait = 3;
    public void Start()
    {
        if (cam == null)
        {
            cam = this.GetComponent<Camera>();
        }
    }
    private void Update()
    {
        if (cam == null)
        { return; }
        if (Input.GetKeyDown(KeyCode.F4))
        {
            _SaveCamTexture();
        }
        if (Input.GetKeyDown(KeyCode.F6))
        {
            SaveIter = 0;
            StartSave = true;
        }
        if(StartSave)
        {
            if (SaveIter < SaveQueue.Length && PosSetFrames == -1)
            {
                PD.Set(SaveQueue[SaveIter].AxisAngle, SaveQueue[SaveIter].Pos);
                PosSetFrames = 0;
            }
        }
    }
    private void LateUpdate()
    {
        if(StartSave)
        {
            WorldCoord.localScale = new Vector3(1, -1, 1);

            PosSetFrames++;
            if(PosSetFrames>=Wait)
            {
                _SaveCamTexture();
                SaveIter++;
                if (SaveIter >= SaveQueue.Length)
                {
                    StartSave = false;
                }
                PosSetFrames = -1;
            }
            if(!StartSave)
            {
                WorldCoord.localScale = Vector3.one;
            }
        }
    }
    private void _SaveCamTexture()
    {
        rt = cam.targetTexture;
        if (rt != null)
        {
            _SaveRenderTexture(rt);
            rt = null;
        }
        else
        {
            GameObject camGo = new GameObject("camGO");
            Camera tmpCam = camGo.AddComponent<Camera>();
            tmpCam.CopyFrom(cam);
            rt = new RenderTexture(Screen.width, Screen.height, 16, RenderTextureFormat.ARGB32);
            tmpCam.targetTexture = rt;
            tmpCam.Render();
            _SaveRenderTexture(rt);
            Destroy(camGo);
            rt.Release();
            Destroy(rt);
        }

    }
    private void _SaveRenderTexture(RenderTexture rt)
    {
        RenderTexture active = RenderTexture.active;
        RenderTexture.active = rt;
        Texture2D png = new Texture2D(rt.width, rt.height, TextureFormat.ARGB32, false);
        png.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        png.Apply();
        RenderTexture.active = active;
        byte[] bytes = png.EncodeToPNG();
        Debug.Log(Axis1.transform.localRotation.eulerAngles);
        //string FileName = Axis1.transform.localEulerAngles.y + " " + Axis2.transform.localEulerAngles.x + " "+transform.localPosition;
        string FileName = PD.Title+" "+ PD.AxisRotation.x + " " + PD.AxisRotation.y + " " + PD.CamPos;
        string path = string.Format("Assets/../ImageCatch/{0}.png", FileName);
        FileStream fs = File.Open(path, FileMode.Create);
        BinaryWriter writer = new BinaryWriter(fs);
        writer.Write(bytes);
        writer.Flush();
        writer.Close();
        fs.Close();
        Destroy(png);
        png = null;
        Debug.Log("保存成功！" + path);
    }
    [Serializable]
    public struct PosDefineInfo
    {
        public Vector2 AxisAngle;
        public Vector3 Pos;
    }
}