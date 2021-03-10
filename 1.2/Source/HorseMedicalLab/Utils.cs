using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace HorseMedicalLab
{
    public static class Utils
    {
        private static HashSet<HediffDef> HorseMedicalLabProductions = new HashSet<HediffDef>
        {
            HML_DefOf.HRM_SilverBulletMkOne,
        };
        public static bool IsHorseMedicalLabProduction(HediffDef hediffDef)
        {
            return HorseMedicalLabProductions.Contains(hediffDef);
        }
    }

    [DefOf]
    public static class HML_DefOf
    {
        public static HediffDef HRM_SilverBulletMkOne;
    }
}
