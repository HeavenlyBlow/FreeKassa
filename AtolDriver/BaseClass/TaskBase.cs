using AtolDriver.Models.AnswerModel;

namespace AtolDriver.BaseClass;

public abstract class TaskBase
{
    public int Priority { get; set; }
    public TaskCompletionSource<Answer> Completion { get; set; }
}

public class JsonTask : TaskBase
{
    public string Task { get; set; }
}

public class FunctionTask : TaskBase
{
    public Func<Answer> Task { get; set; }
}

