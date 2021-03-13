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
	[HarmonyPatch(typeof(HealthUtility), "AdjustSeverity")]
	public class AdjustSeverity_Patch
	{
		public static bool Prefix(Pawn pawn, HediffDef hdDef, ref float sevOffset)
		{
			if (hdDef == HediffDefOf.Heatstroke)
            {
				foreach (var hediff in pawn.health.hediffSet.hediffs)
                {
					if (hediff.def == HML_DefOf.HRM_SilverBulletMkOne)
                    {
						var severity = pawn.GetSeverityFrom(hdDef);
						if (severity + sevOffset >= 0.62f)
                        {
							return false;
                        }
					}
					else if (hediff.def == HML_DefOf.HRM_SilverBulletMkTwo)
					{
						var severity = pawn.GetSeverityFrom(hdDef);
						if (severity + sevOffset >= 0.35f)
						{
							return false;
						}
					}
				}
            }
			else if (hdDef == HediffDefOf.Hypothermia)
            {
				foreach (var hediff in pawn.health.hediffSet.hediffs)
				{
					if (hediff.def == HML_DefOf.HRM_ChestHeaterMkOne)
					{
						var severity = pawn.GetSeverityFrom(hdDef);
						if (severity + sevOffset >= 0.62f)
						{
							return false;
						}
					}
					else if (hediff.def == HML_DefOf.HRM_ChestHeaterMkTwo)
                    {
						var severity = pawn.GetSeverityFrom(hdDef);
						if (severity + sevOffset >= 0.35f)
						{
							return false;
						}
					}
				}
			}
			return true;
		}
	}

	[HarmonyPatch(typeof(Thing), "TakeDamage")]
	public class TakeDamage_Patch
	{
		public static bool Prefix(Thing __instance, DamageInfo dinfo)
		{
			if (dinfo.Def == DamageDefOf.Frostbite && __instance is Pawn pawn)
			{
				foreach (var hediff in pawn.health.hediffSet.hediffs)
				{
					if (hediff.def == HML_DefOf.HRM_ChestHeaterMkOne)
					{
						var severity = pawn.GetSeverityFrom(HediffDefOf.Hypothermia);
						if (severity < 0.35f)
                        {
							return false;
                        }
					}
					else if (hediff.def == HML_DefOf.HRM_ChestHeaterMkTwo)
                    {
						var severity = pawn.GetSeverityFrom(HediffDefOf.Hypothermia);
						if (severity < 0.62f)
						{
							return false;
						}
					}
				}
			}
			return true;
		}
	}

	[HarmonyPatch(typeof(ThoughtWorker_BodyPuristDisgust), "CurrentSocialStateInternal")]
	public class ThoughtWorker_BodyPuristDisgust_Patch
	{
		public static void Prefix()
		{
			CountAddedParts_Patch.DoCheck = true;
		}
		public static void Postfix()
		{
			CountAddedParts_Patch.DoCheck = false;
		}
	}

	[HarmonyPatch(typeof(ThoughtWorker_HasAddedBodyPart), "CurrentStateInternal")]
	public class ThoughtWorker_HasAddedBodyPart_Patch
	{
		public static void Prefix()
		{
			CountAddedParts_Patch.DoCheck = true;

		}
		public static void Postfix()
		{
			CountAddedParts_Patch.DoCheck = false;
		}
	}
	[HarmonyPatch(typeof(HediffUtility), "CountAddedAndImplantedParts")]
	public class CountAddedParts_Patch
	{
		public static bool DoCheck;
		public static bool Prefix(HediffSet hs, ref int __result)
		{
			if (DoCheck)
            {
				if (hs.pawn.story != null && hs.pawn.story.traits.HasTrait(TraitDefOf.BodyPurist))
                {
					__result = CountAddedAndImplantedPartsWithExceptions(hs);
					return false;
				}
			}
			return true;
		}

		public static int CountAddedAndImplantedPartsWithExceptions(HediffSet hs)
		{
			int num = 0;
			List<Hediff> hediffs = hs.hediffs;
			for (int i = 0; i < hediffs.Count; i++)
			{
				if (hediffs[i].def.countsAsAddedPartOrImplant && !Utils.IsHorseMedicalLabProduction(hediffs[i].def))
				{
					num++;
				}
			}
			return num;
		}
	}
}
