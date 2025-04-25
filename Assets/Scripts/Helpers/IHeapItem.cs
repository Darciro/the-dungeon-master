// IHeapItem.cs
// Place this anywhere under your Scripts folder.
using System;

public interface IHeapItem<T> : IComparable<T>
{
    /// <summary>Used by Heap&gt;T&lt; to track this item’s position in the array.</summary>
    int HeapIndex { get; set; }
}
