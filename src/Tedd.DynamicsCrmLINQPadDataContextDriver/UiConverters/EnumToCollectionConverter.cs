using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;

namespace Tedd.DynamicsCrmLINQPadDataContextDriver.UiConverters
{
    // From https://stackoverflow.com/questions/58743/databinding-an-enum-property-to-a-combobox-in-wpf
    // Nice
    public static class EnumHelper
    {
        public static string Description(this Enum e)
        {
            return (e.GetType()
                       .GetField(e.ToString())
                       .GetCustomAttributes(typeof(DescriptionAttribute), false)
                       .FirstOrDefault() as DescriptionAttribute)?.Description ?? e.ToString();
        }
    }
    [ValueConversion(typeof(Enum), typeof(IEnumerable<ValueDescription>))]
    public class EnumToCollectionConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Enum.GetValues(value.GetType())
                .Cast<Enum>()
                .Select(e => new ValueDescription() { Value = e, Description = e.Description() })
                .ToList();
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }

    public class ValueDescription
    {
        public Enum Value { get; set; }
        public string Description { get; set; }
    }
}
