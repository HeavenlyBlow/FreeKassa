using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Diagnostics;
using AtolDriver.BaseClass;
using AtolDriver.Models.AnswerModel;

namespace AtolDriver;

// internal class TaskConveyor
public class TaskConveyor
{
    private ObservableCollection<TaskBase> _concurrentList;
    private object locker = new();
    private readonly AtolInterface _atol;

    public ObservableCollection<TaskBase> ConveyorList
    {
        get => _concurrentList;
        set
        {
            lock (locker)
            {
               var temp = value.OrderBy(c => c.Priority);
               _concurrentList = new ObservableCollection<TaskBase>(temp);
            }
        }
    }

    // internal TaskConveyor(AtolInterface atol)
    public TaskConveyor(AtolInterface atol)
    {
        _atol = atol;
        _concurrentList = new ObservableCollection<TaskBase>();
        Task.Run(() => Start());
    }

    private void Start()
    {
        while (true)
        {
            lock (locker)
            {
                
                if(!_concurrentList.Any())
                    continue;

                var item = _concurrentList[0];
                Answer answer = null;

                switch (item)
                {
                    case JsonTask jsonTask:
                        _atol.SendJson(jsonTask.Task, out answer);
                        break;
                    case FunctionTask functionTask:
                        answer = functionTask.Task.Invoke();
                        break;
                    default:
                        throw new ApplicationException("Неподдерживаемый тип");
                }
                _concurrentList.RemoveAt(0);
                item.Completion.TrySetResult(answer);
            }
        }
    }
    
}