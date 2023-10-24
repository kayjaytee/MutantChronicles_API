using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static MongoDB.Driver.WriteConcern;

namespace MutantChroniclesAPI.Tests;

public static class TestUtility
{

    public static void InvokePrivateMethod(object instance, string methodName, params object[] parameters)
    {
        var methodInfo = instance.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
        methodInfo.Invoke(instance, parameters);
    }

    public static T InvokePrivateMethod<T>(object instance, string methodName, params object[] parameters)
    {
        var methodInfo = instance.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
        if (methodInfo == null)
        {
            throw new ArgumentException($"Method '{methodName}' not found in type '{instance.GetType().FullName}'.");
        }

        var result = methodInfo.Invoke(instance, parameters);
        return (T)result;
    }

    public static async Task<T> InvokePrivateMethodAsync<T>(object instance, string methodName, params object[] parameters)
    {
        var methodInfo = instance.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
        var task = methodInfo.Invoke(instance, parameters) as Task<T>;
        return await task;
    }

    public static void SetPrivateField<T>(object instanceOrType, string fieldName, T value)
    {
        BindingFlags bindingFlags = BindingFlags.NonPublic | BindingFlags.Static;

        Type type = instanceOrType as Type ?? instanceOrType.GetType();

        var fieldInfo = type.GetField(fieldName, bindingFlags);
        if (fieldInfo == null)
        {
            throw new ArgumentException($"Field '{fieldName}' not found in type '{type.FullName}'.");
        }

        fieldInfo.SetValue(instanceOrType, value);
    }

    public static T GetPrivateField<T>(object instanceOrType, string fieldName)
    {
        BindingFlags bindingFlags = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

        Type type = instanceOrType as Type ?? instanceOrType.GetType();

        var fieldInfo = type.GetField(fieldName, bindingFlags);
        if (fieldInfo == null)
        {
            throw new ArgumentException($"Field '{fieldName}' not found in type '{type.FullName}'.");
        }

        return (T)fieldInfo.GetValue(instanceOrType);
    }
}
