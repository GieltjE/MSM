using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;

namespace MSM.Extends
{
    public class EnumDescriptionConverter<T> : EnumConverter
    {
        private readonly Type _enumType;
        public EnumDescriptionConverter() : base(typeof(T))
        {
            _enumType = typeof(T);
        }

        public override Boolean CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(String);
        }
        public override Object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, Object value)
        {
            foreach (FieldInfo field in _enumType.GetFields())
            {
                if (Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) is DescriptionAttribute attribute)
                {
                    if (attribute.Description == (String)value)
                    {
                        return (T)field.GetValue(null);
                    }
                }
                else
                {
                    if (field.Name == (String)value)
                    {
                        return (T)field.GetValue(null);
                    }
                }
            }
            return null;
        }
        public override Boolean CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return destinationType == typeof(String);
        }

        public override Object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, Object value, Type destType)
        {
            DescriptionAttribute descriptionAttribute = (DescriptionAttribute)Attribute.GetCustomAttribute(_enumType.GetField(Enum.GetName(_enumType, value)), typeof(DescriptionAttribute));

            return descriptionAttribute != null ? descriptionAttribute.Description : value.ToString();
        }
    }
    public class BooleanYesNoConverter : BooleanConverter
    {
        public override Object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, Object value, Type destType)
        {
            return (Boolean)value ? "Yes" : "No";
        }
        public override Object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, Object value)
        {
            return (String)value == "Yes";
        }
    }
}