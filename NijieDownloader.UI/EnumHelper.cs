using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace NijieDownloader.UI
{
    public static class EnumHelper
    {
        public static string Description(this Enum eValue)
        {
            var nAttributes = eValue.GetType().GetField(eValue.ToString()).GetCustomAttributes(typeof(DescriptionAttribute), false);

            if (!nAttributes.Any())
                return eValue.ToString();

            return (nAttributes.First() as DescriptionAttribute).Description;
        }

        public static IEnumerable<ValueDescription> GetAllValuesAndDescriptions<TEnum>() where TEnum : struct, IConvertible, IComparable, IFormattable
        {
            if (!typeof(TEnum).IsEnum)
                throw new ArgumentException("TEnum must be an Enumeration type");

            return from e in Enum.GetValues(typeof(TEnum)).Cast<Enum>()
                   select new ValueDescription() { Value = e, Description = e.Description() };
        }
    }

    public class ValueDescription
    {

        public Enum Value { get; set; }

        public string Description { get; set; }
    }
}
