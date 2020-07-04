using UnityEngine;
using System.Collections.Generic;

public abstract class UpdateHandler : MonoBehaviour
{
	[System.NonSerialized]
	public bool isTick = true;
	[System.NonSerialized]
	public bool isBecameVisible = true;
    public abstract void Tick();
}

public abstract class VisiableUpdateHandler : UpdateHandler
{
	public virtual void OnBecameVisible()
    {
		isBecameVisible = true;
    }

	public virtual void OnBecameInvisible()
    {
		isBecameVisible = false;
    }

	public virtual void OnEnable()
	{
		isTick = true;
	}

	public virtual void OnDisable()
	{
		isTick = false;
	}

	public virtual void Start()
	{
		VisiableUpdateHandlerManager.AddUpdate(this);
	}

	public virtual void OnDestroy()
	{
		VisiableUpdateHandlerManager.RemoveUpdate(this);
	}
}


public class VisiableUpdateHandlerManager : MonoBehaviour
{
    readonly List<UpdateHandler> visiableUpdateHandlerList = new List<UpdateHandler>();
    private static VisiableUpdateHandlerManager _instance;
    private static VisiableUpdateHandlerManager Instance
    {
        get
        {
            if (_instance == null)
            {
                var go = new GameObject("VisiableUpdateHandlerManager.");
                _instance = go.AddComponent<VisiableUpdateHandlerManager>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    public static void AddUpdate(UpdateHandler handler)
    {
        Instance.visiableUpdateHandlerList.Add(handler);
    }

    public static void RemoveUpdate(UpdateHandler handler)
    {
        Instance.visiableUpdateHandlerList.Remove(handler);
    }

    void Update()
    {
        if (visiableUpdateHandlerList.Count == 0)
        {
            return;
        }
        for (int i = visiableUpdateHandlerList.Count - 1; i >= 0; i--)
        {
            var handler = visiableUpdateHandlerList[i];
            if (handler == null)
            {
				visiableUpdateHandlerList.RemoveAt(i);
				continue;
			}
			if (handler.isTick && handler.isBecameVisible)
			{
				handler.Tick();
			}
		}
    }
}
