using System.Collections.Generic;

namespace Assets.castle.Scripts
{

    public struct StatModifier
    {
        public string Name;
        public float PercentMod;
        public float StaticMod;
        public float Duration;

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
}
