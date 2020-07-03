
//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2015 Tasharen Entertainment
//----------------------------------------------

//#define SHOW_HIDDEN_OBJECTS

using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// This is an internally-created script used by the UI system. You shouldn't be attaching it manually.
/// </summary>

[ExecuteInEditMode]
[AddComponentMenu("NGUI/Internal/Draw Call")]
public class UIDrawCall : CacheBehaviour
{
	static BetterList<UIDrawCall> mActiveList = new BetterList<UIDrawCall>();
	static BetterList<UIDrawCall> mInactiveList = new BetterList<UIDrawCall>();
    public Vector3 lastpos = Vector3.zero;
	[System.Obsolete("Use UIDrawCall.activeList")]
	static public BetterList<UIDrawCall> list { get { return mActiveList; } }

	/// <summary>
	/// List of active draw calls.
	/// </summary>

	static public BetterList<UIDrawCall> activeList { get { return mActiveList; } }

	/// <summary>
	/// List of inactive draw calls. Only used at run-time in order to avoid object creation/destruction.
	/// </summary>

	static public BetterList<UIDrawCall> inactiveList { get { return mInactiveList; } }

	public enum Clipping : int
	{
		None = 0,
		TextureMask = 1,			// Clipped using a texture rather than math
		SoftClip = 3,				// Alpha-based clipping with a softened edge
		ConstrainButDontClip = 4,	// No actual clipping, but does have an area
	}

	[HideInInspector][System.NonSerialized] public int widgetCount = 0;
	[HideInInspector][System.NonSerialized] public int depthStart = int.MaxValue;
	[HideInInspector][System.NonSerialized] public int depthEnd = int.MinValue;
	[HideInInspector][System.NonSerialized] public UIPanel manager;
	[HideInInspector][System.NonSerialized] public UIPanel panel;
	[HideInInspector][System.NonSerialized] public Texture2D clipTexture;
	[HideInInspector][System.NonSerialized] public bool alwaysOnScreen = false;
	[HideInInspector][System.NonSerialized] public BetterList<Vector3> verts = new BetterList<Vector3>();
	[HideInInspector][System.NonSerialized] public BetterList<Vector2> uvs = new BetterList<Vector2>();
	[HideInInspector][System.NonSerialized] public BetterList<Color32> cols = new BetterList<Color32>();

	Material		mMaterial;		// Material used by this draw call
	Texture			mTexture;		// Main texture used by the material
	Shader			mShader;		// Shader used by the dynamically created material
	int				mClipCount = 0;	// Number of times the draw call's content is getting clipped
	Transform		mTrans;			// Cached transform
	Mesh			mMesh;			// First generated mesh
	MeshFilter		mFilter;		// Mesh filter for this draw call
	MeshRenderer	mRenderer;		// Mesh renderer for this screen
	Material		mDynamicMat;	// Instantiated material
	int[]			mIndices;		// Cached indices

	bool mRebuildMat = true;
	bool mLegacyShader = false;
	int mRenderQueue = 3000;
	int mTriangles = 0;
    /// <summary>
    /// 表示这个drawcall当前的mesh的indexbuffer的大小，例如mesh.triangles里有6个index，这个数字肯定也为6
    /// </summary>
    int mMeshIndexCount = 0;

    const int mMaxTemplateVertexCount = 20000;
    const int mMaxTemplateIndexCount = (mMaxTemplateVertexCount >> 1) * 3;
    static int[] mIndexTemplate = null;

    private bool mIsEmpty = false;
    
	public bool IsEmpty
    {
        get { return mIsEmpty; }
        set { mIsEmpty = value; }
    }

	/// <summary>
	/// Whether the draw call has changed recently.
	/// </summary>

	[System.NonSerialized]
	public bool isDirty = false;

	[System.NonSerialized]
	bool mTextureClip = false;

	public delegate void OnRenderCallback (Material mat);

	/// <summary>
	/// Callback that will be triggered at OnWillRenderObject() time.
	/// </summary>

	public OnRenderCallback onRender;

	/// <summary>
	/// Render queue used by the draw call.
	/// </summary>

	public int renderQueue
	{
		get
		{
			return mRenderQueue;
		}
		set
		{
			if (mRenderQueue != value)
			{
				mRenderQueue = value;

				if (mDynamicMat != null)
				{
					mDynamicMat.renderQueue = value;
#if UNITY_EDITOR
					if (mRenderer != null) mRenderer.enabled = isActive;
#endif
				}
			}
		}
	}

	/// <summary>
	/// Renderer's sorting order, to be used with Unity's 2D system.
	/// </summary>

	public int sortingOrder
	{
		get { return (mRenderer != null) ? mRenderer.sortingOrder : 0; }
		set { if (mRenderer != null && mRenderer.sortingOrder != value) mRenderer.sortingOrder = value; }
	}

	/// <summary>
	/// Final render queue used to draw the draw call's geometry.
	/// </summary>

	public int finalRenderQueue
	{
		get
		{
			return (mDynamicMat != null) ? mDynamicMat.renderQueue : mRenderQueue;
		}
	}

#if UNITY_EDITOR

	/// <summary>
	/// Whether the draw call is currently active.
	/// </summary>
    
	public bool isActive
	{
		get
		{
			return mActive;
		}
		set
		{
			if (mActive != value)
			{
				mActive = value;

				if (mRenderer != null)
				{
					mRenderer.enabled = value;
					NGUITools.SetDirty(gameObject);
				}
			}
		}
	}
	bool mActive = true;
#endif

	/// <summary>
	/// Transform is cached for speed and efficiency.
	/// </summary>

