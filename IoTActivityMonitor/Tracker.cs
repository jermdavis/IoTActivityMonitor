using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTActivityMonitor
{

    public sealed class Tracker
    {
        private object _lock = new object();
        private int _currentTick = 0;
        private int _historyCount;

        private Queue<int> _queue = new Queue<int>();

        public Tracker(int historyCount)
        {
            _historyCount = historyCount;

            for (int i = 0; i < _historyCount; i++)
            {
                _queue.Enqueue(0);
            }
        }

        public IEnumerable<int> Tick()
        {
            lock (_lock)
            {
                _queue.Enqueue(_currentTick);
                _currentTick = 0;

                if (_queue.Count >= _historyCount)
                {
                    _queue.Dequeue();
                }

                return _queue.ToList();
            }
        }

        public void Track(int value)
        {
            lock (_lock)
            {
                _currentTick += value;
            }
        }
    }

}