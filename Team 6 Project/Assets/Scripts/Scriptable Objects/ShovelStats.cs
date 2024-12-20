using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Shovel Stats", menuName = "Scriptables/Shovel Stats", order = 2)]
public class ShovelStats : ScriptableObject
{
    public GameObject shovelModel;
    [Range(1, 15)] public int shovelDMG, shovelDist;
    [Range(0.1f, 10.5f)] public float swingRate, shovelKnockback, knockbackStunDuration;
    [Range(1, 50)] public int durability;
}