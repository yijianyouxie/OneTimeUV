using UnityEngine;
using System.Collections.Generic;

public class UVController : UITweener
{
    public float from;
    public float to;

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
        }
    }

    protected override void OnUpdate(float factor, bool isFinished)
    {
        float value = from * (1f - factor) + to * factor;

        mat.SetFloat("_lifealpha", value);
    }

    [ContextMenu("ResetAndPlay")]
    public void ResetAndPlay()
    {
        ResetToBeginning();
        PlayForward();
    }
}
