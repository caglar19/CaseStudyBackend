namespace CaseStudy.Application.Constants
{
    public static class LeagueConstants
    {
        public static readonly HashSet<string> SelectedCountries = new()
        {
            "Turkey",      // Türkiye
            "Germany",     // Almanya
            "England",     // İngiltere
            "France",      // Fransa
            "Italy",       // İtalya
            "Portugal",    // Portekiz
            "Spain"        // İspanya
        };

        public static readonly Dictionary<string, HashSet<string>> SelectedLeagues = new()
        {
            {
                "Turkey", new HashSet<string>
                {
                    "Süper Lig"
                }
            },
            {
                "Germany", new HashSet<string>
                {
                    "Bundesliga"
                }
            },
            {
                "England", new HashSet<string>
                {
                    "Premier League"
                }
            },
            {
                "France", new HashSet<string>
                {
                    "Ligue 1"
                }
            },
            {
                "Italy", new HashSet<string>
                {
                    "Serie A"
                }
            },
            {
                "Portugal", new HashSet<string>
                {
                    "Primeira Liga"
                }
            },
            {
                "Spain", new HashSet<string>
                {
                    "La Liga"
                }
            }
        };
    }
}
