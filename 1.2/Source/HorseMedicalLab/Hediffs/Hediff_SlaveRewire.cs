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
    public class Hediff_SlaveRewire : Hediff_Implant
    {
        public WorkTypeDef onlyAllowedWorkType;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref onlyAllowedWorkType, "onlyAllowedWorkType");
        }
    }
}
