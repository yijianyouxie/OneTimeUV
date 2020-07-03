//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2015 Tasharen Entertainment
//----------------------------------------------
//#undef UNITY_EDITOR
//#define HEAPPOOL_PERF_TEST
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System;

/// <summary>
/// This improved version of the System.Collections.Generic.List that doesn't release the buffer on Clear(),
/// resulting in better performance and less garbage collection.
/// PRO: BetterList performs faster than List when you Add and Remove items (although slower if you remove from the beginning).
/// CON: BetterList performs worse when sorting the list. If your operations involve sorting, use the standard List instead.
/// </summary>

public struct PoolFlag_t
{
    public int NotInPool;
    public int InPool;
    public int total;

    public void Clear()
    {
        NotInPool = 0;
        InPool = 0;
        total = 0;
    }
    public int CanDel()
    {
        //如果池子里没有，不清了。
        if (InPool < 10)
            return 0;

        //外面正在用100个，预计留25个
        int b = (int)(NotInPool / 4);

        //外面用的少于4个，不清了。
        if (b == 0)
            return 0;

        //否则在外面正在用的是100个，则池里保留25个。
        if (InPool>b)
        {
            return InPool - b;
        }
        return 0;
    }
};

public class HeapPoolEx
{
    public static Dictionary<Type, Dictionary<int, PoolFlag_t>> m_vTouchZeroDic = new Dictionary<Type, Dictionary<int, PoolFlag_t>>();
    public static Dictionary<Type, Dictionary<int, Queue>> m_vQueueArrayDic = new Dictionary<Type, Dictionary<int, Queue>>();
    public static Dictionary<Type, Queue> m_vQueueDic = new Dictionary<Type, Queue>();

    private static float incGCValue = 0;
    //private static int gcCount = 0;

    private static void ClearHalf(Dictionary<int, System.Collections.Queue> dic)
    {
        if (dic != null)
        {
            if (dic.Count > 1)
            {
                foreach (KeyValuePair<int, System.Collections.Queue> data in dic)
                {
                    System.Collections.Queue q = data.Value;
                    int cnt = q.Count;
                    if (cnt > 1)
                    {
                        //清一半
                        int delcnt = (int)(cnt / 2);
                        for (int i = 0; i < delcnt; i++)
                        {
                            System.Object obj = q.Dequeue();
                            obj = null;
                        }
                    }
                }
            }
        }
    }

    public static void ClearNoTouchZero(Dictionary<int, System.Collections.Queue> dic, Dictionary<int, PoolFlag_t> dicTouchZero)
    {
        if (dic != null)
        {
            if (dic.Count > 1)
            {
                foreach (KeyValuePair<int, System.Collections.Queue> data in dic)
                {
                    PoolFlag_t t_TouchZero = new PoolFlag_t();
                    t_TouchZero.Clear();
                    dicTouchZero.TryGetValue(data.Key, out t_TouchZero);
                    int cnt = 0;
                    int delcnt = t_TouchZero.CanDel();
                    if (delcnt>0)
                    {
                        System.Collections.Queue q = data.Value;
                        cnt = q.Count;
                        if (cnt > 1)
                        {
                            //清三分之一
                            for (int i = 0; i < delcnt; i++)
                            {
                                System.Object obj = q.Dequeue();
                                obj = null;
                            }
                        }
                    }
                    t_TouchZero.InPool -= delcnt;
                    t_TouchZero.total -= delcnt;

                    dicTouchZero[data.Key] = t_TouchZero;
                }
            }
        }
    }

