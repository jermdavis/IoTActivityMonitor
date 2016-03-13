using Emmellsoft.IoT.Rpi.SenseHat;
using System;
using System.Linq;
using Windows.ApplicationModel.Background;
using Windows.System.Threading;
using Windows.UI;

namespace IoTActivityMonitor
{

    public sealed class StartupTask : IBackgroundTask
    {
        private BackgroundTaskDeferral _deferral;
        private Tracker _tracker = new Tracker(8);
        private HttpReceiver _receiver;
        private ThreadPoolTimer _timer;
        private ISenseHat _hat;

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            _hat = SenseHatFactory.Singleton.GetSenseHat().Result;

            _deferral = taskInstance.GetDeferral();

            _receiver = new HttpReceiver(1234);
            _receiver.DataReceived += webMessage;
            _receiver.Start();

            _timer = ThreadPoolTimer.CreatePeriodicTimer(_timer_Tick, TimeSpan.FromSeconds(1));

            _hat.Display.Clear();
            _hat.Display.Update();
        }

        private void webMessage(HttpReceiver sender, string data)
        {
            int value = 1;
            if (!string.IsNullOrWhiteSpace(data))
            {
                int.TryParse(data, out value);
            }
            _tracker.Track(value);
        }

        private void _timer_Tick(ThreadPoolTimer timer)
        {
            var ints = _tracker.Tick()
                .Reverse()
                .Scale(8)
                .ToArray();

            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    Color c = colourFor(y, ints[x]);
                    _hat.Display.Screen[y, x] = c;
                }
            }

            _hat.Display.Update();
        }

        private Color colourFor(int y, int value)
        {
            if(y < value -1)
            {
                return Colors.Green;
            }
            else if(y < value)
            {
                return Colors.Red;
            }
            else
            {
                return Colors.Black;
            }
        }
    }

}