public abstract class AgentTask
{
    public bool IsCompleted { get; protected set; }
    public abstract void Update();
}