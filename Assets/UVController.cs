using UnityEngine;
using System.Collections.Generic;

public class UVController : UITweener
{
    [Header("UV的变化值。0表示刚刚看到；1表示完全消失。0.5表示完全显示")]
    public float from;
    public float to;

    [Header("选择U还是V流动。")]
    public bool U_Flow = false;
    public bool V_Flow = true;

    [Header("开始溶解的时间点。")]
    public float dissoveTime = 0f;
    //动画开始后流逝的时间
    private float elapseTime = 0f;

    private Material mat;

    private void Awake()
    {
        if(null == mat)
        {
            MeshRenderer mr = transform.GetComponent<MeshRenderer>();
            if(null != mr)
            {
                mat = mr.material;
            }
        }

        if(null == mat)
        {
            enabled = false;
        }else
        {
            mat.SetFloat("_UFlow", 0f);
            mat.SetFloat("_VFlow", 0f);
        }
    }

    protected override void OnUpdate(float factor, bool isFinished)
    {
        float value = from * (1f - factor) + to * factor;

        if (null != mat)
        {
            if(U_Flow)
            {
                mat.SetFloat("_UFlow", 1f);
            }
            if(V_Flow)
            {
                mat.SetFloat("_VFlow", 1f);
            }
            mat.SetFloat("_UVCurve", value);
        }

        elapseTime += (ignoreTimeScale ? RealTime.deltaTime : Time.deltaTime);
        if (elapseTime > dissoveTime)
        {
            float cutOff = (elapseTime - dissoveTime) / (duration - dissoveTime);
            //开始溶解
            mat.SetFloat("_Cutoff", cutOff);
            //Debug.LogError("============factor:"+ factor + " cutOff:" + cutOff);
        }

    }

    [ContextMenu("ResetAndPlay")]
    public void ResetAndPlay()
    {
        ResetToBeginning();
        PlayForward();
    }
}
