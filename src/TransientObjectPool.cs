using System;
using System.Collections.Generic;
using System.Linq;

namespace Unity.Microsoft.DependencyInjection
{
    public class TransientObjectPool : IDisposable
    {
        private List<object> objects = new List<object>();

        public void Add(object newValue)
        {
            objects.Add(newValue);
        }

        public void Dispose()
        {
            foreach (IDisposable disposable in objects
                .Select(o => o as IDisposable)
                //.Where(o => null != o)
                .Reverse())
            {
                disposable.Dispose();
            }

            objects.Clear();
            objects = null;
            GC.SuppressFinalize(this);
        }
    }
}
