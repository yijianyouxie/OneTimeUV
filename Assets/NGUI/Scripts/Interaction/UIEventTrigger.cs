//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2015 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Attaching this script to an object will let you trigger remote functions using NGUI events.
/// </summary>
public class UIEventTriggerFlag
{
    static public bool bUsing = false;
}
[AddComponentMenu("NGUI/Interaction/Event Trigger")]
public class UIEventTrigger : MonoBehaviour
{
	//static public UIEventTrigger current;

	public List<EventDelegate> onHoverOver = new List<EventDelegate>();
	public List<EventDelegate> onHoverOut = new List<EventDelegate>();
	public List<EventDelegate> onPress = new List<EventDelegate>();
    public List<EventDelegate> onPressure = new List<EventDelegate>();
	public List<EventDelegate> onRelease = new List<EventDelegate>();
	public List<EventDelegate> onSelect = new List<EventDelegate>();
	public List<EventDelegate> onDeselect = new List<EventDelegate>();
	public List<EventDelegate> onClick = new List<EventDelegate>();
	public List<EventDelegate> onDoubleClick = new List<EventDelegate>();
	public List<EventDelegate> onDragStart = new List<EventDelegate>();
	public List<EventDelegate> onDragEnd = new List<EventDelegate>();
	public List<EventDelegate> onDragOver = new List<EventDelegate>();
	public List<EventDelegate> onDragOut = new List<EventDelegate>();
	public List<EventDelegate> onDrag = new List<EventDelegate>();

	/// <summary>
	/// Whether the collider is enabled and the widget can be interacted with.
	/// </summary>

	public bool isColliderEnabled
	{
		get
		{
			Collider c = GetComponent<Collider>();
			if (c != null) return c.enabled;
			Collider2D b = GetComponent<Collider2D>();
			return (b != null && b.enabled);
		}
	}

	void OnHover (bool isOver)
	{
        if (UIEventTriggerFlag.bUsing || !isColliderEnabled) return;
        UIEventTriggerFlag.bUsing = true;
		if (isOver) EventDelegate.Execute(onHoverOver);
		else EventDelegate.Execute(onHoverOut);
        UIEventTriggerFlag.bUsing = false;
	}

	void OnPress (bool pressed)
	{
        if (UIEventTriggerFlag.bUsing || !isColliderEnabled) return;
        UIEventTriggerFlag.bUsing = true;
        if (pressed) EventDelegate.Execute(onPress);
		else EventDelegate.Execute(onRelease);
        UIEventTriggerFlag.bUsing = false;
    }
	void OnPressure(bool pressed)
	{
        if (UIEventTriggerFlag.bUsing || !isColliderEnabled) return;
        UIEventTriggerFlag.bUsing = true;
        if (pressed) EventDelegate.Execute(onPressure);
		else EventDelegate.Execute(onRelease);
        UIEventTriggerFlag.bUsing = false;
    }
	void OnSelect (bool selected)
	{
        if (UIEventTriggerFlag.bUsing || !isColliderEnabled) return;
        UIEventTriggerFlag.bUsing = true;
        if (selected) EventDelegate.Execute(onSelect);
		else EventDelegate.Execute(onDeselect);
        UIEventTriggerFlag.bUsing = false;
    }

	void OnClick ()
	{
        if (UIEventTriggerFlag.bUsing || !isColliderEnabled) return;
        UIEventTriggerFlag.bUsing = true;
        EventDelegate.Execute(onClick);
        UIEventTriggerFlag.bUsing = false;
    }

	void OnDoubleClick ()
	{
        if (UIEventTriggerFlag.bUsing || !isColliderEnabled) return;
        UIEventTriggerFlag.bUsing = true;
        EventDelegate.Execute(onDoubleClick);
        UIEventTriggerFlag.bUsing = false;
    }

	void OnDragStart ()
	{
        if (UIEventTriggerFlag.bUsing || !isColliderEnabled) return;
        UIEventTriggerFlag.bUsing = true;
        EventDelegate.Execute(onDragStart);
        UIEventTriggerFlag.bUsing = false;
    }

	void OnDragEnd ()
	{
        if (UIEventTriggerFlag.bUsing || !isColliderEnabled) return;
        UIEventTriggerFlag.bUsing = true;
        EventDelegate.Execute(onDragEnd);
        UIEventTriggerFlag.bUsing = false;
    }

	void OnDragOver (GameObject go)
	{
        if (UIEventTriggerFlag.bUsing || !isColliderEnabled) return;
        UIEventTriggerFlag.bUsing = true;
        EventDelegate.Execute(onDragOver);
        UIEventTriggerFlag.bUsing = false;
    }

	void OnDragOut (GameObject go)
	{
        if (UIEventTriggerFlag.bUsing || !isColliderEnabled) return;
        UIEventTriggerFlag.bUsing = true;
        EventDelegate.Execute(onDragOut);
        UIEventTriggerFlag.bUsing = false;
    }

	void OnDrag (Vector2 delta)
	{
        if (UIEventTriggerFlag.bUsing || !isColliderEnabled) return;
        UIEventTriggerFlag.bUsing = true;
        EventDelegate.Execute(onDrag);
        UIEventTriggerFlag.bUsing = false;
    }
}
