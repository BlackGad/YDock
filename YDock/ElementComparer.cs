using System;
using System.Collections.Generic;

namespace YDock
{
    public class ElementComparer<T> : IComparer<T>
    {
        private readonly Func<T, T, int> _comparer;

        #region Constructors

        public ElementComparer(Func<T, T, int> comparer)
        {
            _comparer = comparer;
        }

        #endregion

        #region IComparer<T> Members

        public int Compare(T x, T y)
        {
            return _comparer(x, y);
        }

        #endregion
    }
}