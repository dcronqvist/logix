using System.Collections.Generic;
using System.Linq;
using NLua;

namespace LogiX.Scripting;

public static class LuaServiceHelpers
{
    public static void ExtendData(LuaTable table1, LuaTable table2)
    {
        if (table1.Keys.Cast<object>().All(x => x is long) && table2.Keys.Cast<object>().All(x => x is long))
        {
            // If all keys are integers, we just concat the two tables
            long maxTable1Key = table1.Keys.Count == 0 ? 0 : table1.Keys.Cast<long>().Max();
            foreach (KeyValuePair<object, object> kvp in table2)
            {
                table1[maxTable1Key + ((long)kvp.Key)] = kvp.Value;
            }

            return;
        }

        throw new System.NotImplementedException("ExtendData is not implemented for non-integer keys");
    }
}
