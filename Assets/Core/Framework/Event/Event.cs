using System;
public class Event<T> where T : Event<T>
{
    private static Action mOnEvent;
    public static void Register(Action onEvent)
    {
        mOnEvent += onEvent;
    }
    public static void UnRegister(Action onEvent)
    {
        mOnEvent -= onEvent;
    }
    public static void Trigger()
    {
        mOnEvent?.Invoke();
    }
   
}

public class Event<T, TArg> where T : Event<T, TArg>
{
    private static Action<TArg> mOnEvent;
    public static void Register(Action<TArg> onEvent)
    {
        mOnEvent += onEvent;
    }
    public static void UnRegister(Action<TArg> onEvent)
    {
        mOnEvent -= onEvent;
    }
    public static void Trigger(TArg arg)
    {
        mOnEvent?.Invoke(arg);
    }
}