    public static void ClearWhenChangeScene()
    {
        //這三個池太頻繁了，不清了
        Dictionary<int, System.Collections.Queue> qV3Dic = null;
        Dictionary<int, System.Collections.Queue> qV2Dic = null;
        Dictionary<int, System.Collections.Queue> qC3Dic = null;

        m_vQueueArrayDic.TryGetValue(typeof(UnityEngine.Vector3), out qV3Dic);
        m_vQueueArrayDic.TryGetValue(typeof(UnityEngine.Vector2), out qV2Dic);
        m_vQueueArrayDic.TryGetValue(typeof(UnityEngine.Color32), out qC3Dic);

        Dictionary<int, PoolFlag_t> dicV3TouchZero = null;
        Dictionary<int, PoolFlag_t> dicV2TouchZero = null;
        Dictionary<int, PoolFlag_t> dicC3TouchZero = null;

        m_vTouchZeroDic.TryGetValue(typeof(UnityEngine.Vector3), out dicV3TouchZero);
        m_vTouchZeroDic.TryGetValue(typeof(UnityEngine.Vector2), out dicV2TouchZero);
        m_vTouchZeroDic.TryGetValue(typeof(UnityEngine.Color32), out dicC3TouchZero);


        m_vQueueArrayDic.Clear();

        ClearNoTouchZero(qV3Dic, dicV3TouchZero);
        ClearNoTouchZero(qV2Dic, dicV2TouchZero);
        ClearNoTouchZero(qC3Dic, dicC3TouchZero);

        if (qV3Dic != null) m_vQueueArrayDic.Add(typeof(UnityEngine.Vector3), qV3Dic);
        if (qV2Dic != null) m_vQueueArrayDic.Add(typeof(UnityEngine.Vector2), qV2Dic);
        if (qC3Dic != null) m_vQueueArrayDic.Add(typeof(UnityEngine.Color32), qC3Dic);

        m_vQueueDic.Clear();
    }

    public static void Clear()
    {
#if (OPTIMISE_GC_SY_20190426)
        return;
#endif
        //這三個池太頻繁了，不清了
        Dictionary<int, System.Collections.Queue> qV3Dic = null;
        Dictionary<int, System.Collections.Queue> qV2Dic = null;
        Dictionary<int, System.Collections.Queue> qC3Dic = null;

        m_vQueueArrayDic.TryGetValue(typeof(UnityEngine.Vector3), out qV3Dic);
        m_vQueueArrayDic.TryGetValue(typeof(UnityEngine.Vector2), out qV2Dic);
        m_vQueueArrayDic.TryGetValue(typeof(UnityEngine.Color32), out qC3Dic);
        m_vQueueArrayDic.Clear();
        ClearHalf(qV3Dic);
        ClearHalf(qV2Dic);
        ClearHalf(qC3Dic);

        if (qV3Dic != null) m_vQueueArrayDic.Add(typeof(UnityEngine.Vector3), qV3Dic);
        if (qV2Dic != null) m_vQueueArrayDic.Add(typeof(UnityEngine.Vector2), qV2Dic);
        if (qC3Dic != null) m_vQueueArrayDic.Add(typeof(UnityEngine.Color32), qC3Dic);

        m_vQueueDic.Clear();
    }

    public static T[] NewArrayToLarger<T>(int count)
    {
        if (count < 16)
        {
            count = 16;
        }
        float beginGCValue = (GC.GetTotalMemory(false) / (1048576.0f));

#if HEAPPOOL_PERF_TEST
        newTimer.Start();
#endif
        T[] t = internal_NewArrayToLarger<T>(count);

#if HEAPPOOL_PERF_TEST
        newTimer.Stop();
        newTime = (float)newTimer.ElapsedMilliseconds / newCount;
#endif

        float endGCValue = (GC.GetTotalMemory(false) / (1048576.0f));
        incGCValue += Math.Abs(endGCValue - beginGCValue);
        if (incGCValue > 5.0f)
        {
            Clear();
            //SingletonMonoBehaviour.Instance.DelayInvoke(
            //1,
            //() =>
            //{
            //    GC.Collect();
            //    //++gcCount;
            //    incGCValue = 0;
            //},
            //3);
        }
        return t;
    }

#if HEAPPOOL_PERF_TEST
    // 以下为性能分析信息
    private static int newCount = 0;    // 申请内存分配次数
    private static int newQueueEmptyCount = 0;  // 申请分配时碰到池里没有数据需要重新new的次数
    private static int newTypeCount = 0;    // 碰到池里没有所要的这个类型数据的次数
    private static int newCountCount = 0;   // 碰到池里没有所要的这个内存大小的次数

    private static int deleteCount = 0; // 内存返回给池的次数

    private static int cacheHitCount = 0;   // 从池里正确命中所要内存的次数
    private static float cachePer;  // 命中率，越高越好

    private static Stopwatch newTimer = new Stopwatch();
    private static Stopwatch deleteTimer = new Stopwatch();

    private static float newTime;   // 每次new的时间，ms
    private static float deleteTime;    // 每次delete的时间，ms
#endif

