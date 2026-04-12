using System;

namespace AirManager.Models
{
    [Serializable]
    public class DragDropData
    {
        public string EntryType { get; set; }
        public object EntryData { get; set; }
    }
}