	public Transform cachedTransform { get { if (mTrans == null) mTrans = transform; return mTrans; } }

	/// <summary>
	/// Material used by this screen.
	/// </summary>

	public Material baseMaterial
	{
		get
		{
			return mMaterial;
		}
		set
		{
			if (mMaterial != value)
			{
				mMaterial = value;
				mRebuildMat = true;
			}
		}
	}

	/// <summary>
	/// Dynamically created material used by the draw call to actually draw the geometry.
	/// </summary>

	public Material dynamicMaterial { get { return mDynamicMat; } }

	/// <summary>
	/// Texture used by the material.
	/// </summary>

	public Texture mainTexture
	{
		get
		{
			return mTexture;
		}
		set
		{
			mTexture = value;
			if (mDynamicMat != null) mDynamicMat.mainTexture = value;
		}
	}



	/// <summary>
	/// Shader used by the material.
	/// </summary>

	public Shader shader
	{
		get
		{
			return mShader;
		}
		set
		{
			if (mShader != value)
			{
				mShader = value;
				mRebuildMat = true;
			}
		}
	}

	/// <summary>
	/// The number of triangles in this draw call.
	/// </summary>

	public int triangles { get { return (mMesh != null) ? mTriangles : 0; } }

	/// <summary>
	/// Whether the draw call is currently using a clipped shader.
	/// </summary>

	public bool isClipped { get { return mClipCount != 0; } }
    private static Dictionary<string, string> s_ShaderNameDic_Hidden_ClipCount_1 = new Dictionary<string, string>();
    private static Dictionary<string, string> s_ShaderNameDic_Hidden_ClipCount_2 = new Dictionary<string, string>();
    private static Dictionary<string, string> s_ShaderNameDic_Hidden_ClipCount_3 = new Dictionary<string, string>();

    private static Dictionary<string, string> s_ShaderNameDic_ShaderNull_Hidden_ClipCount_1 = new Dictionary<string, string>();
    private static Dictionary<string, string> s_ShaderNameDic_ShaderNull_Hidden_ClipCount_2 = new Dictionary<string, string>();
    private static Dictionary<string, string> s_ShaderNameDic_ShaderNull_Hidden_ClipCount_3 = new Dictionary<string, string>();

    private static Dictionary<string, string> s_ShaderNameDic_TextureMask = new Dictionary<string, string>();
    private static Dictionary<string, string> s_ShaderNameDic_Soft = new Dictionary<string, string>();

    private static Dictionary<string, string> s_ShaderNameDic_Base_Calculate = new Dictionary<string, string>();

    
    public static string CalculateString(string shaderName)
    {
		string retName;
		if (s_ShaderNameDic_Base_Calculate.TryGetValue(shaderName, out retName))
		{
        	return retName;
		}

		string startShaderName = shaderName;
    	
        // Figure out the normal shader's name
        shaderName = shaderName.Replace("GUI/Text Shader", "Unlit/Text");

        if (shaderName.Length > 2)
        {
            if (shaderName[shaderName.Length - 2] == ' ')
            {
                int index = shaderName[shaderName.Length - 1];
                if (index > '0' && index <= '9') shaderName = shaderName.Substring(0, shaderName.Length - 2);
            }
        }

        if (shaderName.StartsWith("Hidden/"))
            shaderName = shaderName.Substring(7);

        // Legacy functionality 
        const string soft = " (SoftClip)";
        shaderName = shaderName.Replace(soft, "");

        const string textureClip = " (TextureClip)";
        shaderName = shaderName.Replace(textureClip, "");
        s_ShaderNameDic_Base_Calculate.Add(startShaderName, shaderName);
        return shaderName;

    }
    
    public string CreateNoGCShaderName_Hidden_ClipCount(string shaderName)
    {
        if(string.IsNullOrEmpty(shaderName))
        {
            return string.Empty;
        }
        string retName;
        if(mClipCount == 1)
        {
            if (s_ShaderNameDic_Hidden_ClipCount_1.TryGetValue(shaderName, out retName))
            {
                //Debug.LogError("shadername=" + retName);
                return retName;
            }
        }
        else if (mClipCount == 2)
        {
            if (s_ShaderNameDic_Hidden_ClipCount_2.TryGetValue(shaderName, out retName))
            {
                //Debug.LogError("shadername=" + retName);
                return retName;
            }
        }
        else if (mClipCount == 3)
        {
            if (s_ShaderNameDic_Hidden_ClipCount_3.TryGetValue(shaderName, out retName))
            {
                //Debug.LogError("shadername=" + retName);
                return retName;
            }
        }

        string startShaderName = shaderName;
        shaderName = CalculateString(shaderName);
        shaderName = "Hidden/" + shaderName + " " + mClipCount;
        if (mClipCount == 1)
        {
            s_ShaderNameDic_Hidden_ClipCount_1.Add(startShaderName, shaderName);
        }
        else if (mClipCount == 2)
        {
            s_ShaderNameDic_Hidden_ClipCount_2.Add(startShaderName, shaderName);
        }
        else if (mClipCount == 3)
        {
            s_ShaderNameDic_Hidden_ClipCount_3.Add(startShaderName, shaderName);
        }
        //Debug.LogError("shadername=" + shaderName);
        return shaderName;
    }
    
