using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.Threading;

namespace ReadySetTarkov.Utility
{
    internal static class ThreadHelper
    {
        private static JoinableTaskContext? s_joinableTaskContextCache;

        public static JoinableTaskContext JoinableTaskContext
        {
            get
            {
                if (s_joinableTaskContextCache is null)
                {
                    s_joinableTaskContextCache = App.GlobalProvider.GetRequiredService<JoinableTaskContext>();
                }

                return s_joinableTaskContextCache;
            }
        }

        public static JoinableTaskFactory JoinableTaskFactory => JoinableTaskContext.Factory;
    }
}
