using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using UnityEngine;

namespace Rimworld_Animations {
    public class AnimationSettings : ModSettings {

        public static bool orgasmQuiver;

        public override void ExposeData() {
            Scribe_Values.Look(ref orgasmQuiver, "orgasmQuiver");
            base.ExposeData();
        }

    }

    public class RJW_Animations : Mod {

        public RJW_Animations(ModContentPack content) : base(content) {
        }

        public override void DoSettingsWindowContents(Rect inRect) {

            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);

            listingStandard.CheckboxLabeled("Enable Orgasm Quiver", ref AnimationSettings.orgasmQuiver);
            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory() {
            return "RJW Animation Settings";
        }
    }
}