    public string CreateNoGCShaderName_ShaderNull_Hidden_ClipCount(string shaderName)
    {
        if (string.IsNullOrEmpty(shaderName))
        {
            return string.Empty;
        }
        string retName;
        if (mClipCount == 1)
        {
            if (s_ShaderNameDic_ShaderNull_Hidden_ClipCount_1.TryGetValue(shaderName, out retName))
            {
                //Debug.LogError("shadername=" + retName);
                return retName;
            }
        }
        else if (mClipCount == 2)
        {
            if (s_ShaderNameDic_ShaderNull_Hidden_ClipCount_2.TryGetValue(shaderName, out retName))
            {
                //Debug.LogError("shadername=" + retName);
                return retName;
            }
        }
        else if (mClipCount == 3)
        {
            if (s_ShaderNameDic_ShaderNull_Hidden_ClipCount_3.TryGetValue(shaderName, out retName))
            {
                //Debug.LogError("shadername=" + retName);
                return retName;
            }
        }

        string startShaderName = shaderName;
        shaderName = CalculateString(shaderName);
        shaderName = shaderName + " " + mClipCount;
        if (mClipCount == 1)
        {
            s_ShaderNameDic_ShaderNull_Hidden_ClipCount_1.Add(startShaderName, shaderName);
        }
        else if (mClipCount == 2)
        {
            s_ShaderNameDic_ShaderNull_Hidden_ClipCount_2.Add(startShaderName, shaderName);
        }
        else if (mClipCount == 3)
        {
            s_ShaderNameDic_ShaderNull_Hidden_ClipCount_3.Add(startShaderName, shaderName);
        }
        //Debug.LogError("shadername=" + shaderName);
        return shaderName;
    }

    
    public string CreateNoGCShaderName_TextureMask(string shaderName)
    {
        if (string.IsNullOrEmpty(shaderName))
        {
            return string.Empty;
        }
        string retName;
        if (s_ShaderNameDic_TextureMask.TryGetValue(shaderName, out retName))
        {
            //Debug.LogError("shadername=" + retName);
            return retName;
        }
        string startShaderName = shaderName;

        shaderName = CalculateString(shaderName);
        const string textureClip = " (TextureClip)";
        shaderName = "Hidden/" + shaderName + textureClip;
        s_ShaderNameDic_TextureMask.Add(startShaderName, shaderName);
        //Debug.LogError("shadername=" + shaderName);
        return shaderName;
    }
    
    public string CreateNoGCShaderName_Soft(string shaderName)
    {
        if (string.IsNullOrEmpty(shaderName))
        {
            return string.Empty;
        }
        string retName;
        if (s_ShaderNameDic_Soft.TryGetValue(shaderName, out retName))
        {
            //Debug.LogError("shadername=" + retName);
            return retName;
        }
        string startShaderName = shaderName;

        shaderName = CalculateString(shaderName);
        const string soft = " (SoftClip)";
        shaderName = shaderName + soft;
        s_ShaderNameDic_Soft.Add(startShaderName, shaderName);
        //Debug.LogError("shadername=" + shaderName);
        return shaderName;
    }

    /// <summary>
    /// Create an appropriate material for the draw call.
    /// </summary>

    void CreateMaterial ()
	{
		mTextureClip = false;
		mLegacyShader = false;
		mClipCount = panel.clipCount;

        //modify by manual
        //begin
        //string shaderName = (mShader != null) ? mShader.name :
            //((mMaterial != null) ? mMaterial.shader.name : "Unlit/Transparent Colored");
		string shaderName = (mShader != null) ? mShader.name :
            ((mMaterial != null) ? mMaterial.shader.name : "Unlit/Transparent Masked");
        //end

        //shaderName = CreateNoGCShaderName(shaderName);
        // Figure out the normal shader's name
        // shaderName = shaderName.Replace("GUI/Text Shader", "Unlit/Text");

		//if (shaderName.Length > 2)
		//{
		//	if (shaderName[shaderName.Length - 2] == ' ')
		//	{
		//		int index = shaderName[shaderName.Length - 1];
		//		if (index > '0' && index <= '9') shaderName = shaderName.Substring(0, shaderName.Length - 2);
		//	}
		//}

		//if (shaderName.StartsWith("Hidden/"))
		//	shaderName = shaderName.Substring(7);

		//// Legacy functionality
		const string soft = " (SoftClip)";
		//shaderName = shaderName.Replace(soft, "");

		const string textureClip = " (TextureClip)";
		//shaderName = shaderName.Replace(textureClip, "");

		if (panel.clipping == Clipping.TextureMask)
		{
			mTextureClip = true;
			shader = Shader.Find(CreateNoGCShaderName_TextureMask(shaderName));
		}
		else if (mClipCount != 0)
		{
			shader = Shader.Find(CreateNoGCShaderName_Hidden_ClipCount(shaderName));
			if (shader == null) shader = Shader.Find(CreateNoGCShaderName_ShaderNull_Hidden_ClipCount(shaderName));

			// Legacy functionality
			if (shader == null && mClipCount == 1)
			{
				mLegacyShader = true;
				shader = Shader.Find(CreateNoGCShaderName_Soft(shaderName));
			}
		}
		else 
		{
            shaderName = CalculateString(shaderName);
            //Debug.LogError("shadername=" + shaderName);
#if USE_SHADERFIND_HELPER
            shader = ShaderFindHelper.ShaderFind(shaderName);
#else
            shader = Shader.Find(shaderName);
#endif
        }

		// Always fallback to the default shader
        if (shader == null)
        {
            //modify by manual
            //shader = Shader.Find("Unlit/Transparent Colored");
            //Debug.LogError("shadername=Unlit/Transparent Masked");
            shader = Shader.Find("Unlit/Transparent Masked");
            //end
        }

		if (mMaterial != null)
		{
            if (mDynamicMat == null)
            {
                GetDynamicMat(mMaterial);
                //mDynamicMat = new Material(mMaterial);
            }

            if (mDynamicMat == null)
            {
                return;
            }
            mDynamicMat.name = "[NGUI] "/* + mMaterial.name*/;
            mDynamicMat.CopyPropertiesFromMaterial(mMaterial);
#if !UNITY_FLASH
            //string[] keywords = mMaterial.shaderKeywords;
            //for (int i = 0; i < keywords.Length; ++i)
            //    mDynamicMat.EnableKeyword(keywords[i]);
#endif
            // If there is a valid shader, assign it to the custom material
            if (shader != null)
            {
                mDynamicMat.shader = shader;
            }
            else if (mClipCount != 0)
            {
                Debug.LogError(shaderName + " shader doesn't have a clipped shader version for " + mClipCount + " clip regions");
            }
        }
		else
		{
            if (mDynamicMat == null)
            {
                GetDynamicMat(shader);
                //mDynamicMat = new Material(shader);
            }
            if(null == mDynamicMat)
            {
                return;
            }
            if (shader != null)
            {
                mDynamicMat.shader = shader;
            }
            mDynamicMat.name = "[NGUI] "/* + shader.name*/;
        }
	}

