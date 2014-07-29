using ProtoBuf;
using System.Collections.Generic;
using UnityEngine;

// This sample demonstrates how you could implement a skill system using inheritance

namespace FullInspector.Samples {
    [ProtoContract]
    [ProtoInclude(100, typeof(FireSkill))]
    [ProtoInclude(101, typeof(IceSkill))]
    [ProtoInclude(102, typeof(MultiSkill))]
    public abstract class BaseSkill {
        [ProtoMember(1)]
        public string Name;
    }

    [ProtoContract]
    public class FireSkill : BaseSkill {
        [ProtoMember(1)]
        public float Strength;
    }

    [ProtoContract]
    public class IceSkill : BaseSkill {
        [ProtoMember(1)]
        public float Duration;

        [ProtoMember(2)]
        public bool EnableSlow;
    }

    [ProtoContract]
    public class MultiSkill : BaseSkill {
        [ProtoMember(1)]
        public List<BaseSkill> Skills;

        public MultiSkill(IEnumerable<BaseSkill> skills) {
            Skills = new List<BaseSkill>(skills);
        }
    }

    [AddComponentMenu("Full Inspector Samples/Other/Skill System")]
    public class SkillsSample : BaseBehavior {
        public List<BaseSkill> Skills;

        [ContextMenu("Add Multi Skill")]
        public void AddNonDefaultCtorSkill() {
            Skills.Add(new MultiSkill(new List<BaseSkill>() { new FireSkill(), new IceSkill() }));
        }
    }
}