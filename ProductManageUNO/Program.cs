using System;
using System.Threading;

// Các using này chỉ được dùng trên Windows
#if WINDOWS
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
#endif

namespace ProductManageUNO
{
#if WINDOWS
    public static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Application.Start((p) =>
            {
                var context = new DispatcherQueueSynchronizationContext(
                    DispatcherQueue.GetForCurrentThread());
                SynchronizationContext.SetSynchronizationContext(context);
                new App();
            });
        }
    }
#endif
}