    /// <summary>
    /// Rebuild the draw call's material.
    /// </summary>
    static private Material[] marray = new Material[1];
    Material RebuildMaterial ()
	{
        // Destroy the old material
        //NGUITools.DestroyImmediate(mDynamicMat);
        ClearDynamicMat();
		// Create a new material
		CreateMaterial();
        if (mDynamicMat == null)
        {
            return null;
        }
		mDynamicMat.renderQueue = mRenderQueue;

		// Assign the main texture
		if (mTexture != null) mDynamicMat.mainTexture = mTexture;

        // Update the renderer
        marray[0] = mDynamicMat;
        if (mRenderer != null) mRenderer.sharedMaterials = marray;
        marray[0] = null;
        return mDynamicMat;
	}

	/// <summary>
	/// Update the renderer's materials.
	/// </summary>

	void UpdateMaterials ()
	{
		// If clipping should be used, we need to find a replacement shader
		if (mRebuildMat || mDynamicMat == null || mClipCount != panel.clipCount || mTextureClip != (panel.clipping == Clipping.TextureMask))
		{
			RebuildMaterial();
			mRebuildMat = false;
		}
		else if (mRenderer.sharedMaterial != mDynamicMat)
		{
#if UNITY_EDITOR
			Debug.LogError("Hmm... This point got hit!");
#endif
            marray[0] = mDynamicMat;
            mRenderer.sharedMaterials = marray;
            marray[0] = null;
        }
    }

	/// <summary>
	/// Set the draw call's geometry.
	/// </summary>

	public void UpdateGeometry (int widgetCount)
	{
        //Profiler.BeginSample("UIDrawCall:UpdateGeometry");

		this.widgetCount = widgetCount;
		int count = verts.size;

		// Safety check to ensure we get valid values
		if (count > 0 && (count == uvs.size && count == cols.size) && (count % 4) == 0)
		{
			// Cache all components
			if (mFilter == null) mFilter = gameObject.GetComponent<MeshFilter>();
			if (mFilter == null) mFilter = gameObject.AddComponent<MeshFilter>();

			if (verts.size < 65000)
			{
				// Populate the index buffer
				int indexCount = (count >> 1) * 3;

                // 如果这个drawcall目前mesh里的indexbuffer的有效index个数与这次所要求填充的不一样，才进行重新构建并填充
                bool setIndices = (mIndices == null || mMeshIndexCount != indexCount);

                //if (mIndices != null && mIndices.Length == indexCount && mMeshIndexCount != indexCount)
                //{
                //    string debugStr = this.name + "\t" + this.panel.name + "\t" + verts.size + "\t" + indexCount + "\t" + mMeshIndexCount;

                //    Debug.Log(debugStr);
                //}

                // Create the mesh
                if (mMesh == null)
				{
                    //Profiler.BeginSample("New Mesh");

					mMesh = new Mesh();
					mMesh.hideFlags = HideFlags.DontSave;
					mMesh.name = (mMaterial != null) ? "[NGUI] "/* + mMaterial.name */: "[NGUI] Mesh";
					mMesh.MarkDynamic();
					setIndices = true;

                    //Profiler.EndSample();
				}

				// If the buffer length doesn't match, we need to trim all buffers
				bool trim = (uvs.buffer.Length != verts.buffer.Length) ||
					(cols.buffer.Length != verts.buffer.Length);

				// Non-automatic render queues rely on Z position, so it's a good idea to trim everything
				if (!trim && panel.renderQueue != UIPanel.RenderQueue.Automatic)
					trim = (mMesh == null || mMesh.vertexCount != verts.buffer.Length);

				// NOTE: Apparently there is a bug with Adreno devices:
				// http://www.tasharen.com/forum/index.php?topic=8415.0
#if !UNITY_ANDROID
				// If the number of vertices in the buffer is less than half of the full buffer, trim it
				if (!trim && (verts.size << 1) < verts.buffer.Length) trim = true;
#endif
				mTriangles = (verts.size >> 1);

                //if (trim || verts.buffer.Length > 65000)
                //{
                //    if (trim || mMesh.vertexCount != verts.size)
                //    {
                //        mMesh.Clear();
                //        setIndices = true;
                //    }

                //    mMesh.vertices = verts.CloneToArray();
                //    mMesh.uv = uvs.CloneToArray();
                //    mMesh.colors32 = cols.CloneToArray();

                //    if (norms != null) mMesh.normals = norms.CloneToArray();
                //    if (tans != null) mMesh.tangents = tans.CloneToArray();
                //}
                //else
				{
					if (mMesh.vertexCount != verts.buffer.Length)
					{
						mMesh.Clear();
						setIndices = true;
					}

                    //Profiler.BeginSample("MeshSetContent");

                    mMesh.vertices = verts.buffer;
                    mMesh.uv = uvs.buffer;
                    mMesh.colors32 = cols.buffer;

                    //Profiler.EndSample();
				}

				if (setIndices)
				{
                    //Profiler.BeginSample("GenerateCachedIndexBuffer");

                    mIndices = GenerateCachedIndexBuffer(count, indexCount);
                    //Profiler.EndSample();

                    //Profiler.BeginSample("mMesh.triangles = mIndices");
                    mMesh.triangles = mIndices;
                    //Profiler.EndSample();
                }

#if !UNITY_FLASH
                if (trim || !alwaysOnScreen)
#endif
					mMesh.RecalculateBounds();

				mFilter.mesh = mMesh;
			}
			else
			{
				mTriangles = 0;
				if (mFilter.mesh != null) mFilter.mesh.Clear();
				Debug.LogError("Too many vertices on one panel: " + verts.size);
			}

			if (mRenderer == null) mRenderer = gameObject.GetComponent<MeshRenderer>();

			if (mRenderer == null)
			{
				mRenderer = gameObject.AddComponent<MeshRenderer>();
#if UNITY_EDITOR
				mRenderer.enabled = isActive;
#endif
			}
            //Profiler.BeginSample("UpdateMaterials");

            UpdateMaterials();

            //Profiler.EndSample();

        }
        else
		{
            if (mFilter != null && mFilter.mesh != null) mFilter.mesh.Clear();
			//Debug.LogError("UIWidgets must fill the buffer with 4 vertices per quad. Found " + count);
		}

#if (OPTIMISE_GC_SY_20190426)
        verts.Clear();
        uvs.Clear();
        cols.Clear();
#else
        verts.size = 0;
        uvs.size = 0;
        cols.size = 0;
#endif
        //if (norms != null)
        //    norms.Clear();
        //if (tans != null)
        //    tans.Clear();

        //Profiler.EndSample();
    }

