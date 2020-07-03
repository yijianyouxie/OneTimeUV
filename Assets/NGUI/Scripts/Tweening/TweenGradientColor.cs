//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2015 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;

/// <summary>
/// Tween the object's color.
/// </summary>

[AddComponentMenu("NGUI/Tween/Tween Gradient Color")]
public class TweenGradientColor : UITweener
{
	public Color topFrom = Color.white;
	public Color topTo = Color.white;

    public Color bottomFrom = Color.white;
    public Color bottomTo = Color.white;

    bool mCached = false;
	UILabel mUILabel;

	void Cache ()
	{
		mCached = true;
		mUILabel = GetComponent<UILabel>();
		if (mUILabel != null) return;
	}

	/// <summary>
	/// Tween's current value.
	/// </summary>

	public Color topValue
	{
		get
		{
			if (!mCached) Cache();
			if (mUILabel != null) return mUILabel.gradientTop;
			return Color.black;
		}
		set
		{
			if (!mCached) Cache();
			if (mUILabel != null) mUILabel.gradientTop = value;
		}
	}

    public Color bottomValue
    {
        get
        {
            if (!mCached) Cache();
            if (mUILabel != null) return mUILabel.gradientBottom;
            return Color.black;
        }
        set
        {
            if (!mCached) Cache();
            if (mUILabel != null) mUILabel.gradientBottom = value;
        }
    }

    /// <summary>
    /// Tween the value.
    /// </summary>

    protected override void OnUpdate (float factor, bool isFinished)
    {
        topValue = Color.Lerp(topFrom, topTo, factor);
        bottomValue = Color.Lerp(bottomFrom, bottomTo, factor);
    }

	/// <summary>
	/// Start the tweening operation.
	/// </summary>

	static public TweenGradientColor Begin (GameObject go, float duration, Color colorTop, Color colorBottom)
	{
#if UNITY_EDITOR
		if (!Application.isPlaying) return null;
#endif
		TweenGradientColor comp = UITweener.Begin<TweenGradientColor>(go, duration);
		comp.topFrom = comp.topValue;
		comp.topTo = colorTop;

        comp.bottomFrom = comp.bottomValue;
        comp.bottomTo = colorBottom;
        

		if (duration <= 0f)
		{
			comp.Sample(1f, true);
			comp.enabled = false;
		}
		return comp;
	}

	[ContextMenu("Set 'From' to current value")]
	public override void SetStartToCurrentValue ()
    {
        topFrom = topValue;
        bottomFrom = bottomValue;
    }

	[ContextMenu("Set 'To' to current value")]
	public override void SetEndToCurrentValue ()
    {
        topTo = topValue;
        bottomTo = bottomValue;
    }

	[ContextMenu("Assume value of 'From'")]
	void SetCurrentValueToStart ()
    {
        topValue = topFrom;
        bottomValue = bottomFrom;
    }

	[ContextMenu("Assume value of 'To'")]
	void SetCurrentValueToEnd ()
    {
        topValue = topTo;
        bottomValue = bottomTo;
    }
}
