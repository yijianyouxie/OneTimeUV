using UnityEngine;
using System.Collections;

public class UVFlowController : VisiableUpdateHandler {

    private Renderer render;
    private Material mat;

    //流逝的总时间
    private float elapseTime = 0f;
    [Header("====主贴图的UV的变化值。-1表示刚刚看到；0表示完全显示；1表示完全消失")]
    //public float _UVFrom = -1f;
    //public float _UVTo = 0f;
    //public float _duration = 1f;
    public bool mainUV_FlowOnce = true;
    //public Vector2 mainUV_FromTo = new Vector2(-1f, 0f);
    [Header("曲线的x轴是时间轴，可以定义持续时间。y轴是UV中U值。")]
    public AnimationCurve mainUV_Curve;
    public Vector2 mainUV_ScrollSpeed = new Vector2(0f, 0f);
    [Space(50)]

    private bool glossUV_FlowOnce = false;
    [Header("====相乘贴图的UV变化速度。========")]
    public Vector2 glossUV_FromTo = new Vector2(0f, 1f);
    public Vector2 glossUV_ScrollSpeed = new Vector2(0f, 0f);

    
    [Space(20)]
    private bool adjustUV_FlowOnce = false;
    [Header("====相加贴图的UV变化速度。========")]
    public Vector2 adjustUV_FromTo = new Vector2(-1f, 0f);
    public Vector2 adjustUV_ScrollSpeed = new Vector2(0f, 0f);

    [Header("====开始溶解的时间点。默认为-1，表示不进行溶解。")]
    public AnimationCurve dissoveUV_Curve;

    // Use this for initialization
    public override void Start()
    {
        base.Start();
        render = GetComponent<Renderer>();
        if (render == null)
        {
            Destroy(this);
            return;
        }
        mat = render.material;
    }

    // Update is called once per frame
    public override void Tick()
    { 
        if (render == null || mat == null)
        {
            return;
        }
        //时间流逝
        elapseTime += Time.deltaTime;
        var offsetX = 0f;
        var offsetY = 0f;

        //主贴图=======================================
        if (mainUV_FlowOnce)
        {
            float currValue = mainUV_Curve.Evaluate(elapseTime);
            offsetX = (mainUV_ScrollSpeed.x != 0f ? currValue : 0f);
            offsetY = (mainUV_ScrollSpeed.y != 0f ? currValue : 0f);
        }
        else
        {
            offsetX = elapseTime * mainUV_ScrollSpeed.x;
            if (offsetX > 1)
                offsetX -= (int)(offsetX);
            else if (offsetX < -1)
                offsetX -= (int)(offsetX);

            offsetY = elapseTime * mainUV_ScrollSpeed.y;
            if (offsetY > 1)
                offsetY -= (int)(offsetY);
            else if (offsetY < -1)
                offsetY -= (int)(offsetY);
        }
        mat.mainTextureOffset = new Vector2(offsetX, offsetY);

        //流光贴图=======================================
        if (glossUV_FlowOnce)
        {
            bool lowToHigh = glossUV_FromTo.x <= glossUV_FromTo.y;
            offsetX = elapseTime * glossUV_ScrollSpeed.x + glossUV_FromTo.x * (glossUV_ScrollSpeed.x != 0f ? 1f : 0f);
            if ((lowToHigh && offsetX >= glossUV_FromTo.y) || (!lowToHigh && offsetX <= glossUV_FromTo.y))
            {
                offsetX = glossUV_FromTo.y;
            }
            offsetY = elapseTime * glossUV_ScrollSpeed.y + glossUV_FromTo.x * (glossUV_ScrollSpeed.y != 0f ? 1f : 0f);
            if ((lowToHigh && offsetY >= glossUV_FromTo.y) || (!lowToHigh && offsetY <= glossUV_FromTo.y))
            {
                offsetY = glossUV_FromTo.y;
            }
        }
        else
        {
            offsetX = elapseTime * glossUV_ScrollSpeed.x;
            if (offsetX > 1)
                offsetX -= (int)(offsetX);
            else if (offsetX < -1)
                offsetX -= (int)(offsetX);

            offsetY = elapseTime * glossUV_ScrollSpeed.y;
            if (offsetY > 1)
                offsetY -= (int)(offsetY);
            else if (offsetY < -1)
                offsetY -= (int)(offsetY);
        }
        mat.SetTextureOffset("_GlossTex", new Vector2(offsetX, offsetY));

        //相加贴图===================================
        if (adjustUV_FlowOnce)
        {
            bool lowToHigh = adjustUV_FromTo.x <= adjustUV_FromTo.y;
            offsetX = elapseTime * adjustUV_ScrollSpeed.x + adjustUV_FromTo.x * (adjustUV_ScrollSpeed.x != 0f ? 1f : 0f);
            if ((lowToHigh && offsetX >= adjustUV_FromTo.y) || (!lowToHigh && offsetX <= adjustUV_FromTo.y))
            {
                offsetX = adjustUV_FromTo.y;
            }
            offsetY = elapseTime * adjustUV_ScrollSpeed.y + adjustUV_FromTo.x * (adjustUV_ScrollSpeed.y != 0f ? 1f : 0f);
            if ((lowToHigh && offsetY >= adjustUV_FromTo.y) || (!lowToHigh && offsetY <= adjustUV_FromTo.y))
            {
                offsetY = adjustUV_FromTo.y;
            }
        }
        else
        {
            offsetX = elapseTime * adjustUV_ScrollSpeed.x;
            if (offsetX > 1)
                offsetX -= (int)(offsetX);
            else if (offsetX < -1)
                offsetX -= (int)(offsetX);

            offsetY = elapseTime * adjustUV_ScrollSpeed.y;
            if (offsetY > 1)
                offsetY -= (int)(offsetY);
            else if (offsetY < -1)
                offsetY -= (int)(offsetY);
        }

        mat.SetTextureOffset("_AdjustTex", new Vector2(offsetX, offsetY));

        //溶解
        float dissoveValue = dissoveUV_Curve.Evaluate(elapseTime);
        mat.SetFloat("_AlphaCutoff", dissoveValue);
    }

    [ContextMenu("ResetAndPlay")]
    public void ResetAndPlay()
    {
        elapseTime = 0f;
        if (render == null || mat == null)
        {
            return;
        }
        mat.SetFloat("_AlphaCutoff", 0f);
    }
}