	private const int maxIndexBufferCache = 70;

#if UNITY_FLASH
	List<int[]> mCache = new List<int[]>(maxIndexBufferCache);
#else
	static List<int[]> mCache = new List<int[]>(maxIndexBufferCache);
#endif

    private void FillIndexBuffer(int[] indexBuffer, int vertexCount, int realIndexCount, int indexCount)
    {
        // 填充index buffer模版
        if (null == mIndexTemplate)
        {
            mIndexTemplate = new int[mMaxTemplateIndexCount];

            int i = 0;

            for (int v = 0; v < mMaxTemplateVertexCount; v += 4)
            {
                mIndexTemplate[i++] = v;
                mIndexTemplate[i++] = v + 1;
                mIndexTemplate[i++] = v + 2;

                mIndexTemplate[i++] = v + 2;
                mIndexTemplate[i++] = v + 3;
                mIndexTemplate[i++] = v;
            }
        }

        // 如果这次要填充的indexCount大于模版indexbuffer的最大数量，或者小于150，就按照正常流程来进行填充，在红米note上的il2cpp上测试过，如果
        // 本次要填充的数据过小，调用Array.Copy或Array.Clear的效率会更低
        if (indexCount < 150 || indexCount > mMaxTemplateIndexCount)
        {
            int index = 0;
            // 正常填充所需的三角形的index
            for (int vertexI = 0; vertexI < vertexCount; vertexI += 4)
            {
                indexBuffer[index++] = vertexI;
                indexBuffer[index++] = vertexI + 1;
                indexBuffer[index++] = vertexI + 2;

                indexBuffer[index++] = vertexI + 2;
                indexBuffer[index++] = vertexI + 3;
                indexBuffer[index++] = vertexI;
            }
        }
        else
        {
            System.Array.Copy(mIndexTemplate, indexBuffer, indexCount);            
        }

        // 把index buffer中剩下的空位都清空
        int indexDelta = realIndexCount - indexCount;
        if (indexDelta > 0)
        {
            if (indexDelta < 150)
            {
                // 把剩下的都填充为0，变成退化的三角形
                for (int indexI = indexCount; indexI < realIndexCount; ++indexI)
                    indexBuffer[indexI] = sIndexFeedNum;
            }
            else
            {
                System.Array.Clear(indexBuffer, indexCount, indexDelta);
            }
        }

        mMeshIndexCount = indexCount;
    }
    /// <summary>
    /// Generates a new index buffer for the specified number of vertices (or reuses an existing one).
    /// 之前的实现是mCache中保存一个固定数组的index buffer缓冲，每个index buffer里的index数量是严格按照这个drawcall所需的三角形
    /// 数量来得出，在实际运行时由于drawcall较多，而且每个的index个数都不一样，所以maxIndexBufferCache很容易被超出，造成频繁进行
    /// new index buffer以及对mCache进行add和remove的操作，所以修改策略为
    /// 1：每次index buffer分配的大小是固定的，由indexNums数组给出；
    /// 2：每次drawcall要申请新的index buffer时，从mCache中找出符合条件的一个（但index buffer数量不一定和当前drawcall所需的三角形数量一致）
    /// 3：超出的index统一填充为0，变成一个退化成一个点的三角形，渲染上不会有问题，会多出一点点渲染开销，但是避免了频繁的申请内存，特别是对于名字板这种大小经常改变的panel来说
    /// </summary>

