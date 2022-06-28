using System;
using System.Collections.Generic;

namespace NetworkUtils
{
    public class Property
    {
        public object Value { get; private set; }
        private readonly List<Tuple<string, object>> Settings;
        public enum PropertyType { String, Integer, Decimal, Bool }
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
                case PropertyType.Bool:
                    SetBool((bool)value); break;
                default:
                    throw new NotImplementedException();
            }
        }

        public (bool, string) TrySetValue(object value)
        {
            switch (ValueType)
            {
                case PropertyType.String:
                    SetString((string)value); return (true, "");
                case PropertyType.Integer:
                    int testInt;
                    if (int.TryParse(value.ToString(), out testInt))
                    {
                        SetInteger(testInt); return (true, "");
                    }
                    else return (false, "Property is integer type!");
                case PropertyType.Decimal:
                    decimal testDecimal;
                    if (decimal.TryParse(value.ToString(), out testDecimal))
                    {
                        SetDecimal(testDecimal); return (true, "");
                    }
                    else return (false, "Property is decimal type!");
                case PropertyType.Bool:
                    bool testBool;
                    if(bool.TryParse(value.ToString(), out testBool))
                    {
                        SetBool(testBool); return (true, "");
                    }
                    else return(false, "Property is bool type!");
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
        private void SetBool(bool value)
        {
            Value = value;
        }
        //Settings
        public const string DECIMAL_MIN = "Min";
        public const string DECIMAL_MAX = "Max";
        public const string INTEGER_MIN = "Min";
        public const string INTEGER_MAX = "Max";

        //Properties
        public const string CyclesToUpdate = "CyclesToUpdate";
        public const string Router = "Router";
        public const string Probability = "Probability";
        public const string LearningWeight = "LearningWeight";
        public const string LoadAllValues = "LoadAllValues";
    }
}
