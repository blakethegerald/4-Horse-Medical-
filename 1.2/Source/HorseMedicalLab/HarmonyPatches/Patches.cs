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
	[HarmonyPatch(typeof(Pawn), "GetGizmos")]
	public class Pawn_GetGizmos_Patch
	{
		public static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> __result, Pawn __instance)
		{
			foreach (var g in __result)
			{
				yield return g;
			}
			if (__instance.MentalState is null)
			{
				var slaveRewire = __instance.health.hediffSet.GetFirstHediffOfDef(HML_DefOf.HRM_SlaverRewire) as Hediff_SlaveRewire;
				if (slaveRewire != null)
				{
					Command_Action command_Action = new Command_Action();
					command_Action.defaultLabel = "HML.ChooseActiveWorkType".Translate();
					command_Action.defaultDesc = "HML.ChooseActiveWorkTypeDesc".Translate();
					command_Action.action = delegate
					{
						Find.WindowStack.Add(new FloatMenu(GetFloatMenuOptions(slaveRewire).ToList()));
					};
					yield return command_Action;

					Command_Action command_Action2 = new Command_Action();
					command_Action2.defaultLabel = "HML.ShutDown".Translate();
					command_Action2.defaultDesc = "HML.ShutDownDesc".Translate();
					command_Action2.action = delegate
					{
						if (slaveRewire.Severity != 0)
                        {
							slaveRewire.Severity = 0;
						}
						else
                        {
							slaveRewire.Severity = 0.5f;
						}
					};
					yield return command_Action2;
				}
			}
		}

		private static IEnumerable<FloatMenuOption> GetFloatMenuOptions(Hediff_SlaveRewire hediff_SlaveRewire)
		{
			foreach (var workType in DefDatabase<WorkTypeDef>.AllDefs)
            {
				yield return new FloatMenuOption(workType.labelShort, delegate
				{
					hediff_SlaveRewire.onlyAllowedWorkType = workType;
				}, MenuOptionPriority.Default, null, null, 29f);
			}
		}
	}

	[HarmonyPatch(typeof(Pawn), "WorkTypeIsDisabled")]
	public class WorkTypeIsDisabled_Patch
	{
		public static void Postfix(Pawn __instance, WorkTypeDef w, ref bool __result)
		{
			var slaveRewire = __instance.health.hediffSet.GetFirstHediffOfDef(HML_DefOf.HRM_SlaverRewire) as Hediff_SlaveRewire;
			if (slaveRewire != null)
			{
				__result = w != slaveRewire.onlyAllowedWorkType;
			}
		}
	}

	[HarmonyPatch(typeof(Pawn_WorkSettings), "WorkGiversInOrderNormal", MethodType.Getter)]
	public class WorkGiversInOrderNormal_Patch
	{
		public static void Postfix(Pawn ___pawn, ref List<WorkGiver> __result)
		{
			var slaveRewire = ___pawn.health.hediffSet.GetFirstHediffOfDef(HML_DefOf.HRM_SlaverRewire) as Hediff_SlaveRewire;
			if (slaveRewire != null)
            {
				__result = __result.Where(x => x.def.workType == slaveRewire.onlyAllowedWorkType).ToList();
			}
		}
	}

	[HarmonyPatch(typeof(StatExtension), nameof(StatExtension.GetStatValue))]
	public static class GetStatValue_Patch
	{
		private static void Postfix(Thing thing, StatDef stat, bool applyPostProcess, ref float __result)
		{
			if (stat == StatDefOf.ImmunityGainSpeed && thing is Pawn pawn)
			{
				var exedene = pawn.health.hediffSet.GetFirstHediffOfDef(HML_DefOf.HRM_Exedene);
				if (exedene != null)
                {
					__result *= 1.5f;
                }
			}
		}
	}

	[HarmonyPatch(typeof(Pawn_HealthTracker), "MakeDowned")]
	public static class MakeDowned_Patch
	{
		private static bool Prefix(Pawn ___pawn, DamageInfo? dinfo, Hediff hediff)
		{
			var adrenalineGland = ___pawn.health.hediffSet.GetFirstHediffOfDef(HML_DefOf.HRM_AdrenalineGland);
			if (adrenalineGland != null && adrenalineGland.Severity >= 0.5f)
			{
				return false;
			}
			return true;
		}
	}


	[HarmonyPatch(typeof(Hediff), "BleedRate", MethodType.Getter)]
	public class BleedRate_Patch
	{
		public static void Postfix(Hediff __instance, ref float __result)
		{
			var traumaLifesaver = __instance.pawn.health.hediffSet.GetFirstHediffOfDef(HML_DefOf.HRM_TraumaLifeSaver) as Hediff_TraumaLifesaver;
			if (traumaLifesaver != null && traumaLifesaver.oldInjuries.Contains(__instance))
            {
				__result = 0f;
			}
		}
	}

	[HarmonyPatch(typeof(Pawn_HealthTracker), "AddHediff", new Type[]
	{
		typeof(Hediff), typeof(BodyPartRecord), typeof(DamageInfo?), typeof(DamageWorker.DamageResult)
	})]
	public static class AddHediff_Patch
	{
		private static bool Prefix(Pawn_HealthTracker __instance, Pawn ___pawn, Hediff hediff, BodyPartRecord part = null, DamageInfo? dinfo = null, DamageWorker.DamageResult result = null)
		{
			if (hediff.def == HediffDefOf.Scaria || hediff.def == HediffDefOf.Carcinoma)
            {
				foreach (var oldHediff in ___pawn.health.hediffSet.hediffs)
				{
					if (oldHediff.def == HML_DefOf.HRM_EpzizymeAlclotoinBooster)
					{
						return false;
					}
				}
			}
			return true;
		}
	}

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
			else if (hdDef == HediffDefOf.WoundInfection)
            {
				foreach (var hediff in pawn.health.hediffSet.hediffs)
				{
					if (hediff.def == HML_DefOf.HRM_CleotanaBooster)
					{
						var severity = pawn.GetSeverityFrom(hdDef);
						if (severity + sevOffset >= 0.87f)
						{
							return false;
						}
					}
					else if (hediff.def == HML_DefOf.HRM_PulmoramineBooster)
					{
						var severity = pawn.GetSeverityFrom(hdDef);
						if (severity + sevOffset >= 0.78f)
						{
							return false;
						}
					}
					else if (hediff.def == HML_DefOf.HRM_EpzizymeAlclotoinBooster)
					{
						var severity = pawn.GetSeverityFrom(hdDef);
						if (severity + sevOffset >= 0.33f)
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

	[HarmonyPatch(typeof(Hediff_Injury), "Heal")]
	public class Heal_Patch
	{
		public static void Prefix(Hediff_Injury __instance, ref float amount)
		{
			if (__instance.def == HediffDefOf.Burn)
            {
				var skin = __instance.pawn.health.hediffSet.GetFirstHediffOfDef(HML_DefOf.HRM_SyntheticSkin);
				if (skin != null)
                {
					amount *= 2f;
                }
            }
		}
	}

	[HarmonyPatch(typeof(HediffComp_GetsPermanent), "IsPermanent", MethodType.Setter)]
	public class IsPermanent_Patch
	{
		public static bool Prefix(HediffComp_GetsPermanent __instance, bool value)
		{
			if (value)
            {
				var skin = __instance.Pawn.health.hediffSet.GetFirstHediffOfDef(HML_DefOf.HRM_SyntheticSkin);
				if (skin != null)
				{
					if (Rand.Chance(0.15f))
                    {
						return false;
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
				if (hediffs[i].def.countsAsAddedPartOrImplant && !Utils.BodyPuristApproved.Contains(hediffs[i].def))
				{
					num++;
				}
			}
			return num;
		}
	}
}
