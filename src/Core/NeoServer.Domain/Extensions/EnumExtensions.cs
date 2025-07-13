using System.ComponentModel;
using System.Reflection;

namespace NeoServer.Domain.Extensions;

public static class EnumExtensions
{
    public static string GetDescription(this Enum value)
    {
        FieldInfo field = value.GetType().GetField(value.ToString());
        if (field == null)
            return value.ToString();

        var attribute = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;
        return attribute?.Description ?? value.ToString();
    }

    public static TEnum FromDescription<TEnum>(string description) where TEnum : Enum
    {
        var type = typeof(TEnum);
        foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            var attribute = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;
            if (attribute != null && attribute.Description == description)
                return (TEnum)field.GetValue(null);

            if (field.Name.Equals(description, StringComparison.OrdinalIgnoreCase))
                return (TEnum)field.GetValue(null);
        }

        throw new ArgumentException($"'{description}' is not a valid description for {type.Name}");
    }
}
