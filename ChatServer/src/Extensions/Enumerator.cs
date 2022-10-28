namespace ChatServer.Extensions;

public static partial class Extensions
{
    public static IntEnumerator GetEnumerator(this Range range) => new IntEnumerator(range);
    public static IntEnumerator GetEnumerator(this int max) => new IntEnumerator(new Range(1, max));
}

public struct IntEnumerator
{
    private readonly int _end;

    public IntEnumerator(Range range)
    {
        if (range.End.IsFromEnd)
        {
            throw new NotSupportedException();
        }
        
        Current = range.Start.Value - 1;
        _end = range.End.Value;
    }

    public int Current { get; private set; }
    
    public bool MoveNext()
    {
        Current++;
        return Current <= _end;
    }
}