    // 必须是6的倍数，因为一个矩形需要6个index，这个数组的大小不能超过maxIndexBufferCache，不然还是会造成运行时内存分配，从目前来看，运行时mCache所缓存的大小不会超过0.5MB的内存
    static int[] sIndexNums =
        { 6, 12, 18, 24, 30, 36, 42, 48, 54, 60,
        78, 90, 108, 120, 144, 180, 210, 240, 270, 300,
        330, 360, 390, 420, 450, 480, 510, 540, 600, 630,
        660, 750, 840, 900, 990, 1104, 1320, 1500, 1740, 1806,
        1920, 2022, 2250, 2700, 2940, 3138, 3300, 3600, 3900, 4248,
        4500, 4800, 5100, 5400, 6000, 6300, 6600, 6900, 7200, 7500,
        7800, 8400, 9000, 9600, 10200, 12000, 14400, 18000, 21000, 24000};
    static int sIndexFeedNum = 0;

	int[] GenerateCachedIndexBuffer (int vertexCount, int indexCount)
	{
        int indexNumCount = sIndexNums.Length;

        int realIndexCount = indexCount;

        // 找出最接近的index buffer大小，如果没找到，也就是所需的index buffer超过了indexNums中最大的数字了，或者说所需大小在缓存中未找到，属于第一次分配，就用原大小
        for (int indexNumI = 0; indexNumI < indexNumCount; ++indexNumI)
        {
            if (indexCount <= sIndexNums[indexNumI])
            {
                realIndexCount = sIndexNums[indexNumI];
//#if UNITY_EDITOR
//                System.IO.StreamWriter saveFile1 = new System.IO.StreamWriter("GenerateCachedIndexBuffer_cache.txt", true);

//                string debugStr = this.name + "\t" + this.panel.name + "\t" + vertexCount + "\t" + indexCount + "\t" + realIndexCount + "\t" + mMeshIndexCount;
//                saveFile1.WriteLine(debugStr);

//                //Debug.Log("NGUI::GenerateCachedIndexBuffer cache index buffer count : " + debugStr);
//                saveFile1.Close();

//#endif
                break;
            }
        }

        //int index = 0;

        int cacheCount = mCache.Count;

        for (int i = 0, imax = cacheCount; i < imax; ++i)
		{
			int[] ids = mCache[i];
			if (ids != null)
            {
                // 在cache中找到一样大小的缓冲，可以直接拿出来用，但里面的数据需要根据需要填充
                if (realIndexCount == ids.Length)
                {
                    FillIndexBuffer(ids, vertexCount, realIndexCount, indexCount);

                    return ids;
                }
            }
        }       

        // 能到达这里，就是这个所需的大小所进行的第一次分配，正常分配内存
		int[] rv = new int[realIndexCount];
//#if UNITY_EDITOR
//        System.IO.StreamWriter saveFile = new System.IO.StreamWriter("GenerateCachedIndexBuffer_new.txt", true);

//        string debugStr1 = this.name + "\t" + this.panel.name + "\t" + vertexCount + "\t" + indexCount + "\t" + realIndexCount + "\t" + mMeshIndexCount;

//        saveFile.WriteLine(debugStr1);

//        //Debug.Log("NGUI::GenerateCachedIndexBuffer new index buffer count : " + debugStr1);
//        saveFile.Close();

//#endif

        FillIndexBuffer(rv, vertexCount, realIndexCount, indexCount);

        if (cacheCount > maxIndexBufferCache) mCache.RemoveAt(0);
		    mCache.Add(rv);

		return rv;
	}

	/// <summary>
	/// This function is called when it's clear that the object will be rendered.
	/// We want to set the shader used by the material, creating a copy of the material in the process.
	/// We also want to update the material's properties before it's actually used.
	/// </summary>

