using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;
using System;
using rjw;

namespace Rimworld_Animations {
	public class JobDriver_SexBaseRecieverLovedForAnimation : JobDriver_SexBaseReciever {

		public readonly TargetIndex ipartner = TargetIndex.A;
		public readonly TargetIndex ibed = TargetIndex.B;

		protected override IEnumerable<Toil> MakeNewToils() {
			setup_ticks();
			parteners.Add(Partner);// add job starter, so this wont fail, before Initiator starts his job
								   //--Log.Message("[RJW]JobDriver_GettinLoved::MakeNewToils is called");

			float partner_ability = xxx.get_sex_ability(Partner);

			// More/less hearts based on partner ability.
			if (partner_ability < 0.8f)
				ticks_between_thrusts += 100;
			else if (partner_ability > 2.0f)
				ticks_between_thrusts -= 25;

			// More/less hearts based on opinion.
			if (pawn.relations.OpinionOf(Partner) < 0)
				ticks_between_hearts += 50;
			else if (pawn.relations.OpinionOf(Partner) > 60)
				ticks_between_hearts -= 25;

			this.FailOnDespawnedOrNull(ipartner);
			this.FailOn(() => !Partner.health.capacities.CanBeAwake);
			this.FailOn(() => pawn.Drafted);
			this.KeepLyingDown(ibed);
			yield return Toils_Reserve.Reserve(ipartner, 1, 0);
			yield return Toils_Reserve.Reserve(ibed, Bed.SleepingSlotsCount, 0);

			Toil get_loved = new Toil();
			get_loved.FailOn(() => Partner.CurJobDef != DefDatabase<JobDef>.GetNamed("JoinInBedAnimation", true));
			get_loved.defaultCompleteMode = ToilCompleteMode.Never;
			get_loved.socialMode = RandomSocialMode.Off;
			get_loved.handlingFacing = true;
			get_loved.AddPreTickAction(delegate {
				if (pawn.IsHashIntervalTick(ticks_between_hearts))
					MoteMaker.ThrowMetaIcon(pawn.Position, pawn.Map, ThingDefOf.Mote_Heart);
			});
			get_loved.AddFinishAction(delegate {
				if (xxx.is_human(pawn))
					pawn.Drawer.renderer.graphics.ResolveApparelGraphics();
			});
			yield return get_loved;
			

		}
	}
}