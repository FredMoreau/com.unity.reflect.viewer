using System;
using UnityEngine;

namespace Unity.Reflect.Viewer.UI
{
    [Serializable]
    public struct PopulateOptionData : IEquatable<SceneOptionData>
    {
        // View Options
        public bool enableTrees; // DEMO_MOD
        
        public bool Equals(SceneOptionData other)
        {
            return enableTrees == other.enableTrees;
        }

        public override bool Equals(object obj)
        {
            return obj is PopulateOptionData other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = enableTrees.GetHashCode();
                //hashCode = (hashCode * 397) ^ enableLightData.GetHashCode();
                return hashCode;
            }
        }
        
        public static bool operator ==(PopulateOptionData a, PopulateOptionData b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(PopulateOptionData a, PopulateOptionData b)
        {
            return !(a == b);
        }
    }
}
