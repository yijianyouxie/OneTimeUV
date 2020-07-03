//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2015 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;

/// <summary>
/// Makes it possible to animate a color of the widget.
/// </summary>

[ExecuteInEditMode]
[RequireComponent(typeof(UIWidget))]
public class AnimatedColor : CacheBehaviour
{
	public Color color = Color.white;
	
	UIWidget mWidget;

	void OnEnable () { mWidget = GetComponent<UIWidget>(); LateUpdate(); }
	void LateUpdate () { mWidget.color = color; }
}
