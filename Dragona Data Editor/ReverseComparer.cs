using System;
using System.Collections.Generic;

namespace Dragona_Data_Editor
{
    [Serializable]
    public sealed class ReverseComparer<T> : IComparer<T>
    {
        private readonly IComparer<T> original;

        public ReverseComparer(IComparer<T> original)
        {
            // TODO: Validation
            this.original = original;
        }

        public int Compare(T left, T right)
        {
            return original.Compare(right, left);
        }
    }
}
