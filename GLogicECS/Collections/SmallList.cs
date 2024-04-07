namespace GLogicECS.Collections;

public struct SmallList<T> where T : IEquatable<T>
{
    private T a;
    private T b;
    private List<T>? _grow;

    public nuint Count;

    public T this[int index]
    {
        get
        {
            if (index < 0 || index >= (int)Count)
            {
                throw new IndexOutOfRangeException();
            }

            return index switch
            {
                0 => a,
                1 => b,
                _ => _grow![index - 2]
            };
        }
        set
        {
            if (index < 0 || index >= (int)Count)
            {
                throw new IndexOutOfRangeException();
            }

            switch (index)
            {
                case 0:
                    a = value;
                    break;
                case 1:
                    b = value;
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
                a = element;
                break;
            case 1:
                b = element;
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

        this[index] = this[(int)--Count];

        return true;
    }

    public int IndexOf(T element)
    {
        for (var i = 0; i < (int)Count; i++)
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