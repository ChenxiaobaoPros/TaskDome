using System;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleAppTask
{
    //学习网站https://www.cnblogs.com/zhaoshujie/p/11082753.html
    //https://docs.microsoft.com/zh-cn/dotnet/api/system.threading.tasks?view=netframework-4.8
    class Program
    {
        //Task 初始化
        static void Main(string[] args)
        {
            #region 经典案例
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
                Console.WriteLine("任务完成，完成时候的状态为："+ t.Status);
                Console.WriteLine("IsCanceled={0}\tIsCompleted={1}\tIsFaulted={2}", task.IsCanceled, task.IsCompleted, task.IsFaulted);
            });
            #endregion

            #region 创建任务-无返回值(写法)
            //[1]
            var t1 = new Task(() => TaskMethod("Task 1"));
            t1.Start();
            Task.WaitAll(t1);//等待所有任务结束 
            //注: 任务的状态:
            //Start之前为: Created
            //Start之后为:WaitingToRun
            //[2]
            Task.Run(() => TaskMethod("Task 2"));
            //[3]
            Task.Factory.StartNew(() => TaskMethod("Task 3")); //直接异步的方法
            //或者
            var t3 = Task.Factory.StartNew(() => TaskMethod("Task 3"));
            //等待所有任务结束
            Task.WaitAll(t3);
            //任务的状态:
            //Start之前为: Running
            //Start之后为:Running
            #endregion

            Console.ReadKey();
        }

        private static void TaskMethod(string v)
        {
            throw new NotImplementedException();
        }
    }
}