    private static T[] internal_NewArrayToLarger<T>(int count)
    {
#if HEAPPOOL_PERF_TEST
        ++newCount;
        cachePer = (float)cacheHitCount / newCount;
#endif
        //int doubleCount = count << 2;
        //int doubleCount = Games.TLBB.Util.GameUtil.GetNearest2N(count);
        int doubleCount = 2 * count;
        Dictionary<int, PoolFlag_t> dicBool = null;
        Dictionary<int, Queue> qDic = null;

        if (m_vQueueArrayDic.TryGetValue(typeof(T), out qDic))
        {
            Queue queue = null;

            if (qDic.TryGetValue(doubleCount, out queue))
            {
                if (queue.Count > 0)
                {
#if HEAPPOOL_PERF_TEST
                    ++cacheHitCount;
#endif

                    if (m_vTouchZeroDic.TryGetValue(typeof(T), out dicBool))
                    {
                        //这个池见底了。
                        PoolFlag_t pf = dicBool[doubleCount];
                        pf.InPool -= 1;
                        pf.NotInPool += 1;
                        dicBool[doubleCount] = pf;
                    }

                    return queue.Dequeue() as T[];
                }
                else
                {
#if HEAPPOOL_PERF_TEST
                    ++newQueueEmptyCount;
#endif
                }
            }
#if HEAPPOOL_PERF_TEST
            else
            {
                if (qDic.TryGetValue(doubleCount*2, out queue))
                {
                    if (queue.Count > 0)
                    {
#if HEAPPOOL_PERF_TEST
                        ++cacheHitCount;
#endif
                        return queue.Dequeue() as T[];
                    }
                    else
                    {
#if HEAPPOOL_PERF_TEST
                        ++newQueueEmptyCount;
#endif
                    }
                }
                ++newCountCount;
            }
#endif
        }
#if HEAPPOOL_PERF_TEST
        else
            ++newTypeCount;
#endif

        if (m_vTouchZeroDic.TryGetValue(typeof(T), out dicBool))
        {
            //这个池见底了。
            PoolFlag_t pf = new PoolFlag_t();
            dicBool.TryGetValue(doubleCount, out pf);
            pf.InPool = 0;
            pf.NotInPool += 1;
            pf.total += 1;
            dicBool[doubleCount] = pf;
        }
        else
        {
            dicBool = new Dictionary<int, PoolFlag_t>();
            PoolFlag_t pf = new PoolFlag_t();
            pf.Clear();
            pf.InPool = 0;
            pf.NotInPool = 1;
            pf.total = 1;
            dicBool[doubleCount] = pf;
            m_vTouchZeroDic.Add(typeof(T), dicBool);
        }
        return new T[doubleCount];
    }

    public static void DeleteArray<T>(ref T[] array)
    {
#if HEAPPOOL_PERF_TEST
        ++deleteCount;
        deleteTimer.Start();
#endif

        Dictionary<int, Queue> qDic = null;
        int size = array.Length;
        if (false == m_vQueueArrayDic.TryGetValue(typeof(T), out qDic))
        {
            qDic = new Dictionary<int, Queue>();
            m_vQueueArrayDic.Add(typeof(T), qDic);
        }

        Queue q = null;
        if (false == qDic.TryGetValue(size, out q))
        {
            q = new Queue();
            qDic.Add(size, q);
        }

        Dictionary<int, PoolFlag_t> dicBool = null;

        if (m_vTouchZeroDic.TryGetValue(typeof(T), out dicBool))
        {
            //这个池见底了。
            PoolFlag_t pf = dicBool[size];
            pf.InPool += 1;
            pf.NotInPool -= 1;
            dicBool[size] = pf;
        }
        q.Enqueue(array);
        array = null;
#if HEAPPOOL_PERF_TEST
        deleteTimer.Stop();
        deleteTime = (float)newTimer.ElapsedMilliseconds / deleteCount;
#endif
    }
}

public class BetterList<T>
{
    /// <summary>
    /// Direct access to the buffer. Note that you should not use its 'Length' parameter, but instead use BetterList.size.
    /// </summary>

    public T[] buffer;

    /// <summary>
    /// Direct access to the buffer's size. Note that it's only public for speed and efficiency. You shouldn't modify it.
    /// </summary>

    public int size = 0;

    /// <summary>
    /// 实际buffer的size，缓存下来避免每次都调用Array.Length
    /// </summary>
    public int bufferLength = 0;

    /// <summary>
    /// For 'foreach' functionality.
    /// </summary>

