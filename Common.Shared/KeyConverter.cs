using System.ComponentModel;
using System.Globalization;

namespace Common.Shared;

public class KeyConverter : TypeConverter
{
    // Overrides the CanConvertFrom method of TypeConverter.
    // The ITypeDescriptorContext interface provides the context for the
    // conversion. Typically, this interface is used at design time to
    // provide information about the design-time container.
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
    {
        if (sourceType == typeof(string))
            return true;
        return base.CanConvertFrom(context, sourceType);
    }

    // Overrides the ConvertFrom method of TypeConverter.
    public override object? ConvertFrom(
        ITypeDescriptorContext? context,
        CultureInfo? culture,
        object value
    )
    {
        if (value is string s)
            return new Key(s);
        return base.ConvertFrom(context, culture, value);
    }

    // Overrides the ConvertTo method of TypeConverter.
    public override object? ConvertTo(
        ITypeDescriptorContext? context,
        CultureInfo? culture,
        object? value,
        Type destinationType
    )
    {
        if (destinationType == typeof(string) && value is not null)
            return ((Key)value).Value;
        return base.ConvertTo(context, culture, value, destinationType);
    }
}
