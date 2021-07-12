using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace MatchTables.Helpers
{
    public class DictionaryCompare : IEqualityComparer<Dictionary<string, object>>
    {
        public bool Equals([AllowNull] Dictionary<string, object> x, [AllowNull] Dictionary<string, object> y)
        {
            if (x.Count != y.Count)
                return false;
           
            foreach (var xKeyValue in x)
            {
                var yKeyValue = y.FirstOrDefault(k => k.Key == xKeyValue.Key);
                if (yKeyValue.Key == null)
                    return false;
                if (!xKeyValue.Value.Equals(yKeyValue.Value))
                    return false;
            }
            return true;
        }
        public int GetHashCode([DisallowNull] Dictionary<string, object> obj)
        {
            return obj.Aggregate(123, (current, keyValue) => current + (keyValue.GetHashCode() * 31));
        }
    }
}