    [DebuggerHidden]
    [DebuggerStepThrough]
    public IEnumerator<T> GetEnumerator()
    {
        if (buffer != null)
        {
            for (int i = 0; i < size; ++i)
            {
                yield return buffer[i];
            }
        }
    }

    /// <summary>
    /// Convenience function. I recommend using .buffer instead.
    /// </summary>

    [DebuggerHidden]
    public T this[int i]
    {
        get { return buffer[i]; }
        set { buffer[i] = value; }
    }

    /// <summary>
    /// Helper function that expands the size of the array, maintaining the content.
    /// </summary>
#if UNITY_EDITOR
    public void Allocate(int sz)
    {
        if (buffer != null && bufferLength >= sz)
        {
            return;
        }
        T[] newList = (buffer != null) ? new T[Math.Max(buffer.Length << 1, sz)] : new T[sz];
        if (buffer != null && size > 0) buffer.CopyTo(newList, 0);
        buffer = newList;
        bufferLength = buffer.Length;
    }
#else
    public void Allocate(int sz)
    {
        if (buffer != null && bufferLength >= sz)
        {
            return;
        }
        T[] newList = null;

        newList = HeapPoolEx.NewArrayToLarger<T>(sz);


        if (buffer != null && size > 0) buffer.CopyTo(newList, 0);

        T[] oldBuffer = buffer;
        if (oldBuffer != null)
            HeapPoolEx.DeleteArray<T>(ref oldBuffer);

        buffer = newList;
        bufferLength = buffer.Length;
    }
#endif

#if UNITY_EDITOR
    public void AllocateMore()
    {
        T[] newList = (buffer != null) ? new T[Math.Max(bufferLength << 1, 32)] : new T[32];
        if (buffer != null && size > 0) buffer.CopyTo(newList, 0);
        buffer = newList;
        bufferLength = buffer.Length;
    }
#else
    public void AllocateMore()
    {
        T[] newList = null;

        newList = HeapPoolEx.NewArrayToLarger<T>(bufferLength);

        if (buffer != null && size > 0) buffer.CopyTo(newList, 0);

        T[] oldBuffer = buffer;
        if (oldBuffer != null)
            HeapPoolEx.DeleteArray<T>(ref oldBuffer);

        buffer = newList;
        bufferLength = buffer.Length;
    }
#endif

    /// <summary>
    /// Trim the unnecessary memory, resizing the buffer to be of 'Length' size.
    /// Call this function only if you are sure that the buffer won't need to resize anytime soon.
    /// </summary>

    void Trim()
    {
        if (size > 0)
        {
            Allocate(size);
        }
        else
        {
            Clear();
        }
    }
#if UNITY_EDITOR
    /// <summary>
    /// Clear the array by resetting its size to zero. Note that the memory is not actually released.
    /// </summary>
    public void Clear() { size = 0; buffer = null; bufferLength = 0; }
#else
    /// <summary>
    /// Clear the array by resetting its size to zero. Note that the memory is not actually released.
    /// </summary>
    public void Clear()
    {
        size = 0;

        if (buffer != null)
        {
            HeapPoolEx.DeleteArray<T>(ref buffer);
            buffer = null;
        }
        bufferLength = 0;
    }

    /// <summary>
    /// Clear the array and release the used memory.
    /// </summary>

#endif
    public void Release() { Clear(); }

    /// <summary>
    /// Add the specified item to the end of the list.
    /// </summary>

    public void Add(T item)
    {
        AllocMoreIfNeed();
        buffer[size++] = item;
    }

    public void AllocMoreIfNeed()
    {
        if (buffer == null || size == bufferLength)
            AllocateMore();
    }

    /// <summary>
    /// Insert an item at the specified index, pushing the entries back.
    /// </summary>

    public void Insert(int index, T item)
    {
        AllocMoreIfNeed();

        if (index > -1 && index < size)
        {
            for (int i = size; i > index; --i) buffer[i] = buffer[i - 1];
            buffer[index] = item;
            ++size;
        }
        else Add(item);
    }

    /// <summary>
    /// Returns 'true' if the specified item is within the list.
    /// </summary>

    public bool Contains(T item)
    {
        if (buffer == null) return false;
        for (int i = 0; i < size; ++i) if (buffer[i].Equals(item)) return true;
        return false;
    }

    /// <summary>
    /// Return the index of the specified item.
    /// </summary>

