using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmotionApp
{
    class HelpUtils
    {
        public static string getAgeString(string ageAffedex)
        {
            string idade = "";
            if (ageAffedex.Contains("18_24"))
            {
                idade = "De 18 há 24 anos";
            }
            else if (ageAffedex.Contains("18"))
            {
                idade = "Abaixo de 18 anos";
            }
            else if (ageAffedex.Contains("25_34"))
            {
                idade = "De 25 há 34 anos";
            }
            else if (ageAffedex.Contains("35_44"))
            {
                idade = "De 35 há 44 anos";
            }
            else if (ageAffedex.Contains("45_54"))
            {
                idade = "De 45 há 54 anos";
            }
            else if (ageAffedex.Contains("55_64"))
            {
                idade = "De 55 há 64 anos";
            }
            else if (ageAffedex.Contains("65"))
            {
                idade = "Mais de 65 anos";
            }
            else
            {
                idade = "Não foi possível recuperar a idade";
            }

            return idade;
        }

        public static string getMetricString(string metricAffedex)
        {
            string metrica = "";
            if (metricAffedex.Equals("Smile"))
            {
                metrica = "Sorriso";
            }
            else if (metricAffedex.Equals("Smirk"))
            {
                metrica = "Sorriso desdenhoso";
            }
            else if (metricAffedex.Equals("Fear"))
            {
                metrica = "Medo";
            }
            else if (metricAffedex.Equals("Surprise"))
            {
                metrica = "Surpresa";
            }
            else if (metricAffedex.Equals("Digust"))
            {
                metrica = "Nojo";
            }
            else if (metricAffedex.Equals("Anger"))
            {
                metrica = "Raiva";
            }
            else if (metricAffedex.Equals("Sadness"))
            {
                metrica = "Tristeza";
            }

            return metrica;
        }
    }
}