	void OnWillRenderObject ()
	{
		UpdateMaterials();

		if (onRender != null) onRender(mDynamicMat ?? mMaterial);
		if (mDynamicMat == null || mClipCount == 0) return;

		if (mTextureClip)
		{
			Vector4 cr = panel.drawCallClipRange;
			Vector2 soft = panel.clipSoftness;

			Vector2 sharpness = new Vector2(1000.0f, 1000.0f);
			if (soft.x > 0f) sharpness.x = cr.z / soft.x;
			if (soft.y > 0f) sharpness.y = cr.w / soft.y;

			mDynamicMat.SetVector(ClipRange[0], new Vector4(-cr.x / cr.z, -cr.y / cr.w, 1f / cr.z, 1f / cr.w));
			mDynamicMat.SetTexture("_ClipTex", clipTexture);

            UIPanel currentPanel = panel;

            for (int i = 0; currentPanel != null;)
            {
                if (currentPanel.hasClipping)
                {
                    float angle = 0f;
                     cr = currentPanel.drawCallClipRange;

                    // Clipping regions past the first one need additional math
                    if (currentPanel != panel)
                    {
                        Vector3 pos = currentPanel.cachedTransform.InverseTransformPoint(panel.cachedTransform.position);
                        cr.x -= pos.x;
                        cr.y -= pos.y;

                        Vector3 v0 = panel.cachedTransform.rotation.eulerAngles;
                        Vector3 v1 = currentPanel.cachedTransform.rotation.eulerAngles;
                        Vector3 diff = v1 - v0;

                        diff.x = NGUIMath.WrapAngle(diff.x);
                        diff.y = NGUIMath.WrapAngle(diff.y);
                        diff.z = NGUIMath.WrapAngle(diff.z);

                        if (Mathf.Abs(diff.x) > 0.001f || Mathf.Abs(diff.y) > 0.001f)
                            Debug.LogWarning("Panel can only be clipped properly if X and Y rotation is left at 0", panel);

                        angle = diff.z;
                    }

                    // Pass the clipping parameters to the shader
                    SetClipping(1, cr, currentPanel.clipSoftness, angle);
                }
                currentPanel = currentPanel.parentPanel;
            }
        }
		else if (!mLegacyShader)
		{
			UIPanel currentPanel = panel;

			for (int i = 0; currentPanel != null; )
			{
				if (currentPanel.hasClipping)
				{
					float angle = 0f;
					Vector4 cr = currentPanel.drawCallClipRange;

					// Clipping regions past the first one need additional math
					if (currentPanel != panel)
					{
						Vector3 pos = currentPanel.cachedTransform.InverseTransformPoint(panel.cachedTransform.position);
						cr.x -= pos.x;
						cr.y -= pos.y;

						Vector3 v0 = panel.cachedTransform.rotation.eulerAngles;
						Vector3 v1 = currentPanel.cachedTransform.rotation.eulerAngles;
						Vector3 diff = v1 - v0;

						diff.x = NGUIMath.WrapAngle(diff.x);
						diff.y = NGUIMath.WrapAngle(diff.y);
						diff.z = NGUIMath.WrapAngle(diff.z);

						if (Mathf.Abs(diff.x) > 0.001f || Mathf.Abs(diff.y) > 0.001f)
							Debug.LogWarning("Panel can only be clipped properly if X and Y rotation is left at 0", panel);

						angle = diff.z;
					}

					// Pass the clipping parameters to the shader
					SetClipping(i++, cr, currentPanel.clipSoftness, angle);
				}
				currentPanel = currentPanel.parentPanel;
			}
		}
		else // Legacy functionality
		{
			Vector2 soft = panel.clipSoftness;
			Vector4 cr = panel.drawCallClipRange;
			Vector2 v0 = new Vector2(-cr.x / cr.z, -cr.y / cr.w);
			Vector2 v1 = new Vector2(1f / cr.z, 1f / cr.w);

			Vector2 sharpness = new Vector2(1000.0f, 1000.0f);
			if (soft.x > 0f) sharpness.x = cr.z / soft.x;
			if (soft.y > 0f) sharpness.y = cr.w / soft.y;

			mDynamicMat.mainTextureOffset = v0;
			mDynamicMat.mainTextureScale = v1;
			mDynamicMat.SetVector("_ClipSharpness", sharpness);
		}
	}

	static int[] ClipRange = null;
	static int[] ClipArgs = null;

	/// <summary>
	/// Set the shader clipping parameters.
	/// </summary>

	void SetClipping (int index, Vector4 cr, Vector2 soft, float angle)
	{
		angle *= -Mathf.Deg2Rad;

		Vector2 sharpness = new Vector2(1000.0f, 1000.0f);
		if (soft.x > 0f) sharpness.x = cr.z / soft.x;
		if (soft.y > 0f) sharpness.y = cr.w / soft.y;

		if (index < ClipRange.Length)
		{
			mDynamicMat.SetVector(ClipRange[index], new Vector4(-cr.x / cr.z, -cr.y / cr.w, 1f / cr.z, 1f / cr.w));
			mDynamicMat.SetVector(ClipArgs[index], new Vector4(sharpness.x, sharpness.y, Mathf.Sin(angle), Mathf.Cos(angle)));
		}
	}

	/// <summary>
	/// Cache the property IDs.
	/// </summary>

	void Awake ()
	{
		if (ClipRange == null)
		{
			ClipRange = new int[]
			{
				Shader.PropertyToID("_ClipRange0"),
				Shader.PropertyToID("_ClipRange1"),
				Shader.PropertyToID("_ClipRange2"),
				Shader.PropertyToID("_ClipRange4"),
			};
		}

		if (ClipArgs == null)
		{
			ClipArgs = new int[]
			{
				Shader.PropertyToID("_ClipArgs0"),
				Shader.PropertyToID("_ClipArgs1"),
				Shader.PropertyToID("_ClipArgs2"),
				Shader.PropertyToID("_ClipArgs3"),
			};
		}
	}

	/// <summary>
	/// The material should be rebuilt when the draw call is enabled.
	/// </summary>

	void OnEnable () { mRebuildMat = true; }

    /// <summary>
    /// Clear all references.
    /// </summary>
    /// 
    
    public static Queue<Material> DynamicMaterial_Pool = new Queue<Material>(40);
    static private Material[] s_EmptyMaterialArray = new Material[] { };

    private void GetDynamicMat(Material mt)
    {
        if(DynamicMaterial_Pool.Count >0)
        {
            mDynamicMat = DynamicMaterial_Pool.Dequeue();
        }
        else
        {
            mDynamicMat = new Material(mt);
        }
    }

    private void GetDynamicMat(Shader sh)
    {
        if (DynamicMaterial_Pool.Count > 0)
        {
            mDynamicMat = DynamicMaterial_Pool.Dequeue();
        }
        else
        {
            mDynamicMat = new Material(sh);
        }
    }

    private void ClearDynamicMat()
    {
        if (mDynamicMat != null)
        {
            mDynamicMat.mainTexture = null;
            mDynamicMat.name = "nothing";
            mDynamicMat.renderQueue = 2000;
            mDynamicMat.shader = null;
            if(DynamicMaterial_Pool.Count <30)
            {
                DynamicMaterial_Pool.Enqueue(mDynamicMat);
            }
            else
            {
                NGUITools.DestroyImmediate(mDynamicMat);
            }
            mDynamicMat = null;
        }
    }

