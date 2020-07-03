//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2015 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TweenGradientColor))]
public class TweenGradientColorEditor : UITweenerEditor
{
	public override void OnInspectorGUI ()
	{
		GUILayout.Space(6f);
		NGUIEditorTools.SetLabelWidth(120f);

		TweenGradientColor tw = target as TweenGradientColor;
		GUI.changed = false;

		Color topFrom = EditorGUILayout.ColorField("TopFrom", tw.topFrom);
		Color topTo = EditorGUILayout.ColorField("TopTo", tw.topTo);

        Color bottomFrom = EditorGUILayout.ColorField("BottomFrom", tw.bottomFrom);
        Color bottomTo = EditorGUILayout.ColorField("BottomTo", tw.bottomTo);

        if (GUI.changed) 
		{
			NGUIEditorTools.RegisterUndo("Tween Change", tw);
			tw.topFrom = topFrom;
			tw.topTo = topTo;

            tw.bottomFrom = bottomFrom;
            tw.bottomTo = bottomTo;
            NGUITools.SetDirty(tw);
		}

		DrawCommonProperties();
	}
}
