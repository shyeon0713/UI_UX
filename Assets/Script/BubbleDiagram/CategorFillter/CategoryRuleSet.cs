using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CategoryRuleSet", menuName = "Scriptable Objects/CategoryRuleSet")]
public class CategoryRuleSet : ScriptableObject
{
    public List<CategoryRule> rules = new List<CategoryRule>();
}
