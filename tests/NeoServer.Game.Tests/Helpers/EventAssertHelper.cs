using System;
using System.Linq;
using System.Linq.Expressions;
using FluentAssertions.Events;
using FluentAssertions.Execution;

namespace NeoServer.Game.Tests.Helpers;

public static class EventAssertHelper
{
    public static void WithArgs<T>(this IEventRecording eventRecording, Expression<Func<T, bool>> predicate)
    {
        var occurredEvent = eventRecording.FirstOrDefault();
        RunPredicate(predicate, (T)occurredEvent?.Parameters[0]);
    }

    public static void WithArgs<T1, T2>(this IEventRecording eventRecording, Expression<Func<T1, bool>> predicate1,
        Expression<Func<T2, bool>> predicate2)
    {
        var occurredEvent = eventRecording.FirstOrDefault();
        RunPredicate(predicate1, (T1)occurredEvent?.Parameters[0]);
        RunPredicate(predicate2, (T2)occurredEvent?.Parameters[1]);
    }

    public static void WithArgs<T1, T2, T3>(this IEventRecording eventRecording, Expression<Func<T1, bool>> predicate1,
        Expression<Func<T2, bool>> predicate2,
        Expression<Func<T3, bool>> predicate3)
    {
        var occurredEvent = eventRecording.FirstOrDefault();
        RunPredicate(predicate1, (T1)occurredEvent?.Parameters[0]);
        RunPredicate(predicate2, (T2)occurredEvent?.Parameters[1]);
        RunPredicate(predicate3, (T3)occurredEvent?.Parameters[2]);
    }

    public static void WithArgs<T1, T2, T3, T4>(this IEventRecording eventRecording,
        Expression<Func<T1, bool>> predicate1,
        Expression<Func<T2, bool>> predicate2,
        Expression<Func<T3, bool>> predicate3,
        Expression<Func<T4, bool>> predicate4)
    {
        var occurredEvent = eventRecording.FirstOrDefault();
        RunPredicate(predicate1, (T1)occurredEvent?.Parameters[0]);
        RunPredicate(predicate2, (T2)occurredEvent?.Parameters[1]);
        RunPredicate(predicate3, (T3)occurredEvent?.Parameters[2]);
        RunPredicate(predicate4, (T4)occurredEvent?.Parameters[3]);
    }

    public static void WithArgs<T1, T2, T3, T4, T5>(this IEventRecording eventRecording,
        Expression<Func<T1, bool>> predicate1,
        Expression<Func<T2, bool>> predicate2,
        Expression<Func<T3, bool>> predicate3,
        Expression<Func<T4, bool>> predicate4,
        Expression<Func<T5, bool>> predicate5)
    {
        var occurredEvent = eventRecording.FirstOrDefault();
        RunPredicate(predicate1, (T1)occurredEvent?.Parameters[0]);
        RunPredicate(predicate2, (T2)occurredEvent?.Parameters[1]);
        RunPredicate(predicate3, (T3)occurredEvent?.Parameters[2]);
        RunPredicate(predicate4, (T4)occurredEvent?.Parameters[3]);
        RunPredicate(predicate5, (T5)occurredEvent?.Parameters[4]);
    }

    private static void RunPredicate<T>(Expression<Func<T, bool>> predicate, T value)
    {
        if (predicate.Compile().Invoke(value) is false)
            AssertionChain.GetOrCreate()
                .FailWith("Expected event with argument of type <{0}> that matches {1}, but found none.",
                    typeof(T),
                    predicate.Body);

        // Execute.Assertion
        //     .FailWith("Expected event with argument of type <{0}> that matches {1}, but found none.",
        //         typeof(T),
        //         predicate.Body);
    }
}