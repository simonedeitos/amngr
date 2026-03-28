using System;

namespace AirManager.Services
{
    /// <summary>
    /// Tipo di emittente: Radio o RadioTV
    /// </summary>
    public enum StationType
    {
        Radio,
        RadioTV
    }

    /// <summary>
    /// Configurazione di una singola emittente
    /// </summary>
    public class StationConfig
    {
        public string Id { get; set; } // ✅ GUID univoco
        public string Name { get; set; } // ✅ Nome emittente
        public string LogoPath { get; set; } // ✅ Path logo PNG
        public string DatabasePath { get; set; } // ✅ Path database (locale o rete)
        public StationType StationType { get; set; } // ✅ Tipo emittente (Radio / RadioTV)
        public DateTime CreatedDate { get; set; }
        public DateTime LastAccessed { get; set; }

        public StationConfig()
        {
            Id = Guid.NewGuid().ToString();
            StationType = StationType.Radio;
            CreatedDate = DateTime.Now;
            LastAccessed = DateTime.Now;
        }

        public StationConfig(string name, string logoPath, string databasePath)
        {
            Id = Guid.NewGuid().ToString();
            Name = name;
            LogoPath = logoPath;
            DatabasePath = databasePath;
            StationType = StationType.Radio;
            CreatedDate = DateTime.Now;
            LastAccessed = DateTime.Now;
        }

        public override string ToString()
        {
            return $"{Name} [{StationType}] ({DatabasePath})";
        }
    }
}