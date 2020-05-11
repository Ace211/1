using RimWorld;
using rjw;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace Rimworld_Animations {
    class JobDriver_SexCasualForAnimation : JobDriver_SexBaseInitiator {

        public readonly TargetIndex ipartner = TargetIndex.A;
        public readonly TargetIndex ibed = TargetIndex.B;

        public override bool TryMakePreToilReservations(bool errorOnFailed) {
            return pawn.Reserve(Target, job, xxx.max_rapists_per_prisoner, 0, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils() {
            setup_ticks();
            this.FailOnDespawnedOrNull(ipartner);
            this.FailOnDespawnedOrNull(ibed);
            this.FailOn(() => !Partner.health.capacities.CanBeAwake);

            yield return Toils_Reserve.Reserve(ipartner, xxx.max_rapists_per_prisoner, 0, null);

            Toil goToPawnInBed = Toils_Goto.GotoThing(ipartner, PathEndMode.OnCell);
            goToPawnInBed.FailOn(() => !(RestUtility.InBed(Partner) || Partner.CurJobDef == AnimationUtility.GettinLovedAnimation));

            yield return goToPawnInBed;


            Toil startPartnerSex = new Toil();
            startPartnerSex.initAction = delegate {

                if(Partner.CurJobDef != AnimationUtility.GettinLovedAnimation) {
                    Job gettinLovedJob = JobMaker.MakeJob(AnimationUtility.GettinLovedAnimation, pawn, Bed); // new gettin loved toil that wakes up the pawn goes here
                    Partner.jobs.jobQueue.EnqueueFirst(gettinLovedJob);
                    Partner.jobs.EndCurrentJob(JobCondition.InterruptForced);
                }
                
            };
            yield return startPartnerSex;

            Toil sexToil = new Toil();
            sexToil.FailOn(() => (Partner.CurJobDef == null) || Partner.CurJobDef != AnimationUtility.GettinLovedAnimation); //partner jobdriver is not sexbaserecieverlovedforanim
            sexToil.socialMode = RandomSocialMode.Off;
            sexToil.defaultCompleteMode = ToilCompleteMode.Never;
            sexToil.handlingFacing = true;
            sexToil.initAction = delegate {

                usedCondom = (CondomUtility.TryUseCondom(base.pawn) || CondomUtility.TryUseCondom(Partner));
                Start();
            };

            sexToil.AddPreTickAction(delegate {

                ticks_left--;
                if(Gen.IsHashIntervalTick(pawn, ticks_between_hearts)) {
                    MoteMaker.ThrowMetaIcon(pawn.Position, pawn.Map, ThingDefOf.Mote_Heart);
                }
                SexTick(pawn, Partner);
                SexUtility.reduce_rest(Partner);
                SexUtility.reduce_rest(pawn, 2);
                if (ticks_left <= 0)
                    ReadyForNextToil();

            });
            sexToil.AddFinishAction(delegate {

                End();

            });
            yield return sexToil;

            Toil finish = new Toil();
            finish.initAction = delegate {
                SexUtility.ProcessSex(pawn, Partner, usedCondom, isRape, isCoreLovin: false, isWhoring, sexType);    
            };
            finish.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return finish;

        }
    }
}
