using UnityEngine;


public class IslandStats
{
    public float People {get; private set;}
    public float Defense {get; private set;}
    public float Food {get; private set;}
    public float Danger {get; private set;}

    public string Affinity {get; private set;}

    public void AddFromStructure(Structure s, float scaleModifier){
        People += s.People * scaleModifier;
        Defense += s.Defense * scaleModifier;
        Food += s.Food * scaleModifier;
        Danger += s.Danger * scaleModifier;
    }
    public void RemoveFromStructure(Structure s){
        People -= s.People;
        Defense -= s.Defense;
        Food -= s.Food;
        Danger -= s.Danger;
    }

    public void ResolveConflicts()
    {
        People = Mathf.Max(0, People - Danger * 0.5f);
        Food = Mathf.Max(0, Food - People * 0.3f);
        Danger = Mathf.Max(0, Danger - Defense * 0.6f);
        Defense = Mathf.Max(0, Defense - Food * 0.3f);
    }

    public void CalculateAffinity()
    {
        float max = Mathf.Max(People, Defense, Food, Danger);
        if (max == People) Affinity = "People";
        else if (max == Defense) Affinity = "Defense";
        else if (max == Food) Affinity = "Food";
        else Affinity = "Danger";
    }
}
