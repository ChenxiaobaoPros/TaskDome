using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleAppTask
{
    //学习网站https://www.cnblogs.com/zhaoshujie/p/11082753.html
    //https://docs.microsoft.com/zh-cn/dotnet/api/system.threading.tasks?view=netframework-4.8
    class Program
    {
        #region 经典案例
        static void Main2(string[] args)
        {

            Task t = new Task(() =>
            {
                Console.WriteLine("任务开始工作……");
                //模拟工作过程
                Thread.Sleep(5000);
            });
            t.Start();
            //可等待当t执行完后执行task
            t.ContinueWith((task) =>
            {
                Console.WriteLine("任务完成，完成时候的状态为：" + t.Status);
                Console.WriteLine("IsCanceled={0}\tIsCompleted={1}\tIsFaulted={2}", task.IsCanceled, task.IsCompleted, task.IsFaulted);
            });

            Console.ReadKey();
        }
        #endregion

        #region 初始化
        #region Task(Action) 
        /*下面的示例使用Task(Action)构造函数创建检索指定目录中的文件名的任务。 所有任务将文件名写入单个ConcurrentBag<T> 对象。 然后，该示例调用WaitAll(Task[])方法以确保所有任务都已完成，然后显示写入ConcurrentBag<T> 对象的文件名称总数的计数*/
        public static async Task Main0()
        {
            //线程安全集合
            var list = new ConcurrentBag<string>();
            string[] dirNames = { ".", ".." };
            List<Task> tasks = new List<Task>();
            foreach (var dirName in dirNames)
            {
                Task t = new Task(() =>
                {
                    foreach (var path in Directory.GetFiles(dirName))
                        list.Add(path);
                });
                tasks.Add(t);
                t.Start();
            }
            await Task.WhenAll(tasks.ToArray());
            foreach (Task t in tasks)
                Console.WriteLine("Task {0} Status: {1}", t.Id, t.Status);

            Console.WriteLine("Number of files read: {0}", list.Count);
           
        }
        #endregion

        #region Task(Action) 
        /*下面的示例是相同的，只不过它使用Run(Action)方法实例化并在单个操作中运行任务。 方法返回Task表示任务的对象。*/
        public static void Main1()
        {
            var list = new ConcurrentBag<string>();
            string[] dirNames = { ".", ".." };
            List<Task> tasks = new List<Task>();
            foreach (var dirName in dirNames)
            {
                Task t = Task.Run(() => {
                    foreach (var path in Directory.GetFiles(dirName))
                        list.Add(path);
                });
                tasks.Add(t);
            }
            Task.WaitAll(tasks.ToArray());
            foreach (Task t in tasks)
                Console.WriteLine("Task {0} Status: {1}", t.Id, t.Status);

            Console.WriteLine("Number of files read: {0}", list.Count);
        }
        #endregion

        #region Task(Action, CancellationToken) 
        /*下面的示例调用Task(Action, CancellationToken)构造函数来创建一个用于循环访问 C:\Windows\System32 目录中的文件的任务。 Lambda 表达式调用Parallel.ForEach方法，将有关每个文件的List<T>信息添加到对象。 Parallel.ForEach循环调用的每个分离的嵌套任务会检查取消标记的状态，如果请求取消，则CancellationToken.ThrowIfCancellationRequested调用方法。 当调用线程调用OperationCanceledException catch CancellationToken.ThrowIfCancellationRequested 方法Task.Wait时，方法将引发在块中处理的异常。 然后Start调用方法以启动任务。*/
        public static async Task Main()
        {
            var tokenSource = new CancellationTokenSource();
            var token = tokenSource.Token;
            var files = new List<Tuple<string, string, long, DateTime>>();

            var t = new Task(() => {
                string dir = "C:\\Windows\\System32\\";
                object obj = new Object();
                if (Directory.Exists(dir))
                {
                    Parallel.ForEach(Directory.GetFiles(dir),
                    f => {
                        if (token.IsCancellationRequested)
                            token.ThrowIfCancellationRequested();
                        var fi = new FileInfo(f);
                        lock (obj)
                        {
                            files.Add(Tuple.Create(fi.Name, fi.DirectoryName, fi.Length, fi.LastWriteTimeUtc));
                        }
                    });
                }
            }, token);
            t.Start();
            tokenSource.Cancel();
            try
            {
                await t;
                Console.WriteLine("Retrieved information for {0} files.", files.Count);
            }
            catch (AggregateException e)
            {
                Console.WriteLine("Exception messages:");
                foreach (var ie in e.InnerExceptions)
                    Console.WriteLine("   {0}: {1}", ie.GetType().Name, ie.Message);

                Console.WriteLine("\nTask status: {0}", t.Status);
            }
            finally
            {
                tokenSource.Dispose();
            }
        }

        #endregion

        #endregion

    }
}
