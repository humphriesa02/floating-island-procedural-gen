using UnityEngine;
using System.Collections.Generic;

public class IslandEvaluator : MonoBehaviour
{
    public class Evaluation
    {
        public List<string> Traits = new();
        public bool IsRare = false;
        public float BalanceScore;
        public string Affinity;
    }

    public Evaluation Evaluate(IslandStats stats)
    {
        var result = new Evaluation();

        result.Affinity = GetAffinity(stats);

        // --- Trait Detection ---
        if (stats.Food > stats.People * 2)
            result.Traits.Add("Abundant");

        if (stats.Defense > stats.Danger * 2)
            result.Traits.Add("Fortified");

        if (stats.Danger > stats.Defense * 2)
        {
            result.Traits.Add("Overrun");
        }

        // if (stats.people > 10 && stats.food > 10 && stats.defense > 10)
        //     result.IsRare = true;

        // --- Balance Metric ---
        float dangerBalance = Mathf.Abs(stats.Danger - stats.Defense);
        float hungerBalance = Mathf.Abs(stats.People - stats.Food);

        result.BalanceScore = dangerBalance + survivalBalance;

        return result;
    }

    public string GetAffinity(IslandStats stats)
    {
        if (stats.Food > stats.People * 2)
            return "Food";

        if (stats.Defense > stats.Danger * 2)
            return "Defense";

        if (stats.Danger > stats.Defense * 2)
            return "Danger";

        return "None";
    }
}
