using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityVolumeRendering;
using System;
using System.IO;
public class PointNormalTest : MonoBehaviour
{
    public VolumeRenderedObject VO;
    public VolumeDataset Dataset;

    public Transform P2, P3;
    public Transform[] OtherPoints;
    public Transform NormalPointer;

    public Vector3 Normal;
    public Vector3 NormalByPoints;
    public float NormalLength;
    public Vector3 Sum;


    public bool NormalStarted = false;
    public bool LabelStarted = false;
    [SerializeField]
    public VectorStatistics.IntV3 IPI;

    public int OneCount;
    public int Now = 0;
    GameObject NormalTestPointer ;
    public GameObject[] LabelTestPointer;
    public float Percent;
    public int Step;
    VectorStatistics.IntV3[] PosInRadiusForNormal, PosInRadiusForLabel;
    public int RadiusForNormal, RadiusForLabel;
    public int MaxPointCount;
    public NormalFunction NormalFunction;
    public int TestSequence;
    public float NotReliablefilter,DotFilter,NormalCrossFilter;
    public bool SizeOverNotReliable;
    GameObject Pointers,LabelPointers;
    public int QueueLength;
    public Queue<VectorStatistics.IntV3> NewlyLabeledPoint;
    public VectorStatistics.IntV3 ConvolutionCenter;
    public float KA, KC;
    public float[] ConvolutionKernel;
    public GameObject[] ConvolutionPointer;
    Transform ConvolutionPointerParent;
    public Transform WorldCoord;
    public bool CrossUnreliableArea;
    static public DateTime StartTime, FinishTime;
    static public TimeSpan Finish()
    {
        FinishTime = DateTime.Now;
        return (FinishTime - StartTime);
    }
    // Start is called before the first frame update
    public void ResetToParent(Transform T, Transform P)
    {
        T.parent = P;
        T.localPosition = Vector3.zero;
        T.localRotation = Quaternion.Euler(Vector3.zero);
        T.localScale = new Vector3(1, 1, 1);
    }
    void Start()
    {
        Dataset = VO.dataset;
        IPI = new VectorStatistics.IntV3(0, 0, 0);
        OneCount = Dataset.OneCount;
        Dataset.PointInfos = new VectorStatistics.PointInfo[Dataset.data.Length];
        Dataset.CountPerLabel = Dataset.LastPerLabel = new int[0];
        NormalTestPointer = Resources.Load("Pointer") as GameObject;
        ConvolutionPointer = new GameObject[3];
        ConvolutionPointer[0] = Resources.Load("CubePointer 0") as GameObject;
        ConvolutionPointer[1] = Resources.Load("CubePointer 1") as GameObject;
        ConvolutionPointer[2] = Resources.Load("CubePointer 2") as GameObject;
        LabelTestPointer = new GameObject[10];
        for (int i = 0; i < 10; i++)
        {
            LabelTestPointer[i] = Resources.Load("LabelPointer/LabelPointer " + (i + 1)) as GameObject;
        }

        PosInRadiusForNormal = VectorStatistics.RelativPointsInRadius(RadiusForNormal, true);
        PosInRadiusForLabel = VectorStatistics.RelativPointsInRadius(RadiusForLabel, true);
        Pointers = new GameObject("Pointers");
        ResetToParent(Pointers.transform, WorldCoord);
        LabelPointers = new GameObject("LabelPointers");
        ResetToParent(LabelPointers.transform, WorldCoord);
        LabelPointers.transform.parent = WorldCoord;
        ConvolutionPointerParent = new GameObject("ConvolutionPointerParent").transform;
        ConvolutionPointerParent.rotation = Quaternion.LookRotation(Vector3.down, Vector3.forward);
        ConvolutionPointerParent.position = new Vector3(0, Dataset.dimZ, 0);
        NewlyLabeledPoint = new Queue<VectorStatistics.IntV3>();

        //ConvolutionKernel = VectorStatistics.MakeKernel(KA, KC);
    }
    // Update is called once per frame
    void Update()
    {
        /*Vector3 centerPointPos = transform.position;

        Normal = VectorStatistics.ThreePointsToNormal(centerPointPos, P2.position, P3.position);
        NormalLength = Normal.magnitude;
        Debug.DrawLine(centerPointPos, centerPointPos + Normal, Color.yellow);
        NormalPointer.position = centerPointPos + Normal;
        NormalPointer.rotation = Quaternion.LookRotation(Normal);

        Vector3[] points = new Vector3[OtherPoints.Length];

        for (int i = 0; i < OtherPoints.Length; i++)
        {
            points[i] = OtherPoints[i].position;
        }

        Vector3[] Normals = VectorStatistics.PointPairsToNormals(centerPointPos, VectorStatistics.PointPair.GetAllPointPair(points));

        NormalByPoints = VectorStatistics.NormalByPoints(centerPointPos, points, true);
        Debug.DrawLine(centerPointPos, centerPointPos + NormalByPoints, Color.blue);

        points = null;
        Sum = Vector3.zero;
        int MaxID = VectorStatistics.FindMedianID(Normals);
        Normals = VectorStatistics.ReverseByMedian(Normals);
        for(int i=0;i<Normals.Length;i++)
        {
            Debug.DrawLine(centerPointPos, centerPointPos + Normals[i], i == MaxID ? Color.green : Color.white);
            Sum += Normals[i];
        }
        //Debug.DrawLine(centerPointPos, centerPointPos + Sum.normalized, Color.red);*/

        if(Input.GetKeyDown(KeyCode.N)&&!NormalStarted)
        {
            StartTime = DateTime.Now;
            NormalStarted = true;
        }
        if (Input.GetKeyDown(KeyCode.L) && !LabelStarted)
        {
            StartTime = DateTime.Now;
            LabelStarted = true;
        }
        if (Input.GetKeyDown(KeyCode.X))
        {
            NormalStarted = false;
            LabelStarted = false;
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            NormalStarted = false;
            IPI = VectorStatistics.DatasetNormals(Dataset, PosInRadiusForNormal, true, NormalFunction, IPI, Step, ref Now, NormalTestPointer, Pointers.transform, MaxPointCount, TestSequence, SizeOverNotReliable,NotReliablefilter);
        }
        if (Input.GetKeyDown(KeyCode.M))
        {
            VectorStatistics.MakeDataSetByFunction(128, 128, 128);
        }
        if(Input.GetKeyDown(KeyCode.C))
        {
            int[][] Layer = VectorStatistics.CubeConvolution(Dataset, ConvolutionCenter, ConvolutionKernel, ConvolutionPointer, ConvolutionPointerParent);
            for (int y = 0; y < Dataset.dimY; y++)
            {
                for (int z = 0; z < Dataset.dimZ; z++)
                {
                    for (int x = 0; x < Dataset.dimX; x++)
                    {
                        if(x<Layer[y][z])
                        {
                            Dataset.data[Dataset.ConvertID(new VectorStatistics.IntV3(x, y, z))] = 0;
                        }
                    }
                }
            }
            Dataset.OutputData("LayerResult", "LayerResult " + ConvolutionCenter.X + " " + ConvolutionCenter.Y + " " + ConvolutionCenter.Z);
        }
        if (NormalStarted)
        {
            if (IPI.X != -1)
            {
                IPI = VectorStatistics.DatasetNormals(Dataset, PosInRadiusForNormal, true, NormalFunction, IPI, Step, ref Now, NormalTestPointer, Pointers.transform, MaxPointCount, TestSequence, SizeOverNotReliable,NotReliablefilter);
            }
            else
            {
                NormalStarted = false;
            }
        }
        if(LabelStarted)
        {
            Pointers.SetActive(false);
            for (int i = 0; i < Step; i++)
            {
                LabelStarted = VectorStatistics.SetLabel(Dataset, ref NewlyLabeledPoint, PosInRadiusForLabel, NotReliablefilter, DotFilter,NormalCrossFilter ,ref Now, TestSequence, LabelTestPointer, LabelPointers.transform,MaxPointCount, PosInRadiusForNormal, NormalFunction, CrossUnreliableArea);
                if (!LabelStarted)
                {
                    i = Step;
                    Dataset.OutputLabel("LabelResult", "Label " + Dataset.LabelCount);
                }
            }
        }
        if (Input.GetKeyDown(KeyCode.K))
        {
            Pointers.SetActive(false);
            Debug.Log("K");
            VectorStatistics.SetLabel(Dataset, ref NewlyLabeledPoint, PosInRadiusForLabel, NotReliablefilter, DotFilter,NormalCrossFilter, ref Now, TestSequence, LabelTestPointer, LabelPointers.transform, MaxPointCount, PosInRadiusForNormal, NormalFunction, CrossUnreliableArea);
        }
        Percent = (float)Now * 100 / OneCount;
        QueueLength = NewlyLabeledPoint.Count;
    }
}
[SerializeField]

public struct VectorStatistics
{
    public struct PointInfo
    {
        public Vector3 Normal;
        public float NotReliable;
        public bool NormalChangeable;
        public int Label;
    }
    public struct PointPair
    {
        public Vector3 P1, P2;
        public PointPair(Vector3 p1, Vector3 p2)
        {
            P1 = p1;
            P2 = p2;
        }
        public static PointPair[] GetAllPointPair(Vector3[] Points)
        {
            int count = Points.Length * (Points.Length - 1) / 2;
            PointPair[] ToReturn = new PointPair[count];
            int xc = 0;
            for (int i = 0; i < Points.Length; i++)
            {
                for (int j = i + 1; j < Points.Length; j++)
                {
                    ToReturn[xc++] = new PointPair(Points[i], Points[j]);
                }
            }
            return ToReturn;
        }
    }
    public static Vector3 ThreePointsToNormal(Vector3 P1, Vector3 P2, Vector3 P3)
    {
        Vector3 D12 = P2 - P1;
        Vector3 D13 = P3 - P1;
        return Vector3.Cross(D12, D13);
    }
    public static Vector3[] PointPairsToNormals(Vector3 P, PointPair[] PointPairs)
    {
        Vector3[] ToReturn = new Vector3[PointPairs.Length];
        for (int i = 0; i < PointPairs.Length; i++)
        {
            ToReturn[i] = ThreePointsToNormal(P, PointPairs[i].P1, PointPairs[i].P2);
        }
        return ToReturn;
    }
    public static int FindMedianID(Vector3[] Vectors)
    {
        float[] Sum = new float[Vectors.Length];
        for (int i = 0; i < Vectors.Length; i++)
        {
            for (int j = i + 1; j < Vectors.Length; j++)
            {
                float CurrentDot = Mathf.Abs(Vector3.Dot(Vectors[i], Vectors[j]));
                Sum[i] += CurrentDot;
                Sum[j] += CurrentDot;
            }
        }
        int MaxID = -1;
        float MaxDot = float.MinValue;
        for (int i = 0; i < Sum.Length; i++)
        {
            if (Sum[i] > MaxDot)
            {
                MaxDot = Sum[i];
                MaxID = i;
            }
        }
        return MaxID;
    }
    public static Vector3 Median(Vector3[] Vectors)
    {
        return Vectors[FindMedianID(Vectors)];
    }
    public static Vector3 ReverseByV3(Vector3 ToReverse, Vector3 V3)
    {
        if (Vector3.Dot(ToReverse, V3) > 0)
        {
            return ToReverse;
        }
        else
        {
            return -ToReverse;
        }
    }
    public static Vector3[] ReverseByMedian(Vector3[] ToRevese)
    {
        int MedianID = FindMedianID(ToRevese);
        Vector3[] toReturn = new Vector3[ToRevese.Length];
        for (int i = 0; i < ToRevese.Length; i++)
        {
            ToRevese[i] = ReverseByV3(ToRevese[i], ToRevese[MedianID]);
        }
        return ToRevese;
    }
    public static Vector3 Average(Vector3[] Directs)
    {
        Directs = ReverseByMedian(Directs);
        Vector3 Sum = Vector3.zero;
        for (int i = 0; i < Directs.Length; i++)
        {
            Sum += Directs[i];
        }
        return Sum.normalized;
    }
    public static Vector3 PanelFit(Vector3[] Points, Vector3 CenterPoint)
    {
        Vector3[] RelativePoints;
        RelativePoints = new Vector3[Points.Length];

        for (int i = 0; i < Points.Length; i++)
        {
            RelativePoints[i] = Points[i] - CenterPoint;
        }
        Vector4 Result = Panel.PanelFit(RelativePoints);
        return new Vector3(Result.x, Result.y, Result.z);
    }
    public static Vector3 RefreshPerPoint(Vector3[] Points, Vector3 CenterPoint)
    {
        Vector3 Normal = new Vector3();
        {
            List<Vector3> FirstRelativeV3 = new List<Vector3>();
            for (int i = 0; i < Points.Length; i++)
            {
                Vector3 Relative = Points[i] - CenterPoint;
                if (Relative.sqrMagnitude > 0)
                {
                    FirstRelativeV3.Add(Relative);
                    if (FirstRelativeV3.Count >= 2)
                    {
                        Normal = Vector3.Cross(FirstRelativeV3[0], FirstRelativeV3[FirstRelativeV3.Count - 1]).normalized;
                        if (Normal.sqrMagnitude > 0)
                        {
                            break;
                        }
                    }
                }
            }
        }
        if (Normal.sqrMagnitude <= 0)
        {
            Debug.Log(Points.Length);
            Normal = Vector3.up;
        }
        for (int i = 0; i < Points.Length; i++)
        {
            Vector3 Relative = Points[i] - CenterPoint;
            Normal = (Normal - Vector3.Dot(Normal, Relative) * Relative / (i + 1)).normalized;
        }
        if (Normal.sqrMagnitude <= 0) Debug.LogError("?");
        return Normal;
    }
    public static Vector4 NormalByPoints(IntV3 CenterPoint, Vector3[] OtherPoints, NormalFunction function, int MaxLength)
    {
        Vector3 CenterPointV3 = CenterPoint.ToVector3();
        if (OtherPoints.Length < 3)
        {
            return new Vector4(0, 0, 0, Mathf.Infinity);
            //Infinity = true;
        }
        if (MaxLength > 0 && OtherPoints.Length > MaxLength)
        {
            Vector3[] NewPoints = new Vector3[MaxLength];
            int Sequence = OtherPoints.Length / MaxLength;
            int id = 0;
            for (int i = 0; i < MaxLength; i += Sequence)
            {
                NewPoints[id++] = OtherPoints[i];
            }
            if (id < 3)
            {
                return new Vector4(0, 0, 0, Mathf.Infinity);
                //Infinity = true;
            }
            OtherPoints = new Vector3[id];
            Array.Copy(NewPoints, OtherPoints, id);
            //OtherPoints = NewPoints;
        }

        Vector3[] Normals;
        Vector3 Normal;
        switch (function)
        {
            case NormalFunction.Average:
                {
                    Normals = PointPairsToNormals(CenterPointV3, PointPair.GetAllPointPair(OtherPoints));
                    Normal = Average(ReverseByMedian(Normals));
                    break;
                }
            case NormalFunction.Median:
                {
                    Normals = PointPairsToNormals(CenterPointV3, PointPair.GetAllPointPair(OtherPoints));
                    Normal = Median(Normals).normalized;
                    break;
                }
            case NormalFunction.PanelFit:
                {
                    Normal = PanelFit(OtherPoints, CenterPointV3).normalized;
                    break;
                }
            case NormalFunction.RefreshPerPoint:
                {
                    Normal = RefreshPerPoint(OtherPoints, CenterPointV3);
                    break;
                }
            default:
                {
                    Normals = PointPairsToNormals(CenterPointV3, PointPair.GetAllPointPair(OtherPoints));
                    Normal = Average(ReverseByMedian(Normals));
                    break;
                }
        }
        float NotReliable = 0;
        if (Normal.sqrMagnitude <= 0)
            NotReliable = Mathf.Infinity;
        else
        {
            for (int i = 0; i < OtherPoints.Length; i++)
            {
                NotReliable += Mathf.Abs(Vector3.Dot(Normal, OtherPoints[i] - CenterPointV3));
            }
            NotReliable = NotReliable / OtherPoints.Length;
        }
        //if (Infinity) NotReliable = Mathf.Infinity;
        return new Vector4(Normal.x, Normal.y, Normal.z, NotReliable);
    }

