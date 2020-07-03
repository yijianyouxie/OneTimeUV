//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2015 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Generated geometry class. All widgets have one.
/// This class separates the geometry creation into several steps, making it possible to perform
/// actions selectively depending on what has changed. For example, the widget doesn't need to be
/// rebuilt unless something actually changes, so its geometry can be cached. Likewise, the widget's
/// transformed coordinates only change if the widget's transform moves relative to the panel,
/// so that can be cached as well. In the end, using this class means using more memory, but at
/// the same time it allows for significant performance gains, especially when using widgets that
/// spit out a lot of vertices, such as UILabels.
/// </summary>

public class UIGeometry
{
    /// <summary>
    /// Widget's vertices (before they get transformed).
    /// </summary>
    public BetterList<Vector3> verts = new BetterList<Vector3>();
    public UIWidget m_pUIWidget = null;
    public int m_vertexnum = 0;
    /// <summary>
    /// Widget's texture coordinates for the geometry's vertices.
    /// </summary>

    public BetterList<Vector2> uvs = new BetterList<Vector2>();

	/// <summary>
	/// Array of colors for the geometry's vertices.
	/// </summary>

	public BetterList<Color32> cols = new BetterList<Color32>();

	/// <summary>
	/// Custom delegate called after WriteToBuffers finishes filling in the geometry.
	/// Use it to apply any and all modifications to vertices that you need.
	/// </summary>

	public OnCustomWrite onCustomWrite;
	public delegate void OnCustomWrite (BetterList<Vector3> v, BetterList<Vector2> u, BetterList<Color32> c);

    // Relative-to-panel vertices, normal, and tangent
    // 为了访问效率高，把变量变成public

#if (OPTIMISE_GC_SY_20190426)
    public static BetterList<Vector3> sRtpVerts = new BetterList<Vector3>();
#else
    public BetterList<Vector3> mRtpVerts = new BetterList<Vector3>();
#endif
    public Matrix4x4 mRtMatrix = Matrix4x4.identity;

    /// <summary>
    /// Whether the geometry contains usable vertices.
    /// </summary>
#if (OPTIMISE_GC_SY_20190426)
    public bool hasVertices { get { return (m_vertexnum>0)? true:(verts.size > 0); } }
#else
    public bool hasVertices { get { return (verts.size > 0); } }
#endif
    /// <summary>
    /// Whether the geometry has usable transformed vertex data.
    /// </summary>

#if (OPTIMISE_GC_SY_20190426)
    public bool hasTransformed { get { return mRtMatrix != Matrix4x4.identity; } }
#else
    public bool hasTransformed { get { return (mRtpVerts != null) && (mRtpVerts.size > 0) && (mRtpVerts.size == verts.size); } }
#endif

    /// <summary>
    /// Step 1: Prepare to fill the buffers -- make them clean and valid.
    /// </summary>

    public void Clear ()
	{
#if (OPTIMISE_GC_SY_20190426)
        ForceLocal();
#endif
        verts.Clear();
		uvs.Clear();
		cols.Clear();
#if (OPTIMISE_GC_SY_20190426)
        mRtMatrix = Matrix4x4.identity;
#else
        mRtpVerts.Clear();
#endif

    }

#if (OPTIMISE_GC_SY_20190426)
    public void ForceLocal()
    {
        m_vertexnum = 0;
    }

#endif


    /// <summary>
    /// Step 2: Transform the vertices by the provided matrix.
    /// </summary>

    public void ApplyTransform (ref Matrix4x4 widgetToPanel)
	{
#if (OPTIMISE_GC_SY_20190426)
        int vertexCount = (m_vertexnum>0)?m_vertexnum:verts.size;
#else
        int vertexCount = verts.size;
#endif

        if (vertexCount > 0)
		{
#if (OPTIMISE_GC_SY_20190426)
            mRtMatrix = widgetToPanel;
#else
            mRtpVerts.size = 0;
            mRtpVerts.Allocate(vertexCount);
            var mRtpVertsbuffer = mRtpVerts.buffer;
            float tempX, tempY, tempZ;

            for (int i = 0; i < vertexCount; ++i)
            {
                tempX = verts[i].x;
                tempY = verts[i].y;
                tempZ = verts[i].z;
                ++mRtpVerts.size;
                // 不调用MultiplyVector，而是直接做矩阵运算，能减少函数调用开销，以及去掉不必要的z坐标计算
                mRtpVertsbuffer[i].x = widgetToPanel.m00 * tempX + widgetToPanel.m01 * tempY + widgetToPanel.m02 * tempZ + widgetToPanel.m03;
                mRtpVertsbuffer[i].y = widgetToPanel.m10 * tempX + widgetToPanel.m11 * tempY + widgetToPanel.m12 * tempZ + widgetToPanel.m13;
                mRtpVertsbuffer[i].z = widgetToPanel.m20 * tempX + widgetToPanel.m21 * tempY + widgetToPanel.m22 * tempZ + widgetToPanel.m23;
                //      mRtpVerts.Add(widgetToPanel.MultiplyPoint3x4(verts[i]));
            }
#endif
        }
		else
        {
            //mRtpVerts.Clear();
#if (OPTIMISE_GC_SY_20190426)
            mRtMatrix = Matrix4x4.identity;
#else
            mRtpVerts.size = 0;
#endif

        }
    }

    static private Vector3[] tmpVerts = new Vector3[4];

    static private BetterList<Vector3> tmpV3 = new BetterList<Vector3>();
    static private BetterList<Vector2> tmpV2 = new BetterList<Vector2>();
    static private BetterList<Color32> tmpC3 = new BetterList<Color32>();
    static private BetterList<Vector3> tmpTransedV3 = new BetterList<Vector3>();

#if OPTIMISE_GC_SY_20190426
    private void WriteNormalBuffers(BetterList<Vector3> v, BetterList<Vector2> u, BetterList<Color32> c)
    {
        if (sRtpVerts != null)
        {
            int vertexCount = verts.size;
            sRtpVerts.Clear();
            sRtpVerts.Allocate(vertexCount);
            var sRtpVertsbuffer = sRtpVerts.buffer;
            float tempX, tempY, tempZ;

            BetterList<Vector3> localverts = verts;
            for (int i = 0; i < vertexCount; ++i)
            {
                tempX = localverts[i].x;
                tempY = localverts[i].y;
                tempZ = localverts[i].z;
                ++sRtpVerts.size;
                // 不调用MultiplyVector，而是直接做矩阵运算，能减少函数调用开销，以及去掉不必要的z坐标计算
                sRtpVertsbuffer[i].x = mRtMatrix.m00 * tempX + mRtMatrix.m01 * tempY + mRtMatrix.m02 * tempZ + mRtMatrix.m03;
                sRtpVertsbuffer[i].y = mRtMatrix.m10 * tempX + mRtMatrix.m11 * tempY + mRtMatrix.m12 * tempZ + mRtMatrix.m13;
                sRtpVertsbuffer[i].z = mRtMatrix.m20 * tempX + mRtMatrix.m21 * tempY + mRtMatrix.m22 * tempZ + mRtMatrix.m23;
            }

            var sRtpVertssize = sRtpVerts.size;
            v.Allocate(v.size + sRtpVertssize);
            u.Allocate(u.size + sRtpVertssize);
            c.Allocate(c.size + sRtpVertssize);

            var vBuffer = sRtpVerts.buffer;
            var uvsBuffer = uvs.buffer;
            var colsBuffer = cols.buffer;
            for (int i = 0; i < sRtpVertssize; ++i)
            {
                v.buffer[v.size++] = vBuffer[i];
                u.buffer[u.size++] = uvsBuffer[i];
                c.buffer[c.size++] = colsBuffer[i];
            }
            if (onCustomWrite != null) onCustomWrite(v, u, c);
            sRtpVerts.Clear();
        }

    }
#endif

