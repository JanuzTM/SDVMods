﻿using Harmony;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.BellsAndWhistles;
using System;
using System.Collections.Generic;
using System.Linq;
using SFarmer = StardewValley.Farmer;
using System.Reflection;
using TwilightShards.Common;
using TwilightShards.Stardew.Common;

namespace DynamicNightTime
{
    public class DynamicNightConfig
    {
        public double latitude = 38.25;
    }

    public class DynamicNightTime : Mod
    {
        internal Color NormalDayLightColor = new Color(255, 255, 255);
        public static DynamicNightConfig NightConfig;

        public override void Entry(IModHelper helper)
        {
            NightConfig = Helper.ReadConfig<DynamicNightConfig>();

            //sanity check lat
            if (NightConfig.latitude > 64)
                NightConfig.latitude = 64;
            if (NightConfig.latitude < -64)
                NightConfig.latitude = -64;

            var harmony = HarmonyInstance.Create("koihimenakamura.dynamicnighttime");
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            //patch getStartingToGetDarkTime
            MethodInfo setStartingToGetDarkTime = typeof(Game1).GetMethods(BindingFlags.Static | BindingFlags.Public).ToList().Find(m => m.Name == "getStartingToGetDarkTime");
            MethodInfo postfix = typeof(Patches.GettingDarkPatch).GetMethods(BindingFlags.Static | BindingFlags.Public).ToList().Find(m => m.Name == "Postfix");
            harmony.Patch(setStartingToGetDarkTime, null, new HarmonyMethod(postfix));

            //patch getTrulyDarkTime
            MethodInfo setTrulyDarkTime = typeof(Game1).GetMethods(BindingFlags.Static | BindingFlags.Public).ToList().Find(m => m.Name == "getTrulyDarkTime");
            MethodInfo postfixDark = typeof(Patches.GetFullyDarkPatch).GetMethods(BindingFlags.Static | BindingFlags.Public).ToList().Find(m => m.Name == "Postfix");
            harmony.Patch(setTrulyDarkTime, null, new HarmonyMethod(postfixDark));

            //patch isDarkOutPatch
            MethodInfo isDarkOut = typeof(Game1).GetMethods(BindingFlags.Static | BindingFlags.Public).ToList().Find(m => m.Name == "isDarkOut");
            MethodInfo postfixIsDarkOut = typeof(Patches.IsDarkOutPatch).GetMethods(BindingFlags.Static | BindingFlags.Public).ToList().Find(m => m.Name == "Postfix");
            harmony.Patch(isDarkOut, null, new HarmonyMethod(postfixIsDarkOut));

            //patch UpdateGameClock
            MethodInfo UpdateGameClock = helper.Reflection.GetMethod(typeof(Game1), "UpdateGameClock").MethodInfo;
            MethodInfo postfixClock =
                helper.Reflection.GetMethod(typeof(Patches.GameClockPatch), "Postfix").MethodInfo;
            harmony.Patch(UpdateGameClock, null, new HarmonyMethod(postfixClock));

            //and now events!
            TimeEvents.TimeOfDayChanged += TimeEvents_TimeOfDayChanged;
        }

        private void TimeEvents_TimeOfDayChanged(object sender, EventArgsIntChanged e)
        {
            bool IsBeforeSunrise = Game1.timeOfDay < GetSunrise().ReturnIntTime();
            bool IsPastSunset = Game1.timeOfDay > Game1.getTrulyDarkTime();

            SFarmer who = Game1.player;

            if (who != null && who.currentLocation != null && Game1.isDarkOut())
            {
                List<Critter> currentCritters = Helper.Reflection.GetField<List<Critter>>(who.currentLocation, "critters").GetValue();
                if (currentCritters != null)
                {
                    for (int i = 0; i < currentCritters.Count; i++)
                    {
                        if (currentCritters[i] is Cloud)
                            currentCritters.Remove(currentCritters[i]);
                    }
                }
            }
        }

        private int GetSunriseTime() => GetSunrise().ReturnIntTime();

        private SDVTime GetSunrise()
        {
            var date = SDate.Now();
            int dayOfYear = date.DaysSinceStart % 112;
            double lat = GeneralFunctions.DegreeToRadians(NightConfig.latitude);

            double solarDeclination = .40927971 * Math.Sin((2 * Math.PI / 112) * (dayOfYear - 1));
            double noon = 720 - 10 * Math.Sin(4 * (Math.PI / 112) * (dayOfYear - 1)) + 8 * Math.Sin(2 * (Math.PI / 112) * dayOfYear);
            double hourAngle = (Math.Sin(0.01163611) - Math.Sin(lat) * Math.Sin(solarDeclination)) / (Math.Cos(lat) * Math.Cos(solarDeclination));
            double procHA = Math.Acos(hourAngle);
            double minHA = (procHA / (2 * Math.PI)) * 1440;
            int astroTwN = (int)Math.Floor(noon - minHA);

            //Conv to an SDV compat time, then clamp it.
            int hr = (int)Math.Floor(astroTwN / 60.0);
            int min = astroTwN - (hr * 60);
            SDVTime calcTime = new SDVTime(hr, min);
            calcTime.ClampToTenMinutes();

            return calcTime;
        }

        private SDVTime GetMorningAstroTwilight()
        {
            var date = SDate.Now();
            int dayOfYear = date.DaysSinceStart % 112;
            double lat = GeneralFunctions.DegreeToRadians(NightConfig.latitude);

            double solarDeclination = .40927971 * Math.Sin((2 * Math.PI / 112) * (dayOfYear - 1));
            double noon = 720 - 10 * Math.Sin(4 * (Math.PI / 112) * (dayOfYear - 1)) + 8 * Math.Sin(2 * (Math.PI / 112) * dayOfYear);
            double astroHA = Math.Acos((Math.Sin(-0.314159265) - Math.Sin(lat) * Math.Sin(solarDeclination)) / (Math.Cos(lat) * Math.Cos(solarDeclination)));
            double minHA = (astroHA / (2 * Math.PI)) * 1440;
            int astroTwN = (int)Math.Floor(noon + minHA);

            //Conv to an SDV compat time, then clamp it.
            int hr = (int)Math.Floor(astroTwN / 60.0);
            int min = astroTwN - (hr * 60);
            SDVTime calcTime = new SDVTime(hr, min);
            calcTime.ClampToTenMinutes();
            return calcTime;
        }

    }
}
