//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2015 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// Very simple sprite animation. Attach to a sprite and specify a common prefix such as "idle" and it will cycle through them.
/// </summary>

[ExecuteInEditMode]
[RequireComponent(typeof(UISprite))]
[AddComponentMenu("NGUI/UI/Sprite AnimationEx")]
public class UISpriteAnimationEx : CacheBehaviour
{
    public static int spritesMaxNum = 10;
    /*[HideInInspector]*/[SerializeField] protected int mFPS = 5;
	/*[HideInInspector]*/[SerializeField] protected string mPrefix = "";
	/*[HideInInspector]*/[SerializeField] protected bool mLoop = true;
	/*[HideInInspector]*/[SerializeField] protected bool mSnap = true;

	protected UISprite mSprite;
	protected float mDelta = 0f;
	protected int mIndex = 0;
	protected bool mActive = true;
	protected List<string> mSpriteNames = new List<string>();

	/// <summary>
	/// Number of frames in the animation.
	/// </summary>

	public int frames { get { return mSpriteNames.Count; } }

	/// <summary>
	/// Animation framerate.
	/// </summary>

	public int framesPerSecond { get { return mFPS; } set { mFPS = value; } }

	/// <summary>
	/// Set the name prefix used to filter sprites from the atlas.
	/// </summary>

	public string namePrefix { get { return mPrefix; } set { if (mPrefix != value) { mPrefix = value; RebuildSpriteList(); } } }
	/// <summary>
	/// Set the animation to be looping or not
	/// </summary>

	public bool loop { get { return mLoop; } set { mLoop = value; } }

    public bool snap { get { return mSnap; } set { mSnap = value; } }
    /// <summary>
    /// Returns is the animation is still playing or not
    /// </summary>

    public bool isPlaying { get { return mActive; } }

	/// <summary>
	/// Rebuild the sprite list first thing.
	/// </summary>

	protected virtual void Start () { RebuildSpriteList(); }

	/// <summary>
	/// Advance the sprite animation process.
	/// </summary>

	protected virtual void Update ()
	{
		if (mActive && mSpriteNames.Count > 1 && Application.isPlaying && mFPS > 0)
		{
			mDelta += Time.deltaTime;
			float rate = 1f / mFPS;

			if (rate < mDelta)
			{
				mDelta = (rate > 0f) ? mDelta - rate : 0f;

				if (++mIndex >= mSpriteNames.Count)
				{
					mIndex = 0;
					mActive = mLoop;
				}

				if (mActive)
				{
					mSprite.spriteName = mSpriteNames[mIndex];
					if (mSnap) mSprite.MakePixelPerfect();
				}
			}
		}
	}

    //初步分析是mSprite.atlas.spriteList数据填充错误导致，后续需要查询根本问题。
    //private static Dictionary<string, List<string>> sSpriteNameLists = new Dictionary<string, List<string>>();

    /// <summary>
    /// Rebuild the sprite list after changing the sprite name.
    /// </summary>
    public void RebuildSpriteList()
    {
        if (mSprite == null) mSprite = GetComponent<UISprite>();

        if (false == false/*LangUtil.emSptListDic.ContainsKey(mPrefix)*/)
        {
            mSpriteNames.Clear();

            if (mSprite != null && mSprite.atlas != null)
            {
                List<UISpriteData> sprites = mSprite.atlas.spriteList;
                for (int m = 1; m <= spritesMaxNum; m++)
                {
                    StringBuilder tempPrefix = new StringBuilder(mPrefix);
                    tempPrefix.Append(string.Format("{0:00}", m));
                    for (int i = 0, imax = sprites.Count; i < imax; ++i)
                    {
                        UISpriteData sprite = sprites[i];
                        string tempPrefixStr = tempPrefix.ToString();
                        if (sprite.name == tempPrefixStr)
                        {
                            mSpriteNames.Add(tempPrefixStr);
                        }
                    }
                }
                //匹配没有序列图的表情
                if (mSpriteNames.Count <= 0)
                {
                    mSpriteNames.Add(mPrefix);
                }
            }

            //    sSpriteNameLists[mPrefix] = mSpriteNames;
        }
        //else
            //mSpriteNames = LangUtil.emSptListDic[mPrefix];
    }
	
	/// <summary>
	/// Reset the animation to the beginning.
	/// </summary>

	public void Play () { mActive = true; }

	/// <summary>
	/// Pause the animation.
	/// </summary>

	public void Pause () { mActive = false; }

	/// <summary>
	/// Reset the animation to frame 0 and activate it.
	/// </summary>

	public void ResetToBeginning ()
	{
		mActive = true;
		mIndex = 0;

		if (mSprite != null && mSpriteNames.Count > 0)
		{
			mSprite.spriteName = mSpriteNames[mIndex];
			if (mSnap) mSprite.MakePixelPerfect();
		}
	}
}
