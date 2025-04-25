// Heap.cs
// A simple binary‐heap (max‐heap) where T must implement IHeapItem&lt;T&gt;.
using System;

public class Heap<T> where T : IHeapItem<T>
{
    private T[] items;
    private int currentItemCount = 0;

    public Heap(int maxHeapSize)
    {
        items = new T[maxHeapSize];
    }

    /// <summary>Add a new item to the heap.</summary>
    public void Add(T item)
    {
        item.HeapIndex = currentItemCount;
        items[currentItemCount] = item;
        SortUp(item);
        currentItemCount++;
    }

    /// <summary>Remove and return the top‐priority item.</summary>
    public T RemoveFirst()
    {
        if (currentItemCount == 0)
            throw new InvalidOperationException("Heap is empty");

        T first = items[0];
        currentItemCount--;
        items[0] = items[currentItemCount];
        items[0].HeapIndex = 0;
        SortDown(items[0]);
        return first;
    }

    /// <summary>Call if an item’s priority (CompareTo) has changed.</summary>
    public void UpdateItem(T item)
    {
        SortUp(item);
    }

    public int Count => currentItemCount;

    /// <summary>True if this exact item is still in the heap.</summary>
    public bool Contains(T item)
    {
        if (item.HeapIndex < 0 || item.HeapIndex >= currentItemCount) return false;
        return items[item.HeapIndex].Equals(item);
    }

    private void SortDown(T item)
    {
        while (true)
        {
            int leftChildIdx = item.HeapIndex * 2 + 1;
            int rightChildIdx = leftChildIdx + 1;
            int swapIdx = -1;

            if (leftChildIdx < currentItemCount)
            {
                swapIdx = leftChildIdx;

                if (rightChildIdx < currentItemCount &&
                    items[rightChildIdx].CompareTo(items[leftChildIdx]) > 0)
                {
                    swapIdx = rightChildIdx;
                }

                if (items[swapIdx].CompareTo(item) > 0)
                {
                    Swap(item, items[swapIdx]);
                }
                else
                {
                    // both children lower priority → done
                    return;
                }
            }
            else
            {
                // no children
                return;
            }
        }
    }

    private void SortUp(T item)
    {
        int parentIdx = (item.HeapIndex - 1) / 2;

        while (parentIdx >= 0)
        {
            T parent = items[parentIdx];
            if (item.CompareTo(parent) > 0)
            {
                Swap(item, parent);
                parentIdx = (item.HeapIndex - 1) / 2;
            }
            else break;
        }
    }

    private void Swap(T a, T b)
    {
        items[a.HeapIndex] = b;
        items[b.HeapIndex] = a;
        int aIndex = a.HeapIndex;
        a.HeapIndex = b.HeapIndex;
        b.HeapIndex = aIndex;
    }
}
