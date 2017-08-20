using Helper;
using SpiderDefault;
using System;
using System.Collections.Generic;
using System.Threading;

namespace SpiderTeste2
{
    public class Teste2 : SpiderBase<string>
    {
        public override bool AutoStart { get { return true; } }
        public override string ID { get => "2"; }

        public Teste2() : base()
        {
            ThreadNumber = 2;
            MaxItemProcessing = 5;
            logger = new Log("Teste2");
        }

        public override IEnumerable<string> Capture(long? jobSchedulerID)
        {
            logger.Info($"Capturing {jobSchedulerID ?? null} [Thread {Thread.CurrentThread.ManagedThreadId}]");

            Thread.Sleep(5000);

            for (int i = 0; i < MaxItemProcessing; i++)
            {
                yield return Guid.NewGuid().ToString();
            }
        }

        public override string Load(long? jobSchedulerID)
        {
            return null;
        }

        public override void Persist(IEnumerable<string> itens)
        {

        }
    }
}
