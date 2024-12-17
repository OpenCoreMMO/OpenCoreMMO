using System;
using System.Reflection;
using System.Threading;
using Xunit.Sdk;

namespace NeoServer.Game.Tests.Helpers
{
    /// <summary>
    /// Block the unit test to avoid concurrency.
    /// </summary>
    public class ThreadBlockingAttribute : BeforeAfterTestAttribute
    {
        public override void Before(MethodInfo methodUnderTest) => ThreadBlocking.Wait();

        public override void After(MethodInfo methodUnderTest) => ThreadBlocking.Release();
    }

    public static class ThreadBlocking
    {
        private static readonly Mutex Mutex = new();
        private static Thread _owningThread;

        public static void Wait()
        {
            try
            {
                Mutex.WaitOne();
                _owningThread = Thread.CurrentThread;
                // Mutex acquired
            }
            catch (AbandonedMutexException)
            {
                Console.WriteLine("Warning: Mutex was abandoned by another thread.");
            }
        }

        public static void Release()
        {
            if (_owningThread == Thread.CurrentThread)
            {
                _owningThread = null; // Reset ownership before releasing
                Mutex.ReleaseMutex();
            }
            else
            {
                Console.WriteLine("Warning: Mutex was not owned by the current thread.");
            }
        }
    }
}