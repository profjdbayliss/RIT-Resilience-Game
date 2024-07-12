using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Facility : MonoBehaviour
{
    public string sector;
    public string facilityName;
    public int networkPoints;
    public int physicalPoints;
    public int financialPoints;
    public List<Effect> effects = new List<Effect>();

    public List<Facility> dependentFacilities = new List<Facility>();

    public bool negatePointsReduction;
    public List<Effect> negateEffects = new List<Effect>();


    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void SpreadEffect(string effectType)
    {
        // Find the effect in the current facility
        Effect sourceEffect = effects.FirstOrDefault(e => e.type == effectType);
        if (sourceEffect != null)
        {
            // Iterate over all dependent facilities
            foreach (Facility dependentFacility in dependentFacilities)
            {
                // Check if the dependent facility already has this effect
                Effect existingEffect = dependentFacility.effects.FirstOrDefault(e => e.type == effectType);
                if (existingEffect != null)
                {
                    // If it does and the source effect has a longer duration, update the duration
                    if (existingEffect.duration < sourceEffect.duration)
                    {
                        existingEffect.duration = sourceEffect.duration;
                    }
                }
                else
                {
                    // If not, add a new effect with the same type and duration
                    dependentFacility.effects.Add(new Effect { type = sourceEffect.type, duration = sourceEffect.duration });
                }
            }
        }
    }
}
