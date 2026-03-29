using System;

namespace AirManager.Models
{
    /// <summary>
    /// Elemento di un Clock: rappresenta una categoria o genere con filtri
    /// </summary>
    public class ClockItem
    {
        /// <summary>
        /// Tipo di elemento (es. "Music_Category", "Music_Genre", "Clips_Category", ecc.)
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Nome della categoria o genere
        /// </summary>
        public string CategoryName { get; set; }

        /// <summary>
        /// Valore (alias per CategoryName per compatibilità)
        /// </summary>
        public string Value
        {
            get => CategoryName;
            set => CategoryName = value;
        }

        /// <summary>
        /// Numero di brani da estrarre (usato nella generazione playlist)
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// Se true, applica il filtro anni
        /// </summary>
        public bool YearFilterEnabled { get; set; }

        /// <summary>
        /// Anno minimo (se YearFilterEnabled = true)
        /// </summary>
        public int YearFrom { get; set; }

        /// <summary>
        /// Anno massimo (se YearFilterEnabled = true)
        /// </summary>
        public int YearTo { get; set; }

        public ClockItem()
        {
            Type = "Music_Category";
            CategoryName = string.Empty;
            Count = 1;
            YearFilterEnabled = false;
            YearFrom = 1900;
            YearTo = DateTime.Now.Year;
        }

        public ClockItem(string type, string value, bool yearFilter = false, int yearFrom = 1900, int yearTo = 0)
        {
            Type = type;
            CategoryName = value;
            Count = 1;
            YearFilterEnabled = yearFilter;
            YearFrom = yearFrom;
            YearTo = yearTo > 0 ? yearTo : DateTime.Now.Year;
        }

        public override string ToString()
        {
            string filter = YearFilterEnabled ? $" [{YearFrom}-{YearTo}]" : "";
            return $"{CategoryName}{filter}";
        }
    }
}
