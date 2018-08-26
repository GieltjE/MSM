using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using MSM.Data;
using MSM.Service;

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
            DescriptionAttribute descriptionAttribute = null;
            if (value != null)
            {
                descriptionAttribute = (DescriptionAttribute)Attribute.GetCustomAttribute(_enumType.GetField(Enum.GetName(_enumType, value)), typeof(DescriptionAttribute));
            }

            // ReSharper disable once PossibleNullReferenceException
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
    public class CsvConverter : TypeConverter
    {
        // Overrides the ConvertTo method of TypeConverter.
        public override Object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, Object value, Type destinationType)
        {
            if (!(value is String[] values)) return "";

            if (destinationType == typeof(String))
            {
                return String.Join(",", values);
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
        public override Object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, Object value)
        {
            if (value == null) return new String[0];
            return ((String)value).Split(',');
        }
    }
    public class CollectionConverter<T> : CollectionBase
    {
        public T this[Int32 index] => (T)List[index];

        // ReSharper disable UnusedMember.Global
        public void Add(T item)
        {
            List.Add(item);
        }
        public void Remove(T item)
        {
            List.Remove(item);
        }
        // ReSharper restore UnusedMember.Global
    }
    public class CheckedListBoxUITypeEditor : UITypeEditor
    {
        private readonly CheckedListBox _checkedListBox = new CheckedListBox();
        private IWindowsFormsEditorService _iWindowsFormsEditorService;
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.DropDown;
        }
        public override Boolean IsDropDownResizable => true;

        public override Object EditValue(ITypeDescriptorContext context, IServiceProvider provider, Object value)
        {
            // ReSharper disable PossibleNullReferenceException
            IEnumerable<ArgumentsAttribute> propertyAttributes = context.PropertyDescriptor.Attributes.OfType<ArgumentsAttribute>();
            // ReSharper restore PossibleNullReferenceException
            if (_iWindowsFormsEditorService == null)
            {
                _iWindowsFormsEditorService = provider?.GetService(typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;
            }
            if (_iWindowsFormsEditorService != null)
            {
                LoadValues((String[])value, propertyAttributes.First().CheckedListBoxSetting);
                _iWindowsFormsEditorService.DropDownControl(_checkedListBox);
            }

            return _checkedListBox.CheckedItems.OfType<String>().ToArray();
        }

        public void LoadValues(String[] checkedItems, Enumerations.CheckedListBoxSetting checkedListBoxSetting)
        {
            String[] items = new String[0];
            switch (checkedListBoxSetting)
            {
                case Enumerations.CheckedListBoxSetting.ServerKeywords:
                    items = Settings.Values.Keywords;
                    break;
            }

            items = items.OrderBy(x => x).ToArray();

            _checkedListBox.Items.Clear();
            foreach (String item in items)
            {
                _checkedListBox.Items.Add(item, checkedItems != null && checkedItems.Contains(item));
            }
        }
    }
    public class ArgumentsAttribute : Attribute
    {
        public Enumerations.CheckedListBoxSetting CheckedListBoxSetting { get; }
        public ArgumentsAttribute(Enumerations.CheckedListBoxSetting checkedListBoxSetting)
        {
            CheckedListBoxSetting = checkedListBoxSetting;
        }
    }
    [Serializable]
    public class Variable
    {
        public String Key;
        public String Value;
    }
    [TypeConverter(typeof(BasicPropertyBagConverter))]
    public class BasicPropertyBag
    {
        public override String ToString()
        {
            return "(variables)";
        }

        private readonly Dictionary<String, Object> _values = new Dictionary<String, Object>();

        public Variable[] Properties
        {
            get
            {
                List<Variable> toReturn = new List<Variable>();
                foreach (KeyValuePair<String, Object> keyValuePair in _values)
                {
                    if (keyValuePair.Value != null && !String.IsNullOrWhiteSpace((String)keyValuePair.Value))
                    {
                        toReturn.Add(new Variable { Key = keyValuePair.Key, Value = (String)keyValuePair.Value });
                    }
                }
                return toReturn.ToArray();
            }
            set
            {
                _values.Clear();
                foreach (Variable variable in value)
                {
                    _values.Add(variable.Key, variable.Value);
                }
            }
        }
        public Object this[String key]
        {
            get => _values.TryGetValue(key, out Object value) ? value : null;
            set { if (value == null) _values.Remove(key); else _values[key] = value; }
        }
    }
    public class BasicPropertyBagConverter : ExpandableObjectConverter
    {
        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, Object value, Attribute[] attributes)
        {
            // ReSharper disable PossibleNullReferenceException
            Enumerations.CheckedListBoxSetting setting = context.PropertyDescriptor.Attributes.OfType<ArgumentsAttribute>().First().CheckedListBoxSetting;
            // ReSharper restore PossibleNullReferenceException

            List<PropertyDescriptor> properties = new List<PropertyDescriptor>();
            switch (setting)
            {
                case Enumerations.CheckedListBoxSetting.ServerVariables:
                    foreach (String variable in Settings.Values.Variables)
                    {
                        properties.Add(new PropertyBagDescriptor(variable, typeof(String), new Attribute[] { new CategoryAttribute("Personal") }));
                    }
                    break;
            }

            return new PropertyDescriptorCollection(properties.ToArray());
        }
    }
    public class PropertyBagDescriptor : PropertyDescriptor
    {
        public PropertyBagDescriptor(String name, Type type, Attribute[] attributes) : base(name, attributes)
        {
            PropertyType = type;
        }
        public override Type PropertyType { get; }

        public override Object GetValue(Object component) { return ((BasicPropertyBag)component)[Name]; }
        public override void SetValue(Object component, Object value) { ((BasicPropertyBag)component)[Name] = (String)value; }
        public override Boolean ShouldSerializeValue(Object component) { return GetValue(component) != null; }
        public override Boolean CanResetValue(Object component) { return true; }
        public override void ResetValue(Object component) { SetValue(component, null); }
        public override Boolean IsReadOnly => false;
        public override Type ComponentType => typeof(BasicPropertyBag);
    }
}