using Helper;
using SpiderDefault;
using System;
using System.Collections.Generic;
using System.Threading;

namespace SpiderTeste3
{
    public class Teste3 : SpiderBase<string>
    {
        public override bool AutoStart { get { return true; } }
        public override string ID { get => "3"; }

        public Teste3() : base()
        {
            ThreadNumber = 3;
            MaxItemProcessing = 15;
            logger = new Log("Teste3");
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
