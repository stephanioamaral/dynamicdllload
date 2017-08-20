using Helper;
using SpiderDefault;
using System;
using System.Collections.Generic;
using System.Threading;

namespace SpiderTeste1
{
    public class Teste1 : SpiderBase<string>
    {
        public override bool AutoStart { get { return true; } }
        public override string ID { get => "1"; }

        public Teste1() : base()
        {
            ThreadNumber = 4;
            MaxItemProcessing = 10;
            logger = new Log("Teste1");
        }

        public override IEnumerable<string> Capture(long? jobSchedulerID)
        {
            logger.Info($"Capturing {jobSchedulerID ?? null} [Thread {Thread.CurrentThread.ManagedThreadId}]");

            Thread.Sleep(5000);

            for (int i = 0; i< MaxItemProcessing; i++)
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
