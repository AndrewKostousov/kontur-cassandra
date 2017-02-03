using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Commons.Logging;
using FluentAssertions;

namespace Commons.Testing
{
    public static class MultithreadingTestHelper
    {
        public static void RunOnSeparateThreads(TimeSpan timeout, params Action<SharedState>[] actions)
        {
            RunOnSeparateThreads(new SharedState(), timeout, actions);
        }

        public static void RunOnSeparateThreads<TSharedState>(TSharedState sharedState, TimeSpan timeout, IEnumerable<Action<TSharedState>> actions) where TSharedState : SharedState
        {
            var threads = actions.Select((a, threadId) => CreateThread(sharedState, a, threadId.ToString())).ToList();
            foreach(var t in threads)
                t.Start();
            foreach(var t in threads)
            {
                t.Join(timeout).Should().BeTrue("Thread did not terminate in: {0}", timeout);
                sharedState.Errors.Should().BeEmpty();
            }
        }

        private static Thread CreateThread<TSharedState>(TSharedState sharedState, Action<TSharedState> action, string threadId) where TSharedState : SharedState
        {
            var threadName = string.Format("test-{0}", threadId);
            return new Thread(() =>
            {
                try
                {
                    action(sharedState);
                }
                catch(Exception e)
                {
                    sharedState.Errors.Add(e);
                    Log.For<SharedState>().Error(string.Format("Unhandled exception on test thread {0}", threadName), e);
                }
            })
                {
                    IsBackground = true,
                    Name = threadName,
                };
        }

        public class SharedState
        {
            public SharedState()
            {
                Errors = new ConcurrentBag<Exception>();
            }

            public ConcurrentBag<Exception> Errors { get; private set; }
        }
    }
}