    void OnDisable ()
	{
		depthStart = int.MaxValue;
		depthEnd = int.MinValue;
		panel = null;
		manager = null;
		mMaterial = null;
		mTexture = null;
		clipTexture = null;

		if (mRenderer != null)
        {
            //2017-10-08 sunyu 没必要每次new
            mRenderer.sharedMaterials = s_EmptyMaterialArray;
        }
        ClearDynamicMat();

        //      NGUITools.DestroyImmediate(mDynamicMat);
        //mDynamicMat = null;
    }

    /// <summary>
    /// Cleanup.
    /// </summary>

    void OnDestroy ()
	{
        mIndices = null;
        verts.Clear();
        uvs.Clear();
        cols.Clear();

        NGUITools.DestroyImmediate(mMesh);
		mMesh = null;
        mIsEmpty = false;
    }

    
    public int TotalVertsNum()
    {
        return verts.bufferLength;
    }

	/// <summary>
	/// Return an existing draw call.
	/// </summary>

	static public UIDrawCall Create (UIPanel panel, Material mat, Texture tex, Shader shader)
	{
#if UNITY_EDITOR
		string name = null;
		if (tex != null) name = tex.name;
		else if (shader != null) name = shader.name;
		else if (mat != null) name = mat.name;
		return Create(name, panel, mat, tex, shader);
#else
		return Create(null, panel, mat, tex, shader);
#endif
	}

	/// <summary>
	/// Create a new draw call, reusing an old one if possible.
	/// </summary>

	static UIDrawCall Create (string name, UIPanel pan, Material mat, Texture tex, Shader shader)
	{
		UIDrawCall dc = Create(name);
		dc.gameObject.layer = pan.cachedGameObject.layer;
		dc.baseMaterial = mat;
		dc.mainTexture = tex;
		dc.shader = shader;
		dc.renderQueue = pan.startingRenderQueue;
		dc.sortingOrder = pan.sortingOrder;
		dc.manager = pan;
		return dc;
	}

	/// <summary>
	/// Create a new draw call, reusing an old one if possible.
	/// </summary>

	static UIDrawCall Create (string name)
	{
#if SHOW_HIDDEN_OBJECTS && UNITY_EDITOR
		name = (name != null) ? "_UIDrawCall [" + name + "]" : "DrawCall";
#endif
		if (mInactiveList.size > 0)
		{
			UIDrawCall dc = mInactiveList.Pop();
            dc.mIsEmpty = false;
			mActiveList.Add(dc);
			if (name != null) dc.name = name;
			NGUITools.SetActive(dc.gameObject, true);
			return dc;
		}

#if UNITY_EDITOR
		// If we're in the editor, create the game object with hide flags set right away
		GameObject go = UnityEditor.EditorUtility.CreateGameObjectWithHideFlags(name,
#if SHOW_HIDDEN_OBJECTS
			HideFlags.DontSave, typeof(UIDrawCall));
#else
			HideFlags.HideAndDontSave, typeof(UIDrawCall));
#endif
		UIDrawCall newDC = go.GetComponent<UIDrawCall>();
#else
		GameObject go = new GameObject(name);
		DontDestroyOnLoad(go);
		UIDrawCall newDC = go.AddComponent<UIDrawCall>();
#endif
        //newDC.verts.Allocate(2048);
        //newDC.uvs.Allocate(2048);
        //newDC.cols.Allocate(2048);

		// Create the draw call
		mActiveList.Add(newDC);
		return newDC;
	}

	/// <summary>
	/// Clear all draw calls.
	/// </summary>

	static public void ClearAll ()
	{
		bool playing = Application.isPlaying;

		for (int i = mActiveList.size; i > 0; )
		{
			UIDrawCall dc = mActiveList[--i];

			if (dc)
			{
				if (playing) NGUITools.SetActive(dc.gameObject, false);
				else NGUITools.DestroyImmediate(dc.gameObject);
			}
		}
		mActiveList.Clear();
	}

	/// <summary>
	/// Immediately destroy all draw calls.
	/// </summary>

	static public void ReleaseAll ()
	{
		ClearAll();
		ReleaseInactive();
	}

	/// <summary>
	/// Immediately destroy all inactive draw calls (draw calls that have been recycled and are waiting to be re-used).
    /// 此函数只能切场景时调用一次，不能频繁调用
	/// </summary>

	static public void ReleaseInactive()
	{
		for (int i = mInactiveList.size; i > 0; )
		{
			UIDrawCall dc = mInactiveList[--i];
			if (dc) NGUITools.DestroyImmediate(dc.gameObject);
		}
		mInactiveList.Clear();
	}

	/// <summary>
	/// Count all draw calls managed by the specified panel.
	/// </summary>

	static public int Count (UIPanel panel)
	{
		int count = 0;
		for (int i = 0; i < mActiveList.size; ++i)
			if (mActiveList[i].manager == panel) ++count;
		return count;
	}

	/// <summary>
	/// Destroy the specified draw call.
	/// </summary>

	static public void Destroy (UIDrawCall dc)
	{
		if (dc)
		{
			dc.onRender = null;

			if (Application.isPlaying)
			{
				if (mActiveList.Remove(dc))
				{
                    if (mInactiveList.size < 40)
                    {
                        NGUITools.SetActive(dc.gameObject, false);
                        mInactiveList.Add(dc);
                    }
                    else
                    {
                        NGUITools.DestroyImmediate(dc.gameObject);
                    }
				}
			}
			else
			{
				mActiveList.Remove(dc);
				NGUITools.DestroyImmediate(dc.gameObject);
			}
		}
	}
}
