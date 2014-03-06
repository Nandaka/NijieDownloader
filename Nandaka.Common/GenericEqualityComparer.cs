using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nandaka.Common
{
    /// <summary>
    /// http://stackoverflow.com/a/10720211
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class EqualityComparer<T> : IEqualityComparer<T>
    {
        public EqualityComparer(Func<T, T, bool> cmp)
        {
            this.cmp = cmp;
        }
        public bool Equals(T x, T y)
        {
            return cmp(x, y);
        }

        public int GetHashCode(T obj)
        {
            return obj.GetHashCode();
        }

        private Func<T, T, bool> cmp { get; set; }
    }
}
