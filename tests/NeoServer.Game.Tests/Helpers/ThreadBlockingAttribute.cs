using System;
using System.Reflection;
using System.Threading;
using Xunit.Sdk;

namespace NeoServer.Game.Tests.Helpers;

/// <summary>
/// Block the unit test to avoid concurrency
/// </summary>
public class ThreadBlockingAttribute : BeforeAfterTestAttribute
{
    public override void Before(MethodInfo methodUnderTest) => ThreadBlocking.Wait();
    public override void After(MethodInfo methodUnderTest) => ThreadBlocking.Release();
}

public static class ThreadBlocking
{
    private static readonly SemaphoreSlim SemaphoreSlim = new(1, 1);

    public static void Wait()
    {
        try
        {
            SemaphoreSlim.Wait();
        }
        catch (Exception ex)
        {
            Console.Write(ex);
        }
    }

    public static void Release() => SemaphoreSlim.Release();
}