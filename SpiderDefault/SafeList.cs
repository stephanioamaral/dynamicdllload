using System;
using System.Collections.Generic;
using System.Linq;

namespace SpiderDefault
{
    public class SafeList<T>
    {
        private List<T> _list = new List<T>();
        private object _sync = new object();

        public int Count()
        {
            lock (_sync)
            {
               return _list.Count();
            }
        }

        public void Add(T value)
        {
            lock (_sync)
            {
                _list.Add(value);
            }
        }

        public void Remove(Predicate<T> predicate)
        {
            lock (_sync)
            {
                _list.RemoveAll(predicate);
            }
        }

        public List<T> All()
        {
            lock (_sync)
            {
                return _list;
            }
        }

        public List<T> Query(Func<T, bool> predicate)
        {
            lock (_sync)
            {
                return _list.Where(predicate).ToList();
            }
        }

        public T Find(Predicate<T> predicate)
        {
            lock (_sync)
            {
                return _list.Find(predicate);
            }
        }

        public T FirstOrDefault()
        {
            lock (_sync)
            {
                return _list.FirstOrDefault();
            }
        }
    }
}
