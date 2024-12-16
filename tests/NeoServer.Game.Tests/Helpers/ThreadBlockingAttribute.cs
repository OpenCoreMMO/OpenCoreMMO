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
    private static readonly Mutex Mutex = new();
    public override void Before(MethodInfo methodUnderTest)
    {
        Mutex.WaitOne();
        // Block the unit test thread
    }

    public override void After(MethodInfo methodUnderTest)
    {
        Mutex.ReleaseMutex();
        // Release the unit test thread
    }

}