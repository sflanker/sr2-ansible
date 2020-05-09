using System;
using ModApi.Craft.Parts;
using ModApi.Craft.Parts.Attributes;
using UnityEngine;

namespace Assets.Scripts.Craft.Parts.Modifiers {
    [Serializable]
    [DesignerPartModifier("PhiloticParallax")]
    [PartModifierTypeId("Ansible.PhiloticParallax")]
    public class PhiloticParallaxData : PartModifierData<PhiloticParallaxScript> {
        [SerializeField]
        [PartModifierProperty]
        private Single _powerConsumptionPerMessage = 1000;

        /// <summary>
        /// The amount of power consumed per message sent in kilowatt seconds.
        /// </summary>
        public Single PowerConsumptionPerMessage => _powerConsumptionPerMessage;
    }
}
