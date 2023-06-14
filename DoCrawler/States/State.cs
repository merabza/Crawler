using Microsoft.Extensions.Logging;

namespace DoCrawler.States;

public /*open*/ class State
{
    protected readonly ILogger Logger;

    protected State(ILogger logger, string stateName)
    {
        Logger = logger;
        StateName = stateName;
        StateId = ProcData.Instance.GetNewStateId();
    }

    public int StateId { get; }
    public string StateName { get; }

    //BackProcessor bp
    public virtual void Execute()
    {
        //, bp
        Logger.LogWarning($"Execute was not override in {nameof(State)}", false);
    }

    //BackProcessor bp
    //public virtual void GoNext()
    //{
    //}
}