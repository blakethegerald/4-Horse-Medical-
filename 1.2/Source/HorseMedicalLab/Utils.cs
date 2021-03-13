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
            HML_DefOf.HRM_SilverBulletMkTwo,
            HML_DefOf.HRM_ChestHeaterMkOne,
            HML_DefOf.HRM_ChestHeaterMkTwo,
        };
        public static bool IsHorseMedicalLabProduction(HediffDef hediffDef)
        {
            return HorseMedicalLabProductions.Contains(hediffDef);
        }

        public static float GetSeverityFrom(this Pawn pawn, HediffDef hediffDef)
        {
            Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(hediffDef);
            if (firstHediffOfDef != null)
            {
                return firstHediffOfDef.Severity;
            }
            else
            {
                return 0f;
            }
        }
    }

    [DefOf]
    public static class HML_DefOf
    {
        public static HediffDef HRM_SilverBulletMkOne;
        public static HediffDef HRM_SilverBulletMkTwo;
        public static HediffDef HRM_ChestHeaterMkOne;
        public static HediffDef HRM_ChestHeaterMkTwo;

    }
}