    /*public static Vector3 AverageByDoublePointAndPointSet(Vector3 P1,Vector3 P2, Vector3[] OtherPoints)
    {
        Vector3[] Normals = new Vector3[OtherPoints.Length];
        for(int i=0;i<OtherPoints.Length;i++)
        {
            Normals[i] = ThreePointsToNormal(P1, P2, OtherPoints[i]);
        }
        return Average(Normals);
    }*/
    [System.Serializable]
    public struct IntV3
    {
        public int X, Y, Z;
        public IntV3(int x, int y, int z)
        {
            X = x; Y = y; Z = z;
        }
        public static IntV3 Add(IntV3 P1, IntV3 P2)
        {
            return new IntV3(P1.X + P2.X, P1.Y + P2.Y, P1.Z + P2.Z);
        }
        public static IntV3 Sub(IntV3 P1, IntV3 P2)
        {
            return new IntV3(P1.X - P2.X, P1.Y - P2.Y, P1.Z - P2.Z);
        }
        public static bool Eaquals(IntV3 P1, IntV3 P2)
        {
            return P1.X == P2.X && P1.Y == P2.Y && P1.Z == P2.Z;
        }
        public int Pow2()
        {
            return X * X + Y * Y + Z * Z;
        }
        public Vector3 ToVector3()
        {
            return new Vector3(X, Y, Z);
        }
        public static Vector3[] ToVector3(IntV3[] IDs)
        {
            Vector3[] ToReturn = new Vector3[IDs.Length];
            for (int i = 0; i < IDs.Length; i++)
            {
                ToReturn[i] = IDs[i].ToVector3();
            }
            return ToReturn;
        }
    }
    public static IntV3[] RelativPointsInRadius(int Radius, bool Sphere)
    {
        List<IntV3> L = new List<IntV3>();
        int Pow2 = Radius * Radius;

        for (int X = -Radius; X <= Radius; X++)
        {
            for (int Y = -Radius; Y <= Radius; Y++)
            {
                for (int Z = -Radius; Z <= Radius; Z++)
                {
                    IntV3 Current = new IntV3(X, Y, Z);
                    if (/*!IntPosID.Eaquals(Current, new IntPosID(0, 0, 0)) &&*/ (!Sphere || Current.Pow2() <= Pow2))
                    {
                        L.Add(Current);
                    }
                }
            }
        }
        IntV3[] ToReturn = new IntV3[L.Count];
        for (int i = 0; i < ToReturn.Length; i++)
        {
            ToReturn[i] = L[i];
        }
        return ToReturn;
    }
    public static Vector3[] RelativPointsInRadius(int Radius, bool Sphere, IntV3 Center, VolumeDataset Dataset)
    {
        List<Vector3> L = new List<Vector3>();
        int Pow2 = Radius * Radius;
        IntV3 Current = new IntV3(0, 0, 0);
        for (Current.X = -Radius; Current.X <= Radius; Current.X++)
        {
            for (Current.Y = -Radius; Current.Y <= Radius; Current.Y++)
            {
                for (Current.Z = -Radius; Current.Z <= Radius; Current.Z++)
                {
                    IntV3 RealPos = IntV3.Add(Center, Current);
                    int RealID = Dataset.ConvertID(RealPos.X, RealPos.Y, RealPos.Z);
                    if ((!Sphere || Current.Pow2() <= Pow2) && RealID != -1 && Dataset.data[RealID] == 1)
                    {
                        L.Add(RealPos.ToVector3());
                    }
                }
            }
        }
        Vector3[] ToReturn = new Vector3[L.Count];
        for (int i = 0; i < ToReturn.Length; i++)
        {
            ToReturn[i] = L[i];
        }
        return ToReturn;
    }
    public static Vector3[] RelativPointsInRadius(IntV3 Center, IntV3[] RelativePosSet, VolumeDataset Dataset, int LabelFilter)
    {
        IntV3[] ToReturnIndexs = new IntV3[RelativePosSet.Length];
        int index = 0;
        for (int i = 0; i < RelativePosSet.Length; i++)
        {
            IntV3 RealPos = IntV3.Add(Center, RelativePosSet[i]);
            int RealID = Dataset.ConvertID(RealPos);
            if ((RealID != -1 && Dataset.data[RealID] == 1) && (LabelFilter == 0 || (LabelFilter == -1 && Dataset.PointInfos[RealID].Label != 0) || Dataset.PointInfos[RealID].Label == LabelFilter))
            {
                ToReturnIndexs[index++] = RealPos;
            }
        }
        Vector3[] ToReturn = new Vector3[index];
        for (int i = 0; i < ToReturn.Length; i++)
        {
            ToReturn[i] = ToReturnIndexs[i].ToVector3();
        }
        return ToReturn;
    }
    public static IntV3[] RelativPointIDsInRadius(IntV3 Center, IntV3[] RelativePosSet, VolumeDataset Dataset, int LabelFilter)
    {
        IntV3[] ToReturnIndexs = new IntV3[RelativePosSet.Length];
        int index = 0;
        for (int i = 0; i < RelativePosSet.Length; i++)
        {
            IntV3 RealPos = IntV3.Add(Center, RelativePosSet[i]);
            int RealID = Dataset.ConvertID(RealPos.X, RealPos.Y, RealPos.Z);
            if (RealID != -1 && Dataset.data[RealID] == 1 && (LabelFilter == 0 || Dataset.PointInfos[RealID].Label == LabelFilter || (LabelFilter == -1 && Dataset.PointInfos[RealID].Label == 0)))
            {
                ToReturnIndexs[index++] = RealPos;
            }
        }
        IntV3[] ToReturn = new IntV3[index];
        Array.Copy(ToReturnIndexs, ToReturn, index);
        return ToReturn;
    }
    /*public static void DatasetNormals(VolumeDataset Dataset,int Radius,bool Sphere,NormalFunction Function, GameObject Pointers,int TestSequence)
    {
        int TestIter = 0;
        GameObject TestPointer = Resources.Load("Pointer") as GameObject;
        IntPosID Current = new IntPosID(0, 0, 0);
        for (Current.X= 0; Current.X< Dataset.dimX; Current.X++)
        {
            for(Current.Y= 0; Current.Y < Dataset.dimY; Current.Y++)
            {
                for(Current.Z= 0; Current.Z < Dataset.dimZ; Current.Z++)
                {
                    int id = Dataset.ConvertID(Current.X, Current.Y, Current.Z);
                    if (Dataset.GetData(id) == 1)
                    {
                        //int L = RelativPointsInRadius(Radius, Sphere, Current, Dataset).Length;
                        Vector3 CurrentV3 = Current.ToVector3();
                        //ToReturn[id] = new Vector3(L, L, L);
                        Vector4 Result= NormalByPoints(CurrentV3, RelativPointsInRadius(Radius, Sphere, Current, Dataset), Function, 20);
                        Dataset.PointInfos[id].Normal = new Vector3(Result.x, Result.y, Result.z);
                        Dataset.PointInfos[id].NotReliable = Result.w;
                        if (TestIter++%TestSequence==0)
                        {
                            GameObject.Instantiate(TestPointer, CurrentV3, Quaternion.LookRotation(Dataset.PointInfos[id].Normal), Pointers.transform);
                        }
                    }
                }
            }
        }
    }*/
    public static IntV3 DatasetNormals(VolumeDataset Dataset, IntV3[] PosInRadius, bool Sphere, NormalFunction Function, IntV3 Start, int Times, ref int Now, GameObject TestPointer, Transform Pointers, int MaxLength, int TestSequence, bool SizeOverNotReliable, float NotReliableFilter)
    {
        IntV3 Iter3D = Start;

        int Iter = 0;
        int startID = Dataset.ConvertID(Start.X, Start.Y, Start.Z);

        for (int iz = 0; iz < Dataset.dimZ; iz++)
        {
            for (int iy = 0; iy < Dataset.dimY; iy++)
            {
                for (int ix = 0; ix < Dataset.dimX; ix++)
                {
                    Iter3D = new IntV3(ix, iy, iz);
                    int Iterid = Dataset.ConvertID(Iter3D);
                    if (Iterid > startID)
                    {
                        if (Dataset.data[Iterid] == 1)
                        {
                            Vector3 CurrentV3 = Iter3D.ToVector3();
                            Vector4 Result = NormalByPoints(Iter3D, RelativPointsInRadius(Iter3D, PosInRadius, Dataset, 0), Function, MaxLength);
                            Dataset.PointInfos[Iterid].Normal = new Vector3(Result.x, Result.y, Result.z);
                            Dataset.PointInfos[Iterid].NotReliable = Result.w;
                            Dataset.PointInfos[Iterid].NormalChangeable = Result.w > NotReliableFilter;
                            if (Now++ % TestSequence == 0)
                            {
                                GameObject Pointer = GameObject.Instantiate(TestPointer, CurrentV3, Quaternion.LookRotation(Dataset.PointInfos[Iterid].Normal), Pointers);
                                
                                float NotReliable = Mathf.Min(Dataset.PointInfos[Iterid].NotReliable, 5);
                                if (SizeOverNotReliable)
                                {
                                    //Pointer.transform.localScale = new Vector3(NotReliable, NotReliable, NotReliable);
                                    float Min = 1;
                                    float Max = 4;
                                    
                                    float Rate = (NotReliable - Min) / (Max - Min);
                                    Vector3 TheColor = Top(Vector3.Lerp(Vector3.up, Vector3.right, Mathf.Clamp(Rate, 0, 1)));
                                    //Vector3 TheColor = Vector3.Lerp(Vector3.one * 0, Vector3.one * 0.7f, Mathf.Clamp(Rate, 0, 1));
                                    LineRenderer LR = Pointer.GetComponent<LineRenderer>();
                                    if (LR != null)
                                    {
                                        LR.startColor = Pointer.GetComponent<LineRenderer>().endColor = new Color(TheColor.x, TheColor.y, TheColor.z);
                                    }
                                    else
                                    {
                                    }
                                }
                                else
                                    Pointer.transform.localScale = new Vector3(3, 3, 3);
                            }
                            Iter++;
                            if (Iter >= Times)
                            {
                                return Iter3D;
                            }
                        }
                    }
                }
            }
        }
        PointNormalTest.FinishTime = DateTime.Now;
        Debug.Log("Normal Finished " + PointNormalTest.Finish());
        
        Dataset.NormalSet = true;
        return new IntV3(-1, -1, -1);
    }
    public static Vector3 Top(Vector3 V3)
    {
        float Max = Mathf.Max(V3.x, V3.y, V3.z);
        if (Max == 0) return Vector3.one;
        float Times = 1 / Max;
        return V3 * Times;
    }
    public static bool SetLabel(VolumeDataset Dataset, ref Queue<IntV3> Start, IntV3[] PosInRadiusLabel, float NotReliableFilter, float DotFilter, float NormalCrossFilter, ref int Now, int TestSequence, GameObject[] PointerForLabel, Transform LabelPointers, int MaxLength, IntV3[] PosInRadiusNormal, NormalFunction NormalFunction,bool CrossUnreliableArea)
    {
        if (Dataset.CountPerLabel.Length == 0)
        {
            if (!Dataset.NormalSet)
            {
                return false;
            }
            if (Start.Count != 0)
            {
                LabelPointByRadius(Dataset, PosInRadiusLabel, ref Start, DotFilter, NormalCrossFilter, NotReliableFilter, ref Now, TestSequence, PointerForLabel, LabelPointers, MaxLength, PosInRadiusNormal, NormalFunction,CrossUnreliableArea);
                return true;
            }
            else
            {
                for (int iz = 0; iz < Dataset.dimZ; iz++)
                {
                    for (int iy = 0; iy < Dataset.dimY; iy++)
                    {
                        for (int ix = 0; ix < Dataset.dimX; ix++)
                        {
                            IntV3 Iter3D = new IntV3(ix, iy, iz);
                            int Currentid = Dataset.ConvertID(Iter3D);
                            if (Dataset.data[Currentid] == 1 && Dataset.PointInfos[Currentid].Label == 0 && Dataset.PointInfos[Currentid].NotReliable <= NotReliableFilter)
                            {
                                Dataset.PointInfos[Currentid].Label = ++Dataset.LabelCount;
                                Start.Enqueue(Iter3D);
                                LabelPointByRadius(Dataset, PosInRadiusLabel, ref Start, DotFilter, NormalCrossFilter, NotReliableFilter, ref Now, 10, PointerForLabel, LabelPointers, MaxLength, PosInRadiusNormal, NormalFunction, CrossUnreliableArea);
                                return true;
                            }
                        }
                    }
                }
            }
            {
                SetCountOfLabel(Dataset);
            }
            for (int i = 0; i < Dataset.CountPerLabel.Length; i++)
            {
                Debug.Log("Type " + i + ":" + Dataset.CountPerLabel[i]);
            }
            return true;
        }
        else
        {
            if (Dataset.CountPerLabel[0] != Dataset.LastPerLabel[0])
            {
                for (int iz = 0; iz < Dataset.dimZ; iz++)
                {
                    for (int iy = 0; iy < Dataset.dimY; iy++)
                    {
                        for (int ix = 0; ix < Dataset.dimX; ix++)
                        {
                            IntV3 Current = new IntV3(ix, iy, iz);
                            int CurrentIter = Dataset.ConvertID(Current);
                            if (Dataset.PointInfos[CurrentIter].Label == 0)
                            {
                                //Vector4[] Normals = new Vector4[Dataset.LabelCount];
                                int MinLabelID = 0;
                                float MinNotReliable = NotReliableFilter;
                                for (int i = 1; i <= Dataset.LabelCount; i++)
                                {
                                    float Currentw;
                                    if (Dataset.CountPerLabel[i] < 3)
                                    {
                                        //Normals[i - 1] = new Vector4(0, 0, 0, Mathf.Infinity);
                                        Currentw = Mathf.Infinity;
                                    }
                                    else
                                    {
                                        //Normals[i - 1] = NormalByPoints(Current.ToVector3(), RelativPointsInRadius(Current, PosInRadius, Dataset, i), NormalFunction.PanelFit, MaxLength);
                                        Currentw = NormalByPoints(Current, RelativPointsInRadius(Current, PosInRadiusNormal, Dataset, i), NormalFunction, MaxLength).w;
                                    }
                                    if (Currentw < MinNotReliable)
                                    {
                                        MinNotReliable = Currentw;
                                        MinLabelID = i;
                                    }
                                }
                                /*for (int i = 1; i <= Dataset.LabelCount; i++)
                                {
                                    if (Normals[i - 1].w < MinNotReliable)
                                    {
                                        MinLabelID = i;
                                        MinNotReliable = Normals[i].w;
                                    }
                                }*/
                                if (MinLabelID != 0)
                                {
                                    Dataset.PointInfos[CurrentIter].Label = MinLabelID;
                                }
                                else
                                {
                                    Dataset.PointInfos[CurrentIter].Label = int.MinValue;
                                }

                                int Label = Dataset.PointInfos[CurrentIter].Label;
                                if (Label != 0 && Label != int.MinValue)
                                {
                                    if (Label < 0)
                                        Label = -Label;
                                    if (Now++ % TestSequence == 0)
                                    {
                                        //Debug.Log(Label+" "+ (-Label % 10));
                                        GameObject Pointer = GameObject.Instantiate(PointerForLabel[Label % 10], Current.ToVector3(), new Quaternion(), LabelPointers);
                                        float scale = 1 + (Label + 10) / 10 * 0.1f;
                                        Pointer.transform.localScale = new Vector3(scale, scale, scale);
                                    }
                                }

                                return false;
                            }
                        }
                    }
                }
                for (int iz = 0; iz < Dataset.dimZ; iz++)
                {
                    for (int iy = 0; iy < Dataset.dimY; iy++)
                    {
                        for (int ix = 0; ix < Dataset.dimX; ix++)
                        {
                            int CurrentID = Dataset.ConvertID(new IntV3(ix, iy, iz));
                            int Label = Dataset.PointInfos[CurrentID].Label;
                            if (Label == int.MinValue)
                            {
                                Dataset.PointInfos[CurrentID].Label = 0;
                            }
                            if (Label < 0)
                            {
                                Dataset.PointInfos[CurrentID].Label = -Dataset.PointInfos[CurrentID].Label;
                            }
                        }
                    }
                }
                SetCountOfLabel(Dataset);
                Debug.Log(Dataset.CountPerLabel[0] + " " + Dataset.LastPerLabel[0]);
                return true;
            }
            else
            {
                Debug.Log("Label End");
                return false;
            }

        }
        return false;
    }
    public static void SetCountOfLabel(VolumeDataset Dataset)
    {
        if (Dataset.CountPerLabel.Length == 0)
        {
            Dataset.LastPerLabel = new int[1];
        }
        else
        {
            Dataset.LastPerLabel = new int[Dataset.CountPerLabel.Length];
            Array.Copy(Dataset.CountPerLabel, Dataset.LastPerLabel, Dataset.CountPerLabel.Length);
        }
        int[] CountPerLabel = new int[Dataset.LabelCount + 1];
        for (int iz = 0; iz < Dataset.dimZ; iz++)
        {
            for (int iy = 0; iy < Dataset.dimY; iy++)
            {
                for (int ix = 0; ix < Dataset.dimX; ix++)
                {
                    int Currentid = Dataset.ConvertID(new IntV3(ix, iy, iz));
                    if (Dataset.data[Currentid] == 1)
                    {
                        CountPerLabel[Dataset.PointInfos[Currentid].Label]++;
                    }
                }
            }
        }
        Dataset.CountPerLabel = CountPerLabel;
    }
    public static void LabelPointByRadius(VolumeDataset Dataset, IntV3[] PointsInRadiusLabel, ref Queue<IntV3> PointQueue, float DotFilter, float NormalCrossFilter, float NotReliableFilter, ref int Now, int TestSequence, GameObject[] PointerForLabel, Transform LabelPointers, int MaxLength, IntV3[] PointsInRadiusNormal, NormalFunction NormalFunction,bool CrossUnreliableArea)
    {
        IntV3 Center = PointQueue.Dequeue();
        //Debug.Log(PointQueue.Count + " " + CurrentCenter.ToVector3());
        int CenterID = Dataset.ConvertID(Center);
        if (Dataset.PointInfos[CenterID].NotReliable >= NotReliableFilter)
            return;
        IntV3[] Points = RelativPointIDsInRadius(Center, PointsInRadiusLabel, Dataset, -1);
        if (Points.Length < 3) return;
        if (Dataset.PointInfos[CenterID].Label == 0) Debug.LogError("L==0!");
        for (int i = 0; i < Points.Length; i++)
        {
            //Debug.Log(CurrentCenter.ToVector3() + " " + Points[i].ToVector3());
            int CurrentIter = Dataset.ConvertID(Points[i]);
            //Debug.Log(!IntPosID.Eaquals(CurrentCenter, Points[i]) +""+ (Mathf.Abs(Vector3.Dot(CurrentCenter.ToVector3() - Points[i].ToVector3(), Dataset.PointInfos[CurrentID].Normal)) < DotFilter) +""+ Dataset.PointInfos[CurrentIter].Label);
            Vector3 Relative = Center.ToVector3() - Points[i].ToVector3();
            bool Labeled = false;
            if (!IntV3.Eaquals(Center, Points[i]) && Mathf.Abs(Vector3.Dot(Relative, Dataset.PointInfos[CenterID].Normal)) < DotFilter && (Dataset.PointInfos[CurrentIter].Label == 0 /*|| Dataset.PointInfos[CurrentIter].NormalChangeable*/))
            {
                int CenterLabel = Dataset.PointInfos[CenterID].Label;
                if (Dataset.PointInfos[CurrentIter].NotReliable < NotReliableFilter)
                {
                    if (Mathf.Abs(Vector3.Dot(Relative, Dataset.PointInfos[CurrentIter].Normal)) < DotFilter && Vector3.Cross(Dataset.PointInfos[CurrentIter].Normal, Dataset.PointInfos[CenterID].Normal).magnitude < NormalCrossFilter)
                    {
                        Dataset.PointInfos[CurrentIter].Label = CenterLabel;
                        /*Vector4 CurrentNormalV4 = NormalByPoints(Points[i], IntPosID.ToVector3(RelativPointIDsInRadius(Points[i], PointsInRadiusNormal, Dataset, CenterLabel)), NormalFunction, MaxLength);
                        if (CurrentNormalV4.w < Dataset.PointInfos[CurrentIter].NotReliable)
                        {
                            Dataset.PointInfos[CurrentIter].Normal = new Vector3(CurrentNormalV4.x, CurrentNormalV4.y, CurrentNormalV4.z);
                            Dataset.PointInfos[CurrentIter].NotReliable = CurrentNormalV4.w;
                        }*/
                        PointQueue.Enqueue(Points[i]);
                        Labeled = true;
                    }
                    else
                    {
                        continue;
                    }
                }
                else
                {
                    if(!CrossUnreliableArea)
                        continue;
                    Vector4 CurrentNormalV4 = NormalByPoints(Points[i], IntV3.ToVector3(RelativPointIDsInRadius(Points[i], PointsInRadiusNormal, Dataset, CenterLabel)), NormalFunction, MaxLength);
                    Vector3 CurrentNormalV3 = new Vector3(CurrentNormalV4.x, CurrentNormalV4.y, CurrentNormalV4.z);
                    Dataset.PointInfos[CurrentIter].Label = CenterLabel;
                    Labeled = true;
                    if (CurrentNormalV4.w < NotReliableFilter && Vector3.Cross(CurrentNormalV3, Dataset.PointInfos[CenterID].Normal).magnitude < NormalCrossFilter)
                    {
                        Dataset.PointInfos[CurrentIter].Normal = new Vector3(CurrentNormalV4.x, CurrentNormalV4.y, CurrentNormalV4.z);
                        Dataset.PointInfos[CurrentIter].NotReliable = CurrentNormalV4.w;
                        PointQueue.Enqueue(Points[i]);
                    }
                }
                if (Labeled)
                {
                    if (Now++ % TestSequence == 0)
                    {
                        int CurrentLabel = Dataset.PointInfos[CurrentIter].Label;
                        GameObject Pointer = GameObject.Instantiate(PointerForLabel[CurrentLabel % 10], Points[i].ToVector3(), new Quaternion(), LabelPointers);
                        float scale = 1 + (CurrentLabel + 10) / 10 * 0.1f;
                        Pointer.transform.localScale = new Vector3(scale, scale, scale);
                    }
                }
            }
        }
    }
    public static void MakeDataSetByFunction(int Dx, int Dy, int Dz)
    {
        VolumeDataset VDS = new VolumeDataset();
        VDS.data = new int[Dx * Dy * Dz];
        VDS.dimX = Dx;
        VDS.dimY = Dy;
        VDS.dimZ = Dz;
        for (int ix = 0; ix < Dx; ix++)
        {
            for (int iy = 0; iy < Dy; iy++)
            {
                for (int iz = 0; iz < Dz; iz++)
                {
                    VDS.data[VDS.ConvertID(ix, iy, iz)] = DataMakeFunction(ix, iy, iz);
                    if (VDS.data[VDS.ConvertID(ix, iy, iz)] == 1)
                        VDS.OneCount++;
                }
            }
        }
        VolumeObjectFactory.CreateObject(VDS);
    }
    static int DataMakeFunction(int X, int Y, int Z)
    {
        /*bool InsideSphere(int px, int py, int pz, int CenterX, int CenterY, int CenterZ, float Radius)
        {
            int Dx = px - CenterX;
            int Dy = py - CenterY;
            int Dz = pz - CenterZ;
            return Dx * Dx + Dy * Dy + Dz * Dz <= Radius * Radius;
        }
        bool InsideSphereShield(int px, int py, int pz, int CenterX, int CenterY, int CenterZ, float Radius, float Shield)
        {
            return InsideSphere(px, py, pz, CenterX, CenterY, CenterZ, Radius) && !InsideSphere(px, py, pz, CenterX, CenterY, CenterZ, Radius - Shield);
        }
        return (InsideSphereShield(X, Y, Z, 0, 0, 0, 120, 1.5f) || InsideSphereShield(X, Y, Z, 120, 0, 0, 100, 1.5f)) && Mathf.Abs(Z - 30f) >= 1 ? 1 : 0;
        */
        return Mathf.Abs(X - 64);
        //return Mathf.Abs(X + Y - 128f) <= 0.5 || Mathf.Abs(X - Y-0.0f) <= 0.5;
    }
    public static int LineConvolution(float[] ConvolutionKernel, float[] SampleLine, int SampleCenter, float[] CompareLine)
    {
        int MinID = -1;
        float Min = Mathf.Infinity;
        for (int SearchID = 0; SearchID < CompareLine.Length; SearchID++)
        {
            float SumWeight = 0;
            float CurrentDistance = 0;
            for (int KernelID = -(ConvolutionKernel.Length - 1); KernelID < ConvolutionKernel.Length; KernelID++)
            {
                int i = SampleCenter + KernelID;
                if (i < 0 || i >= SampleLine.Length) continue;
                int j = SearchID + KernelID;
                if (j < 0 || j >= CompareLine.Length) continue;
                float Distance = SampleLine[i] - CompareLine[j];
                float CurrentWeight = ConvolutionKernel[Mathf.Abs(KernelID)];
                CurrentDistance += Distance * Distance * CurrentWeight;
                SumWeight += CurrentWeight;
            }
            CurrentDistance /= SumWeight;
            if (CurrentDistance < Min)
            {
                Min = CurrentDistance;
                MinID = SearchID;
            }
        }
        if (MinID == -1) Debug.LogError("-1");
        return MinID;
    }
    public static int LineConvolutionForInt(float[] ConvolutionKernel, int[] SampleLine, int SampleCenter, int[] CompareLine)
    {
        //Debug.Log(SampleCenter);
        int MinID = -1;
        float Min = Mathf.Infinity;
        for (int SearchID = 0; SearchID < CompareLine.Length; SearchID++)
        {
            float SumWeight = 0;
            float CurrentDistance = 0;
            for (int KernelID = -(ConvolutionKernel.Length - 1); KernelID < ConvolutionKernel.Length; KernelID++)
            {
                int i = SampleCenter + KernelID;
                //i = (i + SampleLine.Length) % SampleLine.Length;
                if (i < 0 || i >= SampleLine.Length) continue;
                int j = SearchID + KernelID;
                //j = (j + CompareLine.Length) % CompareLine.Length;
                if (j < 0 || j >= CompareLine.Length) continue;
                float Distance = SampleLine[i] - CompareLine[j];
                float CurrentWeight = ConvolutionKernel[Mathf.Abs(KernelID)];
                CurrentDistance += Distance * Distance * CurrentWeight;
                SumWeight += CurrentWeight;
            }
            CurrentDistance /= SumWeight;
            if (CurrentDistance < Min)
            {
                Min = CurrentDistance;
                MinID = SearchID;
            }
        }
        if (MinID == -1) Debug.LogError("-1");

        /*if(Min == 0)
        {
            for(int i=0;i<SampleLine.Length;i++)
            {
                Debug.Log(SampleLine[i]);
                Debug.Log("///");
                Debug.Log(SampleLine[i]);
                Debug.Log("///");
                Debug.Log("///");
            }
        }*/
        return MinID;
    }
    public static int[] DatasetXLine(VolumeDataset Dataset, int Y, int Z)
    {
        int[] ToReturn = new int[Dataset.dimX];
        for (int i = 0; i < Dataset.dimX; i++)
        {
            int id = Dataset.ConvertID(i, Y, Z);
            if (id == -1) Debug.LogError(i+" "+Y+" "+Z);
            ToReturn[i] = Dataset.data[id];
        }
        return ToReturn;
    }
    public static int[] SliceConvolution(VolumeDataset Dataset, IntV3 Center, bool DimY, float[] ConvolutionKernel)
    {
        int[] Toreturn = new int[0];
        if (Dataset.ConvertID(Center) == -1)
        {
            Debug.Log("Out Of Range");
            return Toreturn;
        }
        if (DimY)
        {
            Toreturn = new int[Dataset.dimY];
            Toreturn[Center.Y] = Center.X;

            for (int i = 1; i < Dataset.dimY; i++)
            {
                bool Out = true;
                int CurrentID = Center.Y - i;
                if (CurrentID > 0)
                {
                    Out = false;

                    Toreturn[CurrentID] = LineConvolutionForInt(ConvolutionKernel, DatasetXLine(Dataset, CurrentID + 1, Center.Z), Toreturn[CurrentID + 1], DatasetXLine(Dataset, CurrentID, Center.Z));
                   
                }

                CurrentID = Center.Y + i;
                if (CurrentID < Dataset.dimY)
                {
                    Out = false;

                    Toreturn[CurrentID] = LineConvolutionForInt(ConvolutionKernel, DatasetXLine(Dataset, CurrentID - 1, Center.Z), 
                        Toreturn[CurrentID - 1], DatasetXLine(Dataset, CurrentID, Center.Z));
                }
                if (Out)
                {
                    break;
                }
            }
        }
        else
        {
            Toreturn = new int[Dataset.dimZ];
            Toreturn[Center.Z] = Center.X;

            for (int i = 1; i < Dataset.dimZ; i++)
            {
                bool Out = true;
                int CurrentID = Center.Z - i;
                if (CurrentID > 0)
                {
                    Out = false;

                    Toreturn[CurrentID] = LineConvolutionForInt(ConvolutionKernel, DatasetXLine(Dataset, Center.Y, CurrentID + 1), Toreturn[CurrentID + 1], DatasetXLine(Dataset, Center.Y, CurrentID));
                }

                CurrentID = Center.Z + i;
                if (CurrentID < Dataset.dimZ)
                {
                    Out = false;

                    Toreturn[CurrentID] = LineConvolutionForInt(ConvolutionKernel, DatasetXLine(Dataset, Center.Y, CurrentID - 1), Toreturn[CurrentID - 1], DatasetXLine(Dataset, Center.Y, CurrentID));
                }
                if (Out)
                {
                    break;
                }
            }
        }
        return Toreturn;
    }
    public static int[][] CubeConvolution(VolumeDataset Dataset, IntV3 Center, float[] ConvolutionKernel, GameObject[] Pointer,Transform P)
    {
        int[][] Toreturn = new int[Dataset.dimY][];
        void Set(int x, int y, int z,int i)
        {
            if (Toreturn[y][z] == -1)
            {
                Toreturn[y][z] = x;
                Transform CurrentPointer = GameObject.Instantiate(Pointer[i], P).transform;
                CurrentPointer.localPosition = new Vector3(x, y, z);
            }

        }
        if (Dataset.ConvertID(Center) == -1)
        {
            Debug.Log("Out Of Range");
            return Toreturn;
        }
        for (int y = 0; y < Dataset.dimY; y++)
        {
            Toreturn[y] = new int[Dataset.dimZ];
            for (int z = 0; z < Dataset.dimZ; z++)
            {
                Toreturn[y][z] = -1;
                //Set(LineConvolutionForInt(ConvolutionKernel, DatasetXLine(Dataset, Center.Y, Center.Z), Center.X, DatasetXLine(Dataset, y, z)), y, z, 1);
            }
        }
        Set(Center.X, Center.Y, Center.Z, 0);
        int MaxSquare = Mathf.Max(Dataset.dimY, Dataset.dimZ);
        for (int Square = 1; Square < MaxSquare; Square++)
        {
            if (Center.Y - Square < 0 && Center.Z - Square < 0 && Center.Y + Square >= Dataset.dimY && Center.Z + Square >= Dataset.dimZ)
                break;
            for (int y = Center.Y - Square; y <= Center.Y + Square; y++)
            {
                for (int z = Center.Z - Square; z <= Center.Z + Square; z++)
                {
                    if (Dataset.ConvertID(0, y, z) == -1 || Toreturn[y][z] != -1)
                        continue;
                    Set(ConvolutionInSquare(ConvolutionKernel, Dataset, y, z, Toreturn, 1), y, z, 1);
                }
            }
        }
        //Toreturn[Center.X][Center.Y] = Center.Z;
        /*
        bool StartFromY = Mathf.Min(Dataset.dimY - Center.Y, Center.Y) <= Mathf.Min(Dataset.dimZ - Center.Z, Center.Z);
        if (StartFromY)
        {
            int[] YLine = SliceConvolution(Dataset, Center, true, ConvolutionKernel);
            for (int y = 0; y < YLine.Length; y++)
            {
                Set(YLine[y], y, Center.Z,1);
                int[] ZLine = SliceConvolution(Dataset, new IntV3(YLine[y], y,Center.Z), false, ConvolutionKernel);
                for (int z = 0; z < Dataset.dimZ; z++)
                {
                    Set(ZLine[z], y, z,2);
                    //Toreturn[x][y] = YLine[y];
                }
            }
        }
        else 
        {
            int[] ZLine = SliceConvolution(Dataset, Center, false, ConvolutionKernel);
            for (int z = 0; z < ZLine.Length; z++)
            {
                Set(ZLine[z], Center.Y, z,1);
                int[] YLine = SliceConvolution(Dataset, new IntV3(ZLine[z], Center.Y, z), true, ConvolutionKernel);
                for (int y = 0; y < Dataset.dimX; y++)
                {
                    Set(YLine[y], y, z,2);
                    //Toreturn[x][y] = XLine[x];
                }
            }
        }
         */
        return Toreturn;
    }
    public static float[] MakeKernel(float A,float C)
    {
        //aX^2+C
        //X=(-C/a)^0.5
        if (!(A < 0) || !(C > 0))
        {
            Debug.LogError("-");
            return new float[0];
        }
        //Debug.Log((int)Mathf.Sqrt(-C / A));
        float[] ToReturn = new float[(int)Mathf.Sqrt(-C / A)];
        for (int i = 0; i < ToReturn.Length; i++)
        {
            ToReturn[i] = A  *i* i + C;
        }
        return ToReturn;
    }
    public static int ConvolutionInSquare(float[] ConvolutionKernel, VolumeDataset dataset,int ToCompareY,int ToCompareZ,int [][] Result,int Square)
    {
        int Count = 0;
        int ID = 0;
        for (int y = -Square; y <= Square; y++)
        {
            for (int z = -Square; z <= Square; z++)
            {
                if (y == 0 && z == 0)
                {
                    continue;
                }
                int Y = ToCompareY + y;

                int Z = ToCompareZ + z;
                if (dataset.ConvertID(0, Y, Z) == -1 || Result[Y][Z] == -1)
                {
                    continue;
                }
                Count++;
                ID += LineConvolutionForInt(ConvolutionKernel, DatasetXLine(dataset, Y, Z), Result[Y][Z], DatasetXLine(dataset, ToCompareY, ToCompareZ));
            }
        }
        if (Count == 0) return -1;
        else return ID / Count;
    }
    public static void OutputData(int[] DataToOutput, IntV3 Size, string filePath, string FileName)
    {
        FileStream fs2 = new FileStream(filePath + "/" + FileName + ".dat", FileMode.Create);
        BinaryWriter writer = new BinaryWriter(fs2);
        int uDimension = Size.X * Size.Y * Size.Z;
        for (int i = 0; i < uDimension; i++)
        {
            writer.Write(DataToOutput[i]);
        }
        writer.Close();
    }
}
public enum NormalFunction
{
    Average, Median, PanelFit, RefreshPerPoint
}
public struct Panel
{
    /// <summary>
    /// 平面方程拟合，ax+by+cz+d=0,其中a=result[0],b=result[1],c=result[2],d=result[3]
    public static Vector4 PanelFit(Vector3[] Points)
    {
        Vector4 ToReturn = Vector4.zero;
        int n = Points.Length;
        double[,] A = new double[n, 3];
        double[,] E = new double[n, 1];
        for (int i = 0; i < n; i++)
        {
            A[i, 0] = Points[i].x - Points[i].z;
            A[i, 1] = Points[i].y - Points[i].z;
            A[i, 2] = 1;
            E[i, 0] = -Points[i].z;
        }
        double[,] AT = MatrixInver(A);
        double[,] ATxA = MatrixMultiply(AT, A);
        double[,] OPPAxTA = MatrixOpp(ATxA);
        double[,] OPPATAxAT = MatrixMultiply(OPPAxTA, AT);
        double[,] DP = MatrixMultiply(OPPATAxAT, E);
        ToReturn.x =(float) DP[0, 0];
        ToReturn.y = (float)DP[1, 0];
        ToReturn.z = 1 - ToReturn.x - ToReturn.y;
        ToReturn.w = (float)DP[2, 0];
        return ToReturn;
    }
    public static double[] PanelFit(double[] x, double[] y, double[] z)
    {
        double[] result = new double[4];
        int n = x.Length;
        double[,] A = new double[n, 3];
        double[,] E = new double[n, 1];
        for (int i = 0; i < n; i++)
        {
            A[i, 0] = x[i] - z[i];
            A[i, 1] = y[i] - z[i];
            A[i, 2] = 1;
            E[i, 0] = -z[i];
        }
        double[,] AT = MatrixInver(A);
        double[,] ATxA = MatrixMultiply(AT, A);
        double[,] OPPAxTA = MatrixOpp(ATxA);
        double[,] OPPATAxAT = MatrixMultiply(OPPAxTA, AT);
        double[,] DP = MatrixMultiply(OPPATAxAT, E);
        result[0] = DP[0, 0];
        result[1] = DP[1, 0];
        result[2] = 1 - result[0] - result[1];
        result[3] = DP[2, 0];
        return result;
    }
    /// 矩阵转置
    public static double[,] MatrixInver(double[,] matrix)
    {
        double[,] result = new double[matrix.GetLength(1), matrix.GetLength(0)];
        for (int i = 0; i < matrix.GetLength(1); i++)
            for (int j = 0; j < matrix.GetLength(0); j++)
                result[i, j] = matrix[j, i];
        return result;
    }
    /// 矩阵相乘
    public static double[,] MatrixMultiply(double[,] matrixA, double[,] matrixB)
    {
        double[,] result = new double[matrixA.GetLength(0), matrixB.GetLength(1)];
        for (int i = 0; i < matrixA.GetLength(0); i++)
        {
            for (int j = 0; j < matrixB.GetLength(1); j++)
            {
                result[i, j] = 0;
                for (int k = 0; k < matrixB.GetLength(0); k++)
                {
                    result[i, j] += matrixA[i, k] * matrixB[k, j];
                }
            }
        }
        return result;
    }
    /// 矩阵的逆
    public static double[,] MatrixOpp(double[,] matrix)
    {
        double X = 1 / MatrixSurplus(matrix);
        double[,] matrixB = new double[matrix.GetLength(0), matrix.GetLength(1)];
        double[,] matrixSP = new double[matrix.GetLength(0), matrix.GetLength(1)];
        double[,] matrixAB = new double[matrix.GetLength(0), matrix.GetLength(1)];

        for (int i = 0; i < matrix.GetLength(0); i++)
            for (int j = 0; j < matrix.GetLength(1); j++)
            {
                for (int m = 0; m < matrix.GetLength(0); m++)
                    for (int n = 0; n < matrix.GetLength(1); n++)
                        matrixB[m, n] = matrix[m, n];
                {
                    for (int x = 0; x < matrix.GetLength(1); x++)
                        matrixB[i, x] = 0;
                    for (int y = 0; y < matrix.GetLength(0); y++)
                        matrixB[y, j] = 0;
                    matrixB[i, j] = 1;
                    matrixSP[i, j] = MatrixSurplus(matrixB);
                    matrixAB[i, j] = X * matrixSP[i, j];
                }
            }
        return MatrixInver(matrixAB);
    }
    /// 矩阵的行列式的值  
    public static double MatrixSurplus(double[,] matrix)
    {
        double X = -1;
        double[,] a = matrix;
        int i, j, k, p, r, m, n;
        m = a.GetLength(0);
        n = a.GetLength(1);
        double temp = 1, temp1 = 1, s = 0, s1 = 0;

        if (n == 2)
        {
            for (i = 0; i < m; i++)
                for (j = 0; j < n; j++)
                    if ((i + j) % 2 > 0) temp1 *= a[i, j];
                    else temp *= a[i, j];
            X = temp - temp1;
        }
        else
        {
            for (k = 0; k < n; k++)
            {
                for (i = 0, j = k; i < m && j < n; i++, j++)
                    temp *= a[i, j];
                if (m - i > 0)
                {
                    for (p = m - i, r = m - 1; p > 0; p--, r--)
                        temp *= a[r, p - 1];
                }
                s += temp;
                temp = 1;
            }

            for (k = n - 1; k >= 0; k--)
            {
                for (i = 0, j = k; i < m && j >= 0; i++, j--)
                    temp1 *= a[i, j];
                if (m - i > 0)
                {
                    for (p = m - 1, r = i; r < m; p--, r++)
                        temp1 *= a[r, p];
                }
                s1 += temp1;
                temp1 = 1;
            }

            X = s - s1;
        }
        return X;
    }
}