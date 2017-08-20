using System.Collections.Generic;

namespace SpiderDefault
{
    public interface ISpider<T> where T : class
    {
        string ID { get; }

        bool AutoStart { get; }
        bool Running { get; set; }
        SpiderMode Mode { get; set; }

        int ThreadNumber { get; set; }
        int MaxItemProcessing { get; set; }
        int Wait { get; set; }

        void Start();
        void Restart();
        void Stop();
        string GetStatus();

        void ChangeWait(int n);
        void ChangeThreadNumber(int n);
        void ChangeMaxItemProcessing(int n);
        void ChangeMode(SpiderMode mode);

        IEnumerable<T> Capture(long? jobSchedulerID);
        void Persist(IEnumerable<T> itens);
        T Load(long? jobSchedulerID);
    }

    public enum SpiderMode
    {
        All = 0,
        BatchOnly = 1,
        JobOnly = 2
    }
}
