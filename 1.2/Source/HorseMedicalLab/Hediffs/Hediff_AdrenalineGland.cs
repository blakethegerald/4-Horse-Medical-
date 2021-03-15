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
    public class Hediff_AdrenalineGland : Hediff_Implant
    {
        private float prevSeverity;
        public override void Tick()
        {
            base.Tick();
            var consciousnessLevel = this.pawn.health.capacities.GetLevel(PawnCapacityDefOf.Consciousness);
            if (consciousnessLevel <= 0.25f && prevSeverity < 0.5f)
            {
                this.Severity = 0.5f;
                prevSeverity = this.Severity;
                if (Rand.Chance(0.25f))
                {
                    var heartAttack = this.pawn.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("HeartAttack"));
                    if (heartAttack != null)
                    {
                        heartAttack.Severity += Rand.Range(0.1f, 1f - heartAttack.Severity);
                    }
                }
            }
            else if (consciousnessLevel > 0.25f && prevSeverity >= 0.5f)
            {
                this.Severity = 0f;
                prevSeverity = this.Severity;
            }
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref prevSeverity, "prevSeverity");
        }
    }
}
