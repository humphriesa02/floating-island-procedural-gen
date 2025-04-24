using Unity.Mathematics;
using UnityEngine;


public class IslandStats
{
    public float People {get; private set;}
    public float Defense {get; private set;}
    public float Food {get; private set;}
    public float Danger {get; private set;}

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
}
