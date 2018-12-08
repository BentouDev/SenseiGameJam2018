using System.Linq;
using Framework;
using UnityEngine;

namespace Data.Scripts
{
    public class PointOfTakenProvider : BaseBehaviour
    {
        public Transform PointOfTaken;
        public string SlotName;

        public Vector3 Offset;

        public Transform Provide()
        {
            if (!PointOfTaken)
                foreach (var slots in GetComponentsInChildren<CustomProperties>())
                {
                    if (slots.Properties.Any(p => p.Name == SlotName))
                    {
                        PointOfTaken = slots.transform;
                        break;
                    }
                }

            return PointOfTaken;
        }
    }
}