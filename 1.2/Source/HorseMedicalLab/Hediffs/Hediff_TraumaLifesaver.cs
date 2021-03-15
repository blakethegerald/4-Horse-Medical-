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
    public class Hediff_TraumaLifesaver : Hediff_Implant
    {
        public List<Hediff> oldInjuries;
        public override void PostAdd(DamageInfo? dinfo)
        {
            base.PostAdd(dinfo);
            oldInjuries = this.pawn.health.hediffSet.hediffs.OfType<Hediff>().ToList();
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref oldInjuries, "oldInjuries");
        }
    }
}
