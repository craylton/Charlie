using System;
using System.Collections.Generic;
using System.Linq;

namespace EvalComparisons.Graph
{
    public class GraphType
    {
        private string typeName;
        public string TypeName
        {
            get => typeName;
            set
            {
                if (AllowedGraphTypes.Contains(value))
                    typeName = value;
            }
        }

        public IEnumerable<string> AllowedGraphTypes { get; } = new[] { "Score", "Material" };

        public GraphType() => TypeName = AllowedGraphTypes.First();

        public int GetGraphTypeValue(int truthScore, int material) => TypeName switch
        {
            "Score" => truthScore,
            "Material" => material,
            _ => throw new Exception($"{TypeName} not a valid comparison keyword"),
        };
    }
}
