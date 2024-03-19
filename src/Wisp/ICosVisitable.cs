namespace Wisp;

[PublicAPI]
public interface ICosVisitable
{
    [DebuggerStepThrough]
    void Accept<TContext>(ICosVisitor<TContext> visitor, TContext context);

    [DebuggerStepThrough]
    TResult Accept<TContext, TResult>(ICosVisitor<TContext, TResult> visitor, TContext context);
}