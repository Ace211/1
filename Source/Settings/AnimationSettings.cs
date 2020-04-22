using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using UnityEngine;

namespace Rimworld_Animations {
    public class AnimationSettings : ModSettings {

        public static bool orgasmQuiver, rapeShiver, soundOverride = true, hearts = true;
        public static float shiverIntensity = 2f;

        public override void ExposeData() {
            Scribe_Values.Look(ref orgasmQuiver, "orgasmQuiver");
            Scribe_Values.Look(ref rapeShiver, "rapeShiver");
            Scribe_Values.Look(ref hearts, "heartsOnLovin");
            Scribe_Values.Look(ref soundOverride, "rjwAnimSoundOverride", true);
            Scribe_Values.Look(ref shiverIntensity, "shiverIntensity", 2f);

            base.ExposeData();
        }

    }

    public class RJW_Animations : Mod {

        public RJW_Animations(ModContentPack content) : base(content) {
        }

        public override void DoSettingsWindowContents(Rect inRect) {

            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);

            listingStandard.CheckboxLabeled("Enable Sound Override", ref AnimationSettings.soundOverride);
            listingStandard.CheckboxLabeled("Enable Orgasm Quiver", ref AnimationSettings.orgasmQuiver);
            listingStandard.CheckboxLabeled("Enable Rape Shiver", ref AnimationSettings.rapeShiver);
            listingStandard.CheckboxLabeled("Enable hearts during lovin'", ref AnimationSettings.hearts);

            listingStandard.Label("Shiver/Quiver Intensity (default 2): " + AnimationSettings.shiverIntensity);
            AnimationSettings.shiverIntensity = listingStandard.Slider(AnimationSettings.shiverIntensity, 0.0f, 12f);

            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory() {
            return "RJW Animation Settings";
        }
    }
}
