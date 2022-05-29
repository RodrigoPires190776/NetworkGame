using System;
using System.Collections.Generic;
using static Network.Strategies.Property;

namespace Network.Strategies
{
    public abstract class BaseStrategy
    {
        public Dictionary<string, Property> Properties { get; }

        public BaseStrategy(List<Tuple<string , PropertyType, List<Tuple<string, object>>>> properties)
        {
            Properties = new Dictionary<string, Property>();

            foreach(var property in properties)
            {
                Properties.Add(property.Item1, new Property(property.Item2, property.Item3));
            }
        }
    }

    public class Property
    {
        public object Value { get; private set; }
        private readonly List<Tuple<string, object>> Settings;
        public enum PropertyType { String, Integer, Decimal }
        public PropertyType ValueType { get; }
        public Property(PropertyType type, List<Tuple<string, object>> settings = null)
        {
            ValueType = type;
            if (settings == null) Settings = new List<Tuple<string, object>>();
            else Settings = settings;
        }

        public void SetValue(object value)
        {
            switch (ValueType)
            {
                case PropertyType.String:
                    SetString((string)value); break;
                case PropertyType.Integer:
                    SetInteger((int)value); break;
                case PropertyType.Decimal:
                    SetDecimal((decimal)value); break;
                default: 
                    throw new NotImplementedException();
            }
        }

        private void SetString(string value)
        {
            Value = value;
        }
        private void SetInteger(int value)
        {
            Value = value;
        }
        private void SetDecimal(decimal value)
        {
            foreach(var setting in Settings)
            {
                switch (setting.Item1)
                {
                    case DECIMAL_MIN:
                        if (value < (decimal)setting.Item2) throw new Exception();
                        break;
                    case DECIMAL_MAX:
                        if (value > (decimal)setting.Item2) throw new Exception();
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
            Value = value;
        }

        public const string DECIMAL_MIN = "Min";
        public const string DECIMAL_MAX = "Max";
    }
}
