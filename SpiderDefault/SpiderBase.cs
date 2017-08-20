﻿using Helper;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SpiderDefault
{
    public abstract class SpiderBase<T> : ISpider<T> where T : class
    {
        public virtual bool AutoStart { get { return false; } }

        public bool Running { get; set; }
        public int Wait { get; set; }
        public int ThreadNumber { get; set; }
        public int MaxItemProcessing { get; set; }
        public SpiderMode Mode { get; set; }

        public abstract string ID { get; }

        private CancellationTokenSource Token;
        private SafeList<Tuple<Task, CancellationTokenSource>> TaskList = new SafeList<Tuple<Task, CancellationTokenSource>>();
        protected Log logger;

        public SpiderBase()
        {
            Token = new CancellationTokenSource();
            Wait = 1000;
            ThreadNumber = 5;
            MaxItemProcessing = 10;
            Running = false;
            Mode = SpiderMode.All;
            logger = new Log("SpiderBase");
        }

        public abstract IEnumerable<T> Capture(long? jobSchedulerID);

        public abstract T Load(long? jobSchedulerID);

        public abstract void Persist(IEnumerable<T> itens);

        public void Start()
        {
            if (!Running)
            {
                Token = new CancellationTokenSource();
                Running = true;
                logger.Info("Starting");

                Task.Factory.StartNew(() =>
                {
                    while (true)
                    {
                        Token.Token.ThrowIfCancellationRequested();

                        if (Mode == SpiderMode.All)
                        {
                            RunJob();
                            RunBatch();
                        }
                        else if (Mode == SpiderMode.BatchOnly)
                            RunBatch();
                        else if (Mode == SpiderMode.JobOnly)
                            RunJob();              

                        Thread.Sleep(Wait);
                    }
                });
            }
        }

        public void Restart()
        {
            logger.Info("Restarting");
            Stop();
            Start();
        }

        public void Stop()
        {
            if (Running)
            {
                logger.Info("Stoping");
                Token.Cancel();

                foreach (var item in TaskList.All())
                    item.Item2.Cancel();

                Running = false;
            }
        }

        public void ChangeWait(int n)
        {
            Wait = n;
        }

        public void ChangeThreadNumber(int n)
        {
            ThreadNumber = n; ;
        }

        public void ChangeMaxItemProcessing(int n)
        {
            MaxItemProcessing = n;
        }

        public void ChangeMode(SpiderMode mode)
        {
            Mode = mode;
        }

        public string GetStatus()
        {
            return "teste";
        }

        private void RunBatch()
        {
            TaskList.Remove(p => p.Item1 != null && (p.Item1.IsCompleted || p.Item1.IsCanceled || p.Item1.IsFaulted));

            int i = TaskList.Count();

            for (; i < ThreadNumber; i++)
            {
                CancellationTokenSource source = new CancellationTokenSource();
                CancellationToken token = source.Token;

                Task t = Task.Factory.StartNew(() =>
                {
                    foreach (var item in Capture(null))
                    {
                        token.ThrowIfCancellationRequested();
                    }
                }, token);

                Tuple<Task, CancellationTokenSource> tuple = Tuple.Create<Task, CancellationTokenSource>(t, source);

                TaskList.Add(tuple);
            }
        }

        private void RunJob()
        {
            long[] jobsID = new long[] { 1, 2 };

            foreach(long jobID in jobsID)
            {
                TaskList.Remove(p => p.Item1 != null && (p.Item1.IsCompleted || p.Item1.IsCanceled || p.Item1.IsFaulted));

                int i = TaskList.Count();

                if (i == ThreadNumber)
                    break;

                CancellationTokenSource source = new CancellationTokenSource();
                CancellationToken token = source.Token;

                Task t = Task.Factory.StartNew(() =>
                {
                    foreach (var item in Capture(jobID))
                    {
                        token.ThrowIfCancellationRequested();
                    }
                }, token);

                Tuple<Task, CancellationTokenSource> tuple = Tuple.Create<Task, CancellationTokenSource>(t, source);

                TaskList.Add(tuple);
            }
        }
    }
}
