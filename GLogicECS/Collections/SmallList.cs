namespace GLogicECS.Collections;

public struct SmallList<T> where T : struct, IEquatable<T>
{
    private T _a;
    private T _b;
    private List<T>? _grow;

    public int Count;

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
}