using System;
using System.Collections.Generic;

namespace NetworkUtils
{
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
            foreach (var setting in Settings)
            {
                switch (setting.Item1)
                {
                    case INTEGER_MIN:
                        if (value < (int)setting.Item2) throw new Exception();
                        break;
                    case INTEGER_MAX:
                        if (value > (int)setting.Item2) throw new Exception();
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
            Value = value;
        }
        private void SetDecimal(decimal value)
        {
            foreach (var setting in Settings)
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
        public const string INTEGER_MIN = "Min";
        public const string INTEGER_MAX = "Max";
    }
}
