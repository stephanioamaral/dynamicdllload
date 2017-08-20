using NLog;

namespace Helper
{
    public class Log
    {
        private Logger logger;

        public Log(string name)
        {
            logger = LogManager.GetLogger(name);
        }

        public void Info(string msg)
        {
            logger.Info(msg);
        }

        public void Trace(string msg)
        {
            logger.Trace(msg);
        }

        public void Error(string msg)
        {
            logger.Error(msg);
        }
    }
}
