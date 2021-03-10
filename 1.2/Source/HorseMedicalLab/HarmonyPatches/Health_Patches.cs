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
