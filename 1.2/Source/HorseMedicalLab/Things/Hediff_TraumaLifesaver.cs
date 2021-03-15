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
    public class HediffDuration : HediffWithComps
    {
        public int durationTicks;
        public override bool ShouldRemove => Find.TickManager.TicksGame > durationTicks;
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref durationTicks, "durationTicks");
        }
    }

    public class Apparel_Respirator : Apparel
    {
        public Pawn wearer = null;
        public override void Tick()
        {
            base.Tick();
            if (this.Wearer != wearer)
            {
                if (wearer != null)
                {
                    var medicalInducedComa = wearer.health.hediffSet.GetFirstHediffOfDef(HML_DefOf.HRM_MedicalInducedComa);
                    if (medicalInducedComa != null)
                    {
                        wearer.health.hediffSet.hediffs.Remove(medicalInducedComa);
                    }
                    var sedated = HediffMaker.MakeHediff(HML_DefOf.HRM_Sedated, wearer) as HediffDuration;
                    sedated.durationTicks = (int)(Find.TickManager.TicksGame + (Rand.Range(12f, 24f) * GenDate.TicksPerDay));
                    wearer.health.AddHediff(sedated);
                }
                if (this.Wearer != null)
                {
                    var hediff = HediffMaker.MakeHediff(HML_DefOf.HRM_MedicalInducedComa, this.Wearer);
                    if (hediff != null)
                    {
                        this.Wearer.health.AddHediff(hediff);
                    }
                }
                wearer = this.Wearer;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look<Pawn>(ref wearer, "wearer");
        }
    }
}
