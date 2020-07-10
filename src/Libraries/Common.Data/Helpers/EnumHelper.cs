using System;
using System.ComponentModel;

namespace Common.Data.Helpers
{
    public class EnumHelper
    {
        public string EnumDescription(Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            var fi = field;
            var attributes =
                (DescriptionAttribute[])fi.GetCustomAttributes(
                    typeof(DescriptionAttribute), false);

            return attributes.Length > 0 ? attributes[0].Description : value.ToString();
        }
    }
}
