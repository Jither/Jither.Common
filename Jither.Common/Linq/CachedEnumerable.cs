using System;
using System.Collections;
using System.Collections.Generic;

namespace Jither.Linq;

public class CachedEnumerable<T> : IEnumerable<T>
{
    private readonly IEnumerator<T> enumerator;
    private readonly List<T> cache = new();

    public CachedEnumerable(IEnumerable<T> enumerable) : this(enumerable.GetEnumerator())
    {

    }

    public CachedEnumerable(IEnumerator<T> enumerator)
    {
        this.enumerator = enumerator;
    }

    public IEnumerator<T> GetEnumerator()
    {
        int index = 0;

        while (true)
        {
            if (index >= cache.Count)
            {
                if (enumerator.MoveNext())
                {
                    var current = enumerator.Current;
                    cache.Add(current);
                }
                else
                {
                    yield break;
                }
            }
            yield return cache[index++];
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
