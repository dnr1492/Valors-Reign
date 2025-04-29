using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerRegister
{
    private static Dictionary<Type, object> register = new();

    public static void Register<T>(T controller) where T : class
    {
        var type = typeof(T);
        if (!register.ContainsKey(type)) register[type] = controller;
    }

    public static T Get<T>() where T : class
    {
        register.TryGetValue(typeof(T), out var value);
        return value as T;
    }
}