    /// <summary>
    /// Step 3: Fill the specified buffer using the transformed values.
    /// </summary>
    public void WriteToBuffers (BetterList<Vector3> v, BetterList<Vector2> u, BetterList<Color32> c)
	{
#if (OPTIMISE_GC_SY_20190426)

        if (true)
        {
            if (m_pUIWidget != null)
            {
                if(m_vertexnum>0)
                {
                    tmpV3.Clear();
                    tmpV2.Clear();
                    tmpC3.Clear();

                    bool bNormalVertexBuffer = false;
                    if (m_pUIWidget.GetType() == typeof(UILabel))
                    {
                        UILabel ulb = m_pUIWidget as UILabel;
                        ulb.WriteDrawCallBuffer(tmpV3, tmpV2, tmpC3);

                    }
                    else
                    {
                        UIBasicSprite ubs = m_pUIWidget as UIBasicSprite;
                        if(ubs.type == UIBasicSprite.Type.Simple)
                        {
                            ubs.RealSimpleFill(tmpV3, tmpV2, tmpC3);
                        }
                        else if(ubs.type == UIBasicSprite.Type.Sliced)
                        {
                            ubs.RealSlicedFill(tmpV3, tmpV2, tmpC3);
                        }
                        else if (ubs.type == UIBasicSprite.Type.Filled)
                        {
                            ubs.RealFilledFill(tmpV3, tmpV2, tmpC3);
                        }
                        else
                        {
                            bNormalVertexBuffer = true;
                            WriteNormalBuffers(v, u, c);
                        }
                    }

                    if(bNormalVertexBuffer == false)
                    {
                        int vertexCount = tmpV3.size;
                        tmpTransedV3.Clear();
                        tmpTransedV3.Allocate(vertexCount);
                        var tmpTransedV3buffer = tmpTransedV3.buffer;
                        float tempX, tempY, tempZ;

                        for (int i = 0; i < vertexCount; ++i)
                        {
                            tempX = tmpV3[i].x;
                            tempY = tmpV3[i].y;
                            tempZ = tmpV3[i].z;
                            ++tmpTransedV3.size;
                            // 不调用MultiplyVector，而是直接做矩阵运算，能减少函数调用开销，以及去掉不必要的z坐标计算
                            tmpTransedV3buffer[i].x = mRtMatrix.m00 * tempX + mRtMatrix.m01 * tempY + mRtMatrix.m02 * tempZ + mRtMatrix.m03;
                            tmpTransedV3buffer[i].y = mRtMatrix.m10 * tempX + mRtMatrix.m11 * tempY + mRtMatrix.m12 * tempZ + mRtMatrix.m13;
                            tmpTransedV3buffer[i].z = mRtMatrix.m20 * tempX + mRtMatrix.m21 * tempY + mRtMatrix.m22 * tempZ + mRtMatrix.m23;
                        }

                        v.Allocate(v.size + tmpTransedV3.size);
                        u.Allocate(u.size + tmpTransedV3.size);
                        c.Allocate(c.size + tmpTransedV3.size);

                        var vBuffer = tmpTransedV3.buffer;
                        var uvsBuffer = tmpV2.buffer;
                        var colsBuffer = tmpC3.buffer;

                        for (int i = 0; i < tmpTransedV3.size; ++i)
                        {
                            v.buffer[v.size++] = vBuffer[i];
                            u.buffer[u.size++] = uvsBuffer[i];
                            c.buffer[c.size++] = colsBuffer[i];
                        }

                        tmpV3.Clear();
                        tmpV2.Clear();
                        tmpC3.Clear();
                        tmpTransedV3.Clear();

                        if (onCustomWrite != null) onCustomWrite(v, u, c);
                    }
                }
                else
                {
                    WriteNormalBuffers(v, u, c);
                }
            }
            else
            {
                WriteNormalBuffers(v, u, c);
            }
        }
#else
        if (mRtpVerts != null && mRtpVerts.size > 0)
        {
            var mRtpVertssize = mRtpVerts.size;
            v.Allocate(v.size + mRtpVertssize);
            u.Allocate(u.size + mRtpVertssize);
            c.Allocate(c.size + mRtpVertssize);

            var vBuffer = mRtpVerts.buffer;
            var uvsBuffer = uvs.buffer;
            var colsBuffer = cols.buffer;

            for (int i = 0; i < mRtpVertssize; ++i)
            {
                v.buffer[v.size++] = vBuffer[i];
                u.buffer[u.size++] = uvsBuffer[i];
                c.buffer[c.size++] = colsBuffer[i];
            }
            if (onCustomWrite != null) onCustomWrite(v, u, c);
        }
#endif
    }
}
