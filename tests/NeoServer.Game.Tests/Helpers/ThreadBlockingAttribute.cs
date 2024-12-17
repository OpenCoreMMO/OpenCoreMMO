using System;
using System.Reflection;
using System.Threading;
using Xunit.Sdk;

namespace NeoServer.Game.Tests.Helpers;

/// <summary>
///     Block the unit test to avoid concurrency
/// </summary>
public class ThreadBlockingAttribute : BeforeAfterTestAttribute
{
    public override void Before(MethodInfo methodUnderTest) => ThreadBlocking.Wait();

    public override void After(MethodInfo methodUnderTest) => ThreadBlocking.Release();
}

public static class ThreadBlocking
{
    private static readonly Mutex Mutex = new();
    public static void Wait()
    {
        try
        {
            Mutex.WaitOne();
            // Mutex acquired
        }
        catch (AbandonedMutexException)
        {
            Console.WriteLine("Warning: Mutex was abandoned by another thread.");
        }
    }

    public static void Release()
    {
        if (Mutex.WaitOne(0)) // Check if mutex is owned before releasing
        {
            Mutex.ReleaseMutex();
        }
        else
        {
            Console.WriteLine("Warning: Mutex was not owned by the current thread.");
        }
    }
}