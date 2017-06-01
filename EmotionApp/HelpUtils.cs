using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmotionApp
{
    class HelpUtils
    {
        public static string getAgeString(string ageAffedex, int comboBoxValue)
        {
            string idade = "";
            if (ageAffedex.Contains("18_24"))
            {
                int i1 = 18 + comboBoxValue;
                int i2 = 24 + comboBoxValue;
                idade = "De " + i1.ToString() + " a " + i2.ToString() + " anos";
            }
            else if (ageAffedex.Contains("18"))
            {
                idade = "Abaixo de 18 anos";
            }
            else if (ageAffedex.Contains("25_34"))
            {
                int i1 = 25 + comboBoxValue;
                int i2 = 34 + comboBoxValue;
                idade = "De " + i1.ToString() + " a " + i2.ToString() + " anos";
            }
            else if (ageAffedex.Contains("35_44"))
            {
                int i1 = 35 + comboBoxValue;
                int i2 = 44 + comboBoxValue;
                idade = "De " + i1.ToString() + " a " + i2.ToString() + " anos";
            }
            else if (ageAffedex.Contains("45_54"))
            {
                int i1 = 45 + comboBoxValue;
                int i2 = 54 + comboBoxValue;
                idade = "De " + i1.ToString() + " a " + i2.ToString() + " anos";
            }
            else if (ageAffedex.Contains("55_64"))
            {
                int i1 = 55 + comboBoxValue;
                int i2 = 64 + comboBoxValue;
                idade = "De " + i1.ToString() + " a " + i2.ToString() + " anos";
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
            if (metricAffedex.Equals("Joy"))
            {
                metrica = "Alegria";
            }
            else if (metricAffedex.Equals("Fear"))
            {
                metrica = "Medo";
            }
            else if (metricAffedex.Equals("Surprise"))
            {
                metrica = "Surpresa";
            }
            else if (metricAffedex.Equals("Disgust"))
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
