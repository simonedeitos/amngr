using System;

namespace AirManager.Services
{
    /// <summary>
    /// Configurazione di una singola emittente
    /// </summary>
    public class StationConfig
    {
        public string Id { get; set; } // ✅ GUID univoco
        public string Name { get; set; } // ✅ Nome emittente
        public string LogoPath { get; set; } // ✅ Path logo PNG
        public string DatabasePath { get; set; } // ✅ Path database (locale o rete)
        public DateTime CreatedDate { get; set; }
        public DateTime LastAccessed { get; set; }

        public StationConfig()
        {
            Id = Guid.NewGuid().ToString();
            CreatedDate = DateTime.Now;
            LastAccessed = DateTime.Now;
        }

        public StationConfig(string name, string logoPath, string databasePath)
        {
            Id = Guid.NewGuid().ToString();
            Name = name;
            LogoPath = logoPath;
            DatabasePath = databasePath;
            CreatedDate = DateTime.Now;
            LastAccessed = DateTime.Now;
        }

        public override string ToString()
        {
            return $"{Name} ({DatabasePath})";
        }
    }
}