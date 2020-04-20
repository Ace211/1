using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Rimworld_Animations {
    public class Actor {
        public List<string> defNames;
        public List<string> requiredGenitals;
        public List<AlienRaceOffset> raceOffsets;
        public bool initiator = false;
        public string gender;
        public bool isFucking = false;
        public bool isFucked = false;
        public Vector3 offset = new Vector3(0, 0, 0);
    }
}
