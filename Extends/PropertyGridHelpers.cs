// 
// This file is a part of MSM (Multi Server Manager)
// Copyright (C) 2016-2018 Michiel Hazelhof (michiel@hazelhof.nl)
// 
// MSM is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// MSM is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// If not, see <http://www.gnu.org/licenses/>.
// 

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
using MSM.Functions;
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
            return String.Equals((String)value, "Yes", StringComparison.Ordinal);
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
            FireAddedOrRemoved();
        }
        public void Remove(T item)
        {
            List.Remove(item);
            FireAddedOrRemoved();
        }
        // ReSharper restore UnusedMember.Global

        protected override void OnRemoveComplete(Int32 index, Object value)
        {
            base.OnRemoveComplete(index, value);
            FireAddedOrRemoved();
        }
        protected override void OnInsertComplete(Int32 index, Object value)
        {
            base.OnInsertComplete(index, value);
            FireAddedOrRemoved();
        }

        public event ExtensionMethods.CustomDelegate AddedOrRemoved;
        public void FireAddedOrRemoved()
        {
            AddedOrRemoved?.Invoke();
        }
        public Boolean AddedOrRemovedSet()
        {
            return AddedOrRemoved != null;
        }

        public T[] ToArray()
        {
            return List.Cast<T>().ToArray();
        }
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

        public readonly Dictionary<String, String> Values = new Dictionary<String, String>(StringComparer.Ordinal);
        public Variable[] Properties
        {
            get
            {
                List<Variable> toReturn = new List<Variable>();
                foreach (KeyValuePair<String, String> keyValuePair in Values)
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
                Values.Clear();
                foreach (Variable variable in value)
                {
                    Values.Add(variable.Key, variable.Value);
                }
            }
        }
        public Object this[String key]
        {
            get => Values.TryGetValue(key, out String value) ? value : null;
            set { if (value == null) Values.Remove(key); else Values[key] = (String)value; }
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
                    properties.AddRange(Settings.Values.Variables.Select(variable => new PropertyBagDescriptor(variable, typeof(String), null)));
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