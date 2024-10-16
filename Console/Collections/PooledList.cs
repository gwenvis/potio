namespace Gwenvis.DeveloperConsole.Collections;

public class PooledList<T> : List<T>, IDisposable
{
    private static Stack<PooledList<T>> _listStack = new Stack<PooledList<T>>();
    private const int MaxSize = 256;

    private PooledList() : base() { }
    private PooledList(int capacity) : base(capacity) { }
    private PooledList(IEnumerable<T> enumerable) : base(enumerable) { }
        
    public static PooledList<T> GetList()
    {
        return _listStack.TryPop(out var popped) ? popped : new PooledList<T>();
    }

    public static PooledList<T> GetList(IEnumerable<T> enumerable)
    {
        if (_listStack.TryPop(out var popped))
        {
            popped.AddRange(enumerable);
            return popped;
        }

        return new PooledList<T>(enumerable);
    }
        
    public static PooledList<T> GetList(int capacity)
    {
        if (_listStack.TryPop(out var popped))
        {
            if (popped.Capacity < capacity)
                popped.Capacity = capacity;
            return popped;
        }

        return new PooledList<T>(capacity);
    }

    public void Dispose()
    {
        Clear();
        if (Capacity > MaxSize) Capacity = MaxSize; 
        _listStack.Push(this);
    }
}