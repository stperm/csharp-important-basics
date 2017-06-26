using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Tests
{
    /// <summary>
    /// Tests showing the basic functionality of TPL
    /// </summary>
    [TestFixture]
    public class TPLTests
    {        
        [Test]
        public void TaskRun_Used_ForInvokingAnActionWithoutResult()
        {
            var sharedValue = 0;
            var task = Task.Run(() => { sharedValue++; });
            task.Wait();

            Assert.AreEqual(1, sharedValue);
        }

        [Test]
        public void TaskRun_Used_ForInvokingFuncReturningValue()
        {
            var task = Task.Run(() => { return 5; });
            Assert.AreEqual(5, task.Result);
        }

        /// <summary>
        /// Using CancellationToken to cancel long running task
        /// </summary>
        [Test]
        public void Using_CancellationToken_ToCancelOperation()
        {
            var sharedValue = 0;
            var cts = new CancellationTokenSource();
            var token = cts.Token;
            var task = new Task(() =>
            {
                for(;;)
                {
                    sharedValue++;
                    Task.Delay(30).Wait();
                    token.ThrowIfCancellationRequested();
                }
            }, token); // passing token here only needed for Task to understand if it was cancelled

            task.Start();
            cts.CancelAfter(100);
            var exception = Assert.Throws<AggregateException>(() => task.Wait());
            Assert.IsInstanceOf<OperationCanceledException>(exception.InnerExceptions[0]);
            Assert.IsTrue(task.IsCanceled); // here we won't see true, if not pass the token to the task
        }

        [Test]
        public void UsinngSimpleContinuation()
        {
            var task = Task.Run(() => { return 42; }).
                ContinueWith(t => t.Result * 2).
                ContinueWith(t => t.Result.ToString());
            Assert.AreEqual("84", task.Result);
        }

        /// <summary>
        /// Using continuation for task that failed
        /// </summary>
        [Test]
        public void Using_Contionuation_OnFaulted()
        {
            var task = new Task(() => { throw new Exception(); });
            var continuation = task.ContinueWith((t) => { return "ouch!"; }, TaskContinuationOptions.OnlyOnFaulted);
            task.Start();
            Assert.AreEqual("ouch!", continuation.Result);
        }

        /// <summary>
        /// Using continuation for task that was cancelled
        /// </summary>
        [Test]
        public void Using_Contionuation_OnCancelled()
        {
            var cts = new CancellationTokenSource();
            var token = cts.Token;
            var task = new Task(() => 
            {
                Thread.Sleep(50);
                token.ThrowIfCancellationRequested();
            }, token);
            var continuation = task.ContinueWith((t) => { return "cancelled"; }, TaskContinuationOptions.OnlyOnCanceled);
            task.Start();
            cts.Cancel();
            Assert.AreEqual("cancelled", continuation.Result);
        }

        /// <summary>
        /// Using TaskFactory to create a few tasks with same options.
        /// Using tasks attached to parent to complete the main task when all children finish.
        /// </summary>
        [Test]
        public void Using_TasksAttachedToParent_And_TaskFactory()
        {
            var task = Task.Run(() =>
            {
                var result = new int[3];                
                var factory = new TaskFactory(TaskCreationOptions.AttachedToParent, TaskContinuationOptions.ExecuteSynchronously);

                for (int i = 0; i < 3; i++)
                {
                    var tmp = i;
                    factory.StartNew(() => result[tmp] = tmp + 1);
                }
                return result;
            });

            var end = task.ContinueWith(t => t.Result.Sum());

            Assert.AreEqual(6, end.Result);
        }        

        [Test]
        public void Using_WaitAll_ToWaitForCompletion()
        {
            var counter = 0;
            var tasks = new Task[3];
            for (int i = 0; i < 3; i++)
            {
                var tmp = i;
                tasks[i] = Task.Run(() =>
                {
                    Thread.Sleep(10);
                    Interlocked.Increment(ref counter);
                });
            }

            Task.WaitAll(tasks);
            Assert.AreEqual(3, counter);
        }

        [Test]
        public void Using_WaitAny_ToGetCompletedTasksOneByOne()
        {
            var taskCount = 3;
            var tasks = new List<Task>(taskCount);
            for (int i = 0; i < taskCount; i++)
            {
                var tmp = i;
                tasks.Add(Task.Run(() =>
                {
                    Thread.Sleep((4 - tmp + 1) * 100);                    
                }));
            }

            var completedList = new List<int>(3);

            while (tasks.Count > 0)
            {
                var index = Task.WaitAny(tasks.ToArray());
                completedList.Add(index);
                tasks.RemoveAt(index);
            }

            // Completed in reverse order
            Assert.AreEqual("2,1,0", string.Join(",", completedList));
        }
    }
}