using System.Collections;

namespace GLogicECS.Collections;

public struct SmallList<T> : ICollection<T> where T : struct, IEquatable<T>
{
    private T _a;
    private T _b;
    private List<T>? _grow;

    public SmallList()
    {
        _a = default;
        _b = default;
        _grow = null;
        Count = 0;
    }

    public int Count { get; private set; }
    public bool IsReadOnly { get; } = false;

    public T this[int index]
    {
        get
        {
            if (index < 0 || index >= Count)
            {
                return new T();
            }

            return index switch
            {
                0 => _a,
                1 => _b,
                _ => _grow![index - 2]
            };
        }
        set
        {
            if (index < 0 || index >= Count)
            {
                throw new IndexOutOfRangeException();
            }

            switch (index)
            {
                case 0:
                    _a = value;
                    break;
                case 1:
                    _b = value;
                    break;
                default:
                    _grow![index - 2] = value;
                    break;
            }
        }
    }

    public void Add(T element)
    {
        switch (Count++)
        {
            case 0:
                _a = element;
                break;
            case 1:
                _b = element;
                break;
            default:
                _grow ??= new List<T>();
                _grow.Add(element);
                break;
        }
    }

    public void Clear()
    {
        Count = 0;
    }

    public bool Contains(T item)
    {
        for (var i = 0; i < Count; i++)
        {
            var current = this[i];

            if (current!.Equals(item))
            {
                return true;
            }
        }

        return false;
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        var lenght = array.Length - arrayIndex;
        
        if (lenght > Count)
        {
            throw new ArgumentOutOfRangeException("To long destination array offset");
        }

        if (lenght > 0)
        {
            array[arrayIndex] = _a;
        } else return;

        if (lenght > 1)
        {
            array[arrayIndex + 1] = _b;
        } else return;

        if (lenght > 2)
        {
            _grow!.CopyTo(array, arrayIndex + 2);
        }
    }

    public bool Remove(T element)
    {
        if (Count == 0)
        {
            return false;
        }

        var index = IndexOf(element);
        if (index == -1)
        {
            return false;
        }

        this[index] = this[Count - 1];

        Count--;
        return true;
    }

    public int IndexOf(T element)
    {
        for (var i = 0; i < Count; i++)
        {
            var current = this[i];

            if (current!.Equals(element))
            {
                return i;
            }
        }

        return -1;
    }

    public IEnumerator<T> GetEnumerator()
    {
        if (Count > 0)
        {
            yield return _a;
        }

        if (Count > 1)
        {
            yield return _b;
        }

        if (_grow is null)
        {
            yield break;
        }

        foreach (var t in _grow)
        {
            yield return t;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();
}