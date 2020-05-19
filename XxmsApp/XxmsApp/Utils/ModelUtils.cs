using System;
using System.Collections.Generic;
using System.Text;

namespace XxmsApp
{
    public static class ModelUtils
    {
        public static bool ToBoolean(this string value)
        {
            if (bool.TryParse(value, out bool result))
            {
                return result;
            }
            else return false;
        }
    }
}
