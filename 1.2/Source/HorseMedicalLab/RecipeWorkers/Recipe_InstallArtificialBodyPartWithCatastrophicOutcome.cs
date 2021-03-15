using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace HorseMedicalLab
{
	public class Recipe_InstallArtificialBodyPartDeath : Recipe_InstallArtificialBodyPartWithCatastrophicOutcome
	{
		public override Action<Pawn> CatastrophicOutcome => delegate (Pawn pawn)
		{
			pawn.Kill(null);
		};
	}
	public class Recipe_InstallArtificialBodyPartParalysis : Recipe_InstallArtificialBodyPartWithCatastrophicOutcome
    {
		public override Action<Pawn> CatastrophicOutcome => delegate (Pawn pawn)
		{
			pawn.health.AddHediff(HediffDefOf.Abasia);
		};
	}

	public class Recipe_InstallArtificialBodyPartPsychotic : Recipe_InstallArtificialBodyPartWithCatastrophicOutcome
	{
		public override Action<Pawn> CatastrophicOutcome => delegate (Pawn pawn)
		{
			pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Wander_Psychotic);
		};
	}

	public class Recipe_InstallArtificialBodyPartBerserk : Recipe_InstallArtificialBodyPartWithCatastrophicOutcome
	{
		public override Action<Pawn> CatastrophicOutcome => delegate (Pawn pawn)
		{
			pawn.mindState.mentalStateHandler.TryStartMentalState(HML_DefOf.HLM_Berserk);
		};
	}
	public class Recipe_InstallArtificialBodyPartWithCatastrophicOutcome : Recipe_Surgery
	{
		public virtual Action<Pawn> CatastrophicOutcome => null;
		public override IEnumerable<BodyPartRecord> GetPartsToApplyOn(Pawn pawn, RecipeDef recipe)
		{
			return MedicalRecipesUtility.GetFixedPartsToApplyOn(recipe, pawn, delegate (BodyPartRecord record)
			{
				IEnumerable<Hediff> source = pawn.health.hediffSet.hediffs.Where((Hediff x) => x.Part == record);
				if (source.Count() == 1 && source.First().def == recipe.addsHediff)
				{
					return false;
				}
				if (record.parent != null && !pawn.health.hediffSet.GetNotMissingParts().Contains(record.parent))
				{
					return false;
				}
				return (!pawn.health.hediffSet.PartOrAnyAncestorHasDirectlyAddedParts(record) || pawn.health.hediffSet.HasDirectlyAddedPartFor(record)) ? true : false;
			});
		}
		private static readonly SimpleCurve MedicineMedicalPotencyToSurgeryChanceFactor = new SimpleCurve
		{
			new CurvePoint(0f, 0.7f),
			new CurvePoint(1f, 1f),
			new CurvePoint(2f, 1.3f)
		};
		protected bool CheckSurgeryFailCustom(Pawn surgeon, Pawn patient, List<Thing> ingredients, BodyPartRecord part, Bill bill)
		{
			if (bill.recipe.surgerySuccessChanceFactor >= 99999f)
			{
				return false;
			}
			float num = 1f;
			if (!patient.RaceProps.IsMechanoid)
			{
				num *= surgeon.GetStatValue(StatDefOf.MedicalSurgerySuccessChance);
			}
			if (patient.InBed())
			{
				num *= patient.CurrentBed().GetStatValue(StatDefOf.SurgerySuccessChanceFactor);
			}
			num *= MedicineMedicalPotencyToSurgeryChanceFactor.Evaluate(GetAverageMedicalPotency(ingredients, bill));
			num *= recipe.surgerySuccessChanceFactor;
			if (surgeon.InspirationDef == InspirationDefOf.Inspired_Surgery && !patient.RaceProps.IsMechanoid)
			{
				num *= 2f;
				surgeon.mindState.inspirationHandler.EndInspiration(InspirationDefOf.Inspired_Surgery);
			}
			num = Mathf.Min(num, 0.98f);
			if (!Rand.Chance(num))
			{
				if (Rand.Chance(recipe.deathOnFailedSurgeryChance))
				{
					HealthUtility.GiveInjuriesOperationFailureCatastrophic(patient, part);
					if (!patient.Dead)
					{
						patient.Kill(null, null);
					}
					Find.LetterStack.ReceiveLetter("LetterLabelSurgeryFailed".Translate(patient.Named("PATIENT")), "MessageMedicalOperationFailureFatal".Translate(surgeon.LabelShort, patient.LabelShort, recipe.LabelCap, surgeon.Named("SURGEON"), patient.Named("PATIENT")), LetterDefOf.NegativeEvent, patient);
				}
				else if (Rand.Chance(0.5f))
				{
					if (Rand.Chance(0.1f))
					{
						Find.LetterStack.ReceiveLetter("LetterLabelSurgeryFailed".Translate(patient.Named("PATIENT")), "MessageMedicalOperationFailureRidiculous".Translate(surgeon.LabelShort, patient.LabelShort, surgeon.Named("SURGEON"), patient.Named("PATIENT"), recipe.Named("RECIPE")), LetterDefOf.NegativeEvent, patient);
						HealthUtility.GiveInjuriesOperationFailureRidiculous(patient);
					}
					else
					{
						Find.LetterStack.ReceiveLetter("LetterLabelSurgeryFailed".Translate(patient.Named("PATIENT")), "MessageMedicalOperationFailureCatastrophic".Translate(surgeon.LabelShort, patient.LabelShort, surgeon.Named("SURGEON"), patient.Named("PATIENT"), recipe.Named("RECIPE")), LetterDefOf.NegativeEvent, patient);
						HealthUtility.GiveInjuriesOperationFailureCatastrophic(patient, part);
						CatastrophicOutcome(patient);
					}
				}
				else
				{
					Find.LetterStack.ReceiveLetter("LetterLabelSurgeryFailed".Translate(patient.Named("PATIENT")), "MessageMedicalOperationFailureMinor".Translate(surgeon.LabelShort, patient.LabelShort, surgeon.Named("SURGEON"), patient.Named("PATIENT"), recipe.Named("RECIPE")), LetterDefOf.NegativeEvent, patient);
					HealthUtility.GiveInjuriesOperationFailureMinor(patient, part);
				}
				if (!patient.Dead)
				{
					TryGainBotchedSurgeryThought(patient, surgeon);
				}
				return true;
			}
			return false;
		}
		private void TryGainBotchedSurgeryThought(Pawn patient, Pawn surgeon)
		{
			if (patient.RaceProps.Humanlike && patient.needs.mood != null)
			{
				patient.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.BotchedMySurgery, surgeon);
			}
		}

		private float GetAverageMedicalPotency(List<Thing> ingredients, Bill bill)
		{
			ThingDef thingDef = (bill as Bill_Medical)?.consumedInitialMedicineDef;
			int num = 0;
			float num2 = 0f;
			if (thingDef != null)
			{
				num++;
				num2 += thingDef.GetStatValueAbstract(StatDefOf.MedicalPotency);
			}
			for (int i = 0; i < ingredients.Count; i++)
			{
				Medicine medicine = ingredients[i] as Medicine;
				if (medicine != null)
				{
					num += medicine.stackCount;
					num2 += medicine.GetStatValue(StatDefOf.MedicalPotency) * (float)medicine.stackCount;
				}
			}
			if (num == 0)
			{
				return 1f;
			}
			return num2 / (float)num;
		}
		public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
		{
			bool flag = MedicalRecipesUtility.IsClean(pawn, part);
			bool flag2 = !PawnGenerator.IsBeingGenerated(pawn) && IsViolationOnPawn(pawn, part, Faction.OfPlayer);
			if (billDoer != null)
			{
				if (CheckSurgeryFailCustom(billDoer, pawn, ingredients, part, bill))
				{
					return;
				}
				TaleRecorder.RecordTale(TaleDefOf.DidSurgery, billDoer, pawn);
				MedicalRecipesUtility.RestorePartAndSpawnAllPreviousParts(pawn, part, billDoer.Position, billDoer.Map);
				if (flag && flag2 && part.def.spawnThingOnRemoved != null)
				{
					ThoughtUtility.GiveThoughtsForPawnOrganHarvested(pawn);
				}
				if (flag2)
				{
					ReportViolation(pawn, billDoer, pawn.FactionOrExtraMiniOrHomeFaction, -70, "GoodwillChangedReason_NeedlesslyInstalledWorseBodyPart".Translate(recipe.addsHediff.label));
				}
			}
			else if (pawn.Map != null)
			{
				MedicalRecipesUtility.RestorePartAndSpawnAllPreviousParts(pawn, part, pawn.Position, pawn.Map);
			}
			else
			{
				pawn.health.RestorePart(part);
			}
			pawn.health.AddHediff(recipe.addsHediff, part);
		}

		public override bool IsViolationOnPawn(Pawn pawn, BodyPartRecord part, Faction billDoerFaction)
		{
			if ((pawn.Faction == billDoerFaction || pawn.Faction == null) && !pawn.IsQuestLodger())
			{
				return false;
			}
			if (recipe.addsHediff.addedPartProps != null && recipe.addsHediff.addedPartProps.betterThanNatural)
			{
				return false;
			}
			return HealthUtility.PartRemovalIntent(pawn, part) == BodyPartRemovalIntent.Harvest;
		}
	}
}