    public int IndexOf(T item)
    {
        if (buffer == null) return -1;
        for (int i = 0; i < size; ++i) if (buffer[i].Equals(item)) return i;
        return -1;
    }

    /// <summary>
    /// Remove the specified item from the list. Note that RemoveAt() is faster and is advisable if you already know the index.
    /// </summary>

    public bool Remove(T item)
    {
        if (buffer != null)
        {
            EqualityComparer<T> comp = EqualityComparer<T>.Default;

            for (int i = 0; i < size; ++i)
            {
                if (comp.Equals(buffer[i], item))
                {
                    --size;
                    buffer[i] = default(T);
                    for (int b = i; b < size; ++b) buffer[b] = buffer[b + 1];
                    buffer[size] = default(T);
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// Remove an item at the specified index.
    /// </summary>

    public void RemoveAt(int index)
    {
        if (buffer != null && index > -1 && index < size)
        {
            --size;
            buffer[index] = default(T);
            for (int b = index; b < size; ++b) buffer[b] = buffer[b + 1];
            buffer[size] = default(T);
        }
    }

    public void RemoveRange(int index, int count)
    {
        if (buffer != null && index > -1 && index < size )
        {
            if (index + count >= size)
            {
                count = size - index;
            }

            if (index == 0 && count == size)
            {
                for (int i = 0; i < size; i++)
                {
                    buffer[i] = default(T);
                }
                size = 0;
            }
            else if (index + count == size)
            {
                for (int i = index; i < size; i++)
                {
                    buffer[i] = default(T);
                }
                size -= count;
            }
            else
            {
                int cursize = index + count;
                //for (int i = index; i < cursize; i++)
                //{
                //    buffer[i] = default(T);
                //}

                for (int i = cursize; i < size; i++)
                {
                    buffer[i-count] = buffer[i];
                }

                for (int i = size-1; i > size - count - 1; i--)
                {
                    buffer[i] = default(T);
                }
                size -= count;
            }
        }
    }
    /// <summary>
    /// Remove an item from the end.
    /// </summary>

    public T Pop()
    {
        if (buffer != null && size != 0)
        {
            T val = buffer[--size];
            buffer[size] = default(T);
            return val;
        }
        return default(T);
    }

    /// <summary>
    /// Mimic List's ToArray() functionality, except that in this case the list is resized to match the current size.
    /// </summary>

    public T[] ToArray() { Trim(); return buffer; }

    //public T[] CloneToArray()
    //{
    //    if (size > 0)
    //    {
    //        if (size < buffer.Length)
    //        {
    //            T[] newList = new T[size];
    //            for (int i = 0; i < size; ++i) newList[i] = buffer[i];
    //            return newList;
    //        }
    //    }
    //    return null;
    //}

    //class Comparer : System.Collections.IComparer
    //{
    //    public System.Comparison<T> func;
    //    public int Compare (object x, object y) { return func((T)x, (T)y); }
    //}

    //Comparer mComp = new Comparer();

    /// <summary>
    /// List.Sort equivalent. Doing Array.Sort causes GC allocations.
    /// </summary>

    //public void Sort (System.Comparison<T> comparer)
    //{
    //    if (size > 0)
    //    {
    //        mComp.func = comparer;
    //        System.Array.Sort(buffer, 0, size, mComp);
    //    }
    //}

    /// <summary>
    /// List.Sort equivalent. Manual sorting causes no GC allocations.
    /// </summary>

    [DebuggerHidden]
    [DebuggerStepThrough]
    public void Sort(CompareFunc comparer)
    {
        int start = 0;
        int max = size - 1;
        bool changed = true;

        while (changed)
        {
            changed = false;

            for (int i = start; i < max; ++i)
            {
                // Compare the two values
                if (comparer(buffer[i], buffer[i + 1]) > 0)
                {
                    // Swap the values
                    T temp = buffer[i];
                    buffer[i] = buffer[i + 1];
                    buffer[i + 1] = temp;
                    changed = true;
                }
                else if (!changed)
                {
                    // Nothing has changed -- we can start here next time
                    start = (i == 0) ? 0 : i - 1;
                }
            }
        }
    }

    /// <summary>
    /// Comparison function should return -1 if left is less than right, 1 if left is greater than right, and 0 if they match.
    /// </summary>

    public delegate int CompareFunc(T left, T right);
}
