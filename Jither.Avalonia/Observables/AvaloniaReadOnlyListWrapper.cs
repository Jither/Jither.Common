using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jither.Avalonia.Observables;

/// <summary>
/// Wrapper helper to avoid implementing (Avalonia-required) IList on readonly collections.
/// </summary>
/// <remarks>
/// The wrapper simply forwards all calls to the wrapped list, and throws on IList methods not supported.
/// In addition, it surfaces the list's collection change notifications as its own and automates property change notifications
/// when the wrapped list's Count changes.
/// </remarks>
public class AvaloniaReadOnlyListWrapper<T> : IList, IReadOnlyList<T>, INotifyPropertyChanged, INotifyCollectionChanged
{
    private static readonly PropertyChangedEventArgs CountChangedArgs = new PropertyChangedEventArgs(nameof(Count));

    private readonly IReadOnlyList<T> list;
    private int currentCount;

    public AvaloniaReadOnlyListWrapper(IReadOnlyList<T> list)
    {
        this.list = list;
        currentCount = list.Count;
        if (list is INotifyCollectionChanged ncc)
        {
            ncc.CollectionChanged += ListCollectionChanged;
        }
    }

    private void ListCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        CollectionChanged?.Invoke(this, e);
        if (list.Count != currentCount)
        {
            currentCount = list.Count;
            PropertyChanged?.Invoke(this, CountChangedArgs);
            CountChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public T this[int index] { get => list[index]; set => throw new NotSupportedException(); }
    object? IList.this[int index] { get => list[index]; set => throw new NotSupportedException(); }

    public int Count => list.Count;

    public bool IsReadOnly => true;
    public bool IsFixedSize => false;
    public bool IsSynchronized => false;

    public object SyncRoot => throw new NotSupportedException();

    public event PropertyChangedEventHandler? PropertyChanged;
    public event NotifyCollectionChangedEventHandler? CollectionChanged;
    public event EventHandler? CountChanged;

    public int Add(object? value) => throw new NotSupportedException();
    public void Clear() => throw new NotSupportedException();
    public bool Contains(T item) => list.Contains(item);
    public bool Contains(object? value) => value is T item ? list.Contains(item) : false;

    public void CopyTo(T[] array, int arrayIndex)
    {
        for (int i = 0; i < list.Count; i++)
        {
            array[arrayIndex++] = list[i];
        }
    }
    public void CopyTo(Array array, int index)
    {
        for (int i = 0; i < list.Count; i++)
        {
            array.SetValue(list[i], index++);
        }
    }

    public int IndexOf(T item)
    {
        if (list is IList<T> concrete)
        {
            return concrete.IndexOf(item);
        }
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i]?.Equals(item) == true)
            {
                return i;
            }
        }
        return -1;
    }

    public int IndexOf(object? value)
    {
        if (value is T item)
        {
            return IndexOf(item);
        }
        return -1;
    }

    public void Insert(int index, T item) => throw new NotSupportedException();
    public void Insert(int index, object? value) => throw new NotSupportedException();
    public bool Remove(T item) => throw new NotSupportedException();
    public void Remove(object? value) => throw new NotSupportedException();
    public void RemoveAt(int index) => throw new NotSupportedException();

    public IEnumerator<T> GetEnumerator() => list.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

