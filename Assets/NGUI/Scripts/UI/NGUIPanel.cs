using UnityEngine;
using System.Collections;

public class NGUIPanel : CacheBehaviour
{
	void Update ()
    {
        try
        {
            UI2DSprite.CustomUpdate();
            UIPanel.CustomUpdate();
        }
        catch (System.Exception ex)
        {
            //Games.TLBB.Util.GameUtil.LogAndReportException(ex, "NGUIPanel.Update");
        }

    }

    void LateUpdate()
    {
        try
        {
            UIPanel.LateCustomUpdate();
        }
        catch (System.Exception ex)
        {
            //Games.TLBB.Util.GameUtil.LogAndReportException(ex, "NGUIPanel.LateUpdate");
        }
        
    }

}
