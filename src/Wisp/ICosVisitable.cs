namespace Wisp;

[PublicAPI]
public interface ICosVisitable
{
    [DebuggerStepThrough]
    void Accept<TContext>(ICosVisitor<TContext> visitor, TContext context);
}