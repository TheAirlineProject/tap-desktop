using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Markup;

namespace TheAirline.Infrastructure.Extensions
{
    public class EnumListExtension : MarkupExtension
    {
        private Type _enumType;

        public EnumListExtension()
        {
        }

        public EnumListExtension(Type enumType)
        {
            EnumType = enumType;
        }

        public Type EnumType
        {
            get { return _enumType; }
            set
            {
                if (value != _enumType)
                {
                    if (value != null)
                    {
                        var enumType = Nullable.GetUnderlyingType(value) ?? value;
                        if (!enumType.IsEnum)
                        {
                            throw new ArgumentException("Type must be an enumeration.");
                        }

                        _enumType = value;
                    }
                }
            }
        }

        public bool AsString { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (_enumType == null)
            {
                throw new InvalidOperationException("Enumeration must be specified.");
            }

            var actualType = Nullable.GetUnderlyingType(_enumType) ?? _enumType;
            var values = Enum.GetValues(actualType);

            if (!AsString)
            {
                if (actualType == _enumType)
                {
                    return values;
                }

                var temp = Array.CreateInstance(actualType, values.Length + 1);
                values.CopyTo(temp, 1);

                return temp;
            }

            var items = new List<string>();

            if (actualType != _enumType)
                items.Add(null);

            foreach (var item in Enum.GetValues(_enumType))
            {
                var itemString = item.ToString();
                var field = _enumType.GetField(itemString);
                var attribs = field.GetCustomAttributes(typeof (DescriptionAttribute), false);

                if (attribs.Length > 0)
                    itemString = ((DescriptionAttribute) attribs[0]).Description;

                items.Add(itemString);
            }

            return items.ToArray();
        }
    }
}