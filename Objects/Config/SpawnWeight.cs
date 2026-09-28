using Dawn;
using Dusk.Weights;
using System;
using System.Collections.Generic;
using System.Text;

namespace LunarConfig.Objects.Config
{
    public enum WeightCondition
    {
        Basic,
        Price
    }

    public class SpawnWeight
    {
        public WeightCondition type = WeightCondition.Basic;
        public NamespacedKey? source;
        public float weight;
        public MathOperation operation;
        public IntComparison? intComp;

        public SpawnWeight()
        {

        }

        public SpawnWeight(NamespacedKey key, MathOperation op, float val)
        {
            source = key;
            operation = op;
            weight = val;
        }

        public SpawnWeight(WeightCondition t, IntComparison comp, MathOperation op, float val)
        {
            type = t;
            intComp = comp;
            operation = op;
            weight = val;
        }

        public override string ToString()
        {
            if (type == WeightCondition.Basic)
            {
                string c = "";

                switch (operation)
                {
                    case MathOperation.Additive: c = ""; break;
                    case MathOperation.Subtractive: c = "-"; break;
                    case MathOperation.Multiplicative: c = "*"; break;
                    case MathOperation.Divisive: c = "/"; break;
                }

                return ":" + c + ((operation == MathOperation.Additive || operation == MathOperation.Subtractive) ? (int)weight : weight);
            }
            else if (type == WeightCondition.Price)
            {
                string c = "";

                switch (intComp.ComparisonOperation)
                {
                    case ComparisonOperation.Equal: c = "="; break;
                    case ComparisonOperation.NotEqual: c = "!="; break;
                    case ComparisonOperation.GreaterOrEqual: c = ">="; break;
                    case ComparisonOperation.LessOrEqual: c = "<="; break;
                    case ComparisonOperation.Greater: c = ">"; break;
                    case ComparisonOperation.Less: c = "<"; break;
                }

                string m = "";

                switch (operation)
                {
                    case MathOperation.Additive: m = "+"; break;
                    case MathOperation.Subtractive: m = "-"; break;
                    case MathOperation.Multiplicative: m = "*"; break;
                    case MathOperation.Divisive: m = "/"; break;
                }


                return c + ((operation == MathOperation.Additive || operation == MathOperation.Subtractive) ? (int)weight : weight) + ":" + m;
            }
        }
    }
}
