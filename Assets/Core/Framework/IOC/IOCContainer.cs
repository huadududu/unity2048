using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.VisualScripting;

public class IOCContainer
{
    private Dictionary<Type, object> mInstance = new Dictionary<Type, object>();
    public void Register<T>(T instance)
    {
        var key = typeof(T);
        if (mInstance.ContainsKey(key))
        {
            mInstance[key] = instance;
        }
        else
        {
            mInstance.Add(key, instance);
        }
    }
    public T Get<T>() where T : class
    {
        var key = typeof(T);
        if (mInstance.ContainsKey(key))
        {
            return (T)mInstance[key];
        }
        throw new InvalidOperationException("Variable not found");
    }
}
