using System;
using System.Collections.Generic;

namespace Dragona_Data_Editor
{
    [Serializable]
    public class Export
    {
        public int ListId = 0;
        public int itemID = 0;
        public int ForVersion = 0;
        public int type = 0;
        public SortedDictionary<int, object> data = null;
    }
}
