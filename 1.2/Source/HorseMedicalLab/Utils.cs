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
        public static HashSet<HediffDef> BodyPuristApproved = new HashSet<HediffDef>
        {
            HML_DefOf.HRM_SilverBulletMkOne,
            HML_DefOf.HRM_SilverBulletMkTwo,
            HML_DefOf.HRM_ChestHeaterMkOne,
            HML_DefOf.HRM_ChestHeaterMkTwo,
            HML_DefOf.HRM_AdrenalineGland
        };
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
        public static MentalStateDef HLM_Berserk;
        public static HediffDef HRM_SyntheticSkin;
        public static HediffDef HRM_CleotanaBooster;
        public static HediffDef HRM_PulmoramineBooster;
        public static HediffDef HRM_EpzizymeAlclotoinBooster;
        public static HediffDef HRM_TraumaLifeSaver;
        public static HediffDef HRM_AdrenalineGland;
        public static HediffDef HRM_Exedene;
        public static HediffDef HRM_SlaverRewire;
        public static HediffDef HRM_MedicalInducedComa;
        public static HediffDef HRM_Sedated;
    }
}
