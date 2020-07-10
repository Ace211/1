using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Rimworld_Animations {
    public static class AnimationUtility {
        /*  
         Note: always make the list in this order:
             Female pawns, animal female pawns, male pawns, animal male pawns
        */
        public static AnimationDef tryFindAnimation(ref List<Pawn> participants, rjw.xxx.rjwSextype sexType = 0) {

            //aggressors last
            participants = participants.OrderBy(p => p.jobs.curDriver is rjw.JobDriver_SexBaseInitiator).ToList();

            //pawns that can fuck last
            participants = participants.OrderBy(p => rjw.xxx.can_fuck(p)).ToList();

            participants = participants.OrderByDescending(p => rjw.GenderHelper.GetSex(p) == rjw.GenderHelper.Sex.futa).ToList();


            List<Pawn> localParticipants = new List<Pawn>(participants);

            IEnumerable<AnimationDef> options = DefDatabase<AnimationDef>.AllDefs.Where((AnimationDef x) => {



                if (x.actors.Count != localParticipants.Count) {
                    return false;
                }
                for (int i = 0; i < x.actors.Count; i++) {

                    if (rjw.RJWPreferenceSettings.Malesex == rjw.RJWPreferenceSettings.AllowedSex.Nohomo) {
                        if (rjw.xxx.is_male(localParticipants[i]) && x.actors[i].isFucked) {
                            return false;
                        }
                }

                    if ((x.actors[i].blacklistedRaces != null) && x.actors[i].blacklistedRaces.Contains(localParticipants[i].def.defName)) {
                        if (rjw.RJWSettings.DevMode) {
                            Log.Message(x.defName.ToStringSafe() + " not selected -- " + localParticipants[i].def.defName.ToStringSafe() + " " + localParticipants[i].Name.ToStringSafe() + " is blacklisted");
                        }
                        return false;
                    }

                    if(x.actors[i].defNames.Contains("Human")) {
                        if (!rjw.xxx.is_human(localParticipants[i])) {
                            if(rjw.RJWSettings.DevMode) {
                                Log.Message(x.defName.ToStringSafe() + " not selected -- " + localParticipants[i].def.defName.ToStringSafe() + " " + localParticipants[i].Name.ToStringSafe() + " is not human");
                            }
                            
                            return false;
                        }
                        
                    }
                    else if (!x.actors[i].bodyDefTypes.Contains(localParticipants[i].RaceProps.body)) {

                        if (!x.actors[i].defNames.Contains(localParticipants[i].def.defName)) {

                            if (rjw.RJWSettings.DevMode) {
                                string animInfo = x.defName.ToStringSafe() + " not selected -- " + localParticipants[i].def.defName.ToStringSafe() + " " + localParticipants[i].Name.ToStringSafe() + " is not ";
                                foreach(String defname in x.actors[i].defNames) {
                                    animInfo += defname + ", ";
                                }
                                Log.Message(animInfo);
                            }

                            return false;
                        }
                    }
                    //genitals checking
                    if(x.actors[i].requiredGenitals != null) {
                        if (x.actors[i].requiredGenitals.Contains("Vagina")) {

                            if (!rjw.Genital_Helper.has_vagina(localParticipants[i])) {
                                Log.Message(x.defName.ToStringSafe() + " not selected -- " + localParticipants[i].def.defName.ToStringSafe() + " " + localParticipants[i].Name.ToStringSafe() + " doesn't have vagina");
                                return false;
                            }

                        }

                        if (x.actors[i].requiredGenitals.Contains("Penis")) {

                            if (!(rjw.Genital_Helper.has_multipenis(localParticipants[i]) || rjw.Genital_Helper.has_penis_infertile(localParticipants[i]) || rjw.Genital_Helper.has_penis_fertile(localParticipants[i]))) {
                                Log.Message(x.defName.ToStringSafe() + " not selected -- " + localParticipants[i].def.defName.ToStringSafe() + " " + localParticipants[i].Name.ToStringSafe() + " doesn't have penis");
                                return false;
                            }

                        }

                        if (x.actors[i].requiredGenitals.Contains("Mouth")) {

                            if (!rjw.Genital_Helper.has_mouth(localParticipants[i])) {
                                Log.Message(x.defName.ToStringSafe() + " not selected -- " + localParticipants[i].def.defName.ToStringSafe() + " " + localParticipants[i].Name.ToStringSafe() + " doesn't have mouth");
                                return false;
                            }

                        }

                        if (x.actors[i].requiredGenitals.Contains("Anus")) {

                            if (!rjw.Genital_Helper.has_anus(localParticipants[i])) {
                                Log.Message(x.defName.ToStringSafe() + " not selected -- " + localParticipants[i].def.defName.ToStringSafe() + " " + localParticipants[i].Name.ToStringSafe() + " doesn't have anus");
                                return false;
                            }

                        }

                        if(x.actors[i].requiredGenitals.Contains("Breasts")) {
                            if (!rjw.Genital_Helper.can_do_breastjob(localParticipants[i])) {
                                Log.Message(x.defName.ToStringSafe() + " not selected -- " + localParticipants[i].def.defName.ToStringSafe() + " " + localParticipants[i].Name.ToStringSafe() + " doesn't have breasts");
                                return false;
                            }
                        }

                        if (x.actors[i].requiredGenitals.Contains("NoVagina")) {

                            if (rjw.Genital_Helper.has_vagina(localParticipants[i])) {
                                Log.Message(x.defName.ToStringSafe() + " not selected -- " + localParticipants[i].def.defName.ToStringSafe() + " " + localParticipants[i].Name.ToStringSafe() + " has vagina");
                                return false;
                            }

                        }

                        if (x.actors[i].requiredGenitals.Contains("NoPenis")) {

                            if ((rjw.Genital_Helper.has_multipenis(localParticipants[i]) || rjw.Genital_Helper.has_penis_infertile(localParticipants[i]) || rjw.Genital_Helper.has_penis_fertile(localParticipants[i]))) {
                                Log.Message(x.defName.ToStringSafe() + " not selected -- " + localParticipants[i].def.defName.ToStringSafe() + " " + localParticipants[i].Name.ToStringSafe() + " has penis");
                                return false;
                            }

                        }

                        if (x.actors[i].requiredGenitals.Contains("NoMouth")) {

                            if (rjw.Genital_Helper.has_mouth(localParticipants[i])) {
                                Log.Message(x.defName.ToStringSafe() + " not selected -- " + localParticipants[i].def.defName.ToStringSafe() + " " + localParticipants[i].Name.ToStringSafe() + " has mouth");
                                return false;
                            }

                        }

                        if (x.actors[i].requiredGenitals.Contains("NoAnus")) {

                            if (rjw.Genital_Helper.has_anus(localParticipants[i])) {
                                Log.Message(x.defName.ToStringSafe() + " not selected -- " + localParticipants[i].def.defName.ToStringSafe() + " " + localParticipants[i].Name.ToStringSafe() + " has anus");
                                return false;
                            }

                        }

                        if (x.actors[i].requiredGenitals.Contains("NoBreasts")) {
                            if (rjw.Genital_Helper.can_do_breastjob(localParticipants[i])) {
                                Log.Message(x.defName.ToStringSafe() + " not selected -- " + localParticipants[i].def.defName.ToStringSafe() + " " + localParticipants[i].Name.ToStringSafe() + " has breasts");
                                return false;
                            }
                        }
                    }

                    

                    //TESTING ANIMATIONS ONLY REMEMBER TO COMMENT OUT BEFORE PUSH
                    /*
                    if (x.defName != "Missionary")
                        return false;
                    */


                    if (x.actors[i].isFucking && !rjw.xxx.can_fuck(localParticipants[i])) {
                        Log.Message(x.defName.ToStringSafe() + " not selected -- " + localParticipants[i].def.defName.ToStringSafe() + " " + localParticipants[i].Name.ToStringSafe() + " can't fuck");
                        return false;
                    }

                    if (x.actors[i].isFucked && !rjw.xxx.can_be_fucked(localParticipants[i])) {
                        Log.Message(x.defName.ToStringSafe() + " not selected -- " + localParticipants[i].def.defName.ToStringSafe() + " " + localParticipants[i].Name.ToStringSafe() + " can't be fucked");
                        return false;
                    }
                }
                return true;
            });
            List<AnimationDef> optionsWithSexType = options.ToList().FindAll(x => x.sexTypes.Contains(sexType));
            List<AnimationDef> optionsWithSexTypeAndInitiator = optionsWithSexType.FindAll(x => {
                bool initiatorsAlignWithSexType = true;
                for (int i = 0; i < x.actors.Count; i++) {

                    //if the animation not for initiators, but an initiator is playing it

                    if (x.actors[i].initiator && !(localParticipants[i].jobs.curDriver is rjw.JobDriver_SexBaseInitiator)) {
                        initiatorsAlignWithSexType = false;
                    }
                }
                return initiatorsAlignWithSexType;
            });
            List<AnimationDef> optionsWithInitiator = options.ToList().FindAll(x => {
                bool initiatorsAlignWithSexType = true;
                for (int i = 0; i < x.actors.Count; i++) {

                    //if the animation not for initiators, but an initiator is playing it

                    if (x.actors[i].initiator && !(localParticipants[i].jobs.curDriver is rjw.JobDriver_SexBaseInitiator)) {
                        initiatorsAlignWithSexType = false;
                    }
                }
                return initiatorsAlignWithSexType;
            });


            if (optionsWithSexTypeAndInitiator.Any()) {
                Log.Message("Selecting animation for rjwSexType " + sexType.ToStringSafe() + " and initiators...");
                return optionsWithSexType.RandomElement();
            }

            if (optionsWithSexType.Any()) {
                Log.Message("Selecting animation for rjwSexType " + sexType.ToStringSafe() + "...");
                return optionsWithSexType.RandomElement();
            }

            if(optionsWithInitiator.Any()) {
                Log.Message("Selecting animation for initiators...");
            }

            if (options != null && options.Any()) {
                Log.Message("Randomly selecting animation...");
                return options.RandomElement();
            }
            else
                return null;
        }

        public static void RenderPawnHeadMeshInAnimation(Mesh mesh, Vector3 loc, Quaternion quaternion, Material material, bool portrait, Pawn pawn) {

            if(pawn == null || pawn.Map != Find.CurrentMap) {
                GenDraw.DrawMeshNowOrLater(mesh, loc, quaternion, material, portrait);
                return;
            }

            CompBodyAnimator pawnAnimator = pawn.TryGetComp<CompBodyAnimator>();

            if (pawnAnimator == null || !pawnAnimator.isAnimating || portrait) {
                GenDraw.DrawMeshNowOrLater(mesh, loc, quaternion, material, portrait);
            } else {
                Vector3 pawnHeadPosition = pawnAnimator.getPawnHeadPosition();
                pawnHeadPosition.y = loc.y;
                GenDraw.DrawMeshNowOrLater(mesh, pawnHeadPosition, Quaternion.AngleAxis(pawnAnimator.headAngle, Vector3.up), material, portrait);
            }
        }

        public static void RenderPawnHeadMeshInAnimation(Mesh mesh, Vector3 loc, Quaternion quaternion, Material material, bool portrait, Pawn pawn, float bodySizeFactor = 1) {

            if (pawn == null) {
                GenDraw.DrawMeshNowOrLater(mesh, loc, quaternion, material, portrait);
                return;
            }

            CompBodyAnimator pawnAnimator = pawn.TryGetComp<CompBodyAnimator>();

            if (pawnAnimator == null || !pawnAnimator.isAnimating || portrait) {
                GenDraw.DrawMeshNowOrLater(mesh, loc, quaternion, material, portrait);
            }
            else {
                Vector3 pawnHeadPosition = pawnAnimator.getPawnHeadPosition();
                pawnHeadPosition.x *= bodySizeFactor;
                pawnHeadPosition.x *= bodySizeFactor;
                pawnHeadPosition.y = loc.y;
                GenDraw.DrawMeshNowOrLater(mesh, pawnHeadPosition, Quaternion.AngleAxis(pawnAnimator.headAngle, Vector3.up), material, portrait);
            }
        }
    }
}
