﻿
using Microsoft.Xna.Framework.Input;

namespace ClimateOfFerngill
{
    public class ClimateConfig
    {
        public bool TooMuchInfo { get; set; }

        public Keys Keyboard { get; set; }
        public Buttons Controller { get; set; }

        public bool AllowSnowOnFall28 { get; set; }
        public bool AllowStormsFirstSpring { get; set; }
        public string ClimateType { get; set; }
        public string TempGauge { get; set; }
        public bool DisplaySecondScale { get; set; }
        public string SecondScaleGauge { get; set; }

        public bool StormyPenalty { get; set; }
        public double DiseaseChance { get; set; }
        public int StaminaPenalty { get; set; }

        public bool HarshWeather { get; set; }
        public int HeatwaveWarning { get; set; }
        public int DeathTemp { get; set; }
        public bool AllowCropHeatDeath { get; set; }
        public int FrostWarning { get; set; }
        public double FrostHardiness { get; set; } 
        public bool GhostsDoNotSpawn { get; set; }
        public bool StormTotem { get; set; }

        public ClimateConfig()
        {
            //set defaults for mod specific stuff
            TooMuchInfo = false;

            //set keyboard key
            Keyboard = Keys.Z;

            //set overrides
            AllowSnowOnFall28 = true;
            AllowStormsFirstSpring = false;

            //set climate information
            ClimateType = "normal";

            //set tv information
            TempGauge = "celsius";
            DisplaySecondScale = false;
            SecondScaleGauge = "";

            //set storm penalty stuff
            StormyPenalty = true;
            DiseaseChance = .475;
            StaminaPenalty = 2;

            //set harsh weather events - currently unused.
            HarshWeather = false;
            HeatwaveWarning = 37; //98.6F 
            FrostWarning = 2; //35.6F
            FrostHardiness = .45; // 45% of all crops will survive the frost wave.
            DeathTemp = 41; //105.8F
            AllowCropHeatDeath = false;

            //moon options
            GhostsDoNotSpawn = false;

            //other options
            StormTotem = false;
        }
    }
}
