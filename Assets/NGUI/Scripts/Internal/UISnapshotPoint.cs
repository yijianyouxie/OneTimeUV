//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2015 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("NGUI/Internal/Snapshot Point")]
public class UISnapshotPoint : CacheBehaviour
{
	public bool isOrthographic = true;
	public float nearClip = -100f;
	public float farClip = 100f;

	[Range(10, 80)]
	public int fieldOfView = 35;
	public float orthoSize = 30f;

	public Texture2D thumbnail;

	void Start () { if (tag != "EditorOnly") tag = "EditorOnly"; }
}
