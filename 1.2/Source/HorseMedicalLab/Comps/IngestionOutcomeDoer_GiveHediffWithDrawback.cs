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
	public class IngestionOutcomeDoer_GiveHediffWithDrawback : IngestionOutcomeDoer
	{
		public HediffDef hediffDef;

		public float severity = -1f;

		public ChemicalDef toleranceChemical;

		public float diseaseChance; 
		private bool divideByBodySize;

		protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested)
		{
			if (Rand.Chance(diseaseChance))
			{
				Hediff disease = HediffMaker.MakeHediff(HediffDefOf.Flu, pawn);
				disease.Severity = 0.1f;
				pawn.health.AddHediff(disease);
			}
			Hediff hediff = HediffMaker.MakeHediff(hediffDef, pawn);
			float effect = (!(severity > 0f)) ? hediffDef.initialSeverity : severity;
			if (divideByBodySize)
			{
				effect /= pawn.BodySize;
			}
			AddictionUtility.ModifyChemicalEffectForToleranceAndBodySize(pawn, toleranceChemical, ref effect);
			hediff.Severity = effect;
			pawn.health.AddHediff(hediff);

		}

		public override IEnumerable<StatDrawEntry> SpecialDisplayStats(ThingDef parentDef)
		{
			if (parentDef.IsDrug && chance >= 1f)
			{
				foreach (StatDrawEntry item in hediffDef.SpecialDisplayStats(StatRequest.ForEmpty()))
				{
					yield return item;
				}
			}
		}
	}
}
