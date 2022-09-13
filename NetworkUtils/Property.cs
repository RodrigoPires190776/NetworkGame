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
            bool initSet = false;
            foreach(var property in Settings)
            {
                if(property.Item1 == INITIAL_VALUE)
                {
                    initSet = true;
                    SetValue(property.Item2);
                }
            }
            if(!initSet) SetInitValue();
        }

        private void SetInitValue()
        {
            switch (ValueType)
            {
                case PropertyType.String:
                    SetString(""); break;
                case PropertyType.Integer:
                    SetInteger(0); break;
                case PropertyType.Decimal:
                    SetDecimal(0m); break;
                case PropertyType.Bool:
                    SetBool(false); break;
                default:
                    throw new NotImplementedException();
            }
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
                        try { SetInteger(testInt); return (true, ""); }
                        catch(Exception e) { return (false, e.Message); }
                    }
                    else return (false, "Property is integer type!");
                case PropertyType.Decimal:
                    decimal testDecimal;
                    if (decimal.TryParse(value.ToString(), out testDecimal))
                    {
                        try { SetDecimal(testDecimal); return (true, ""); }
                        catch(Exception e) { return (false, e.Message); }
                    }
                    else return (false, "Property is decimal type!");
                case PropertyType.Bool:
                    bool testBool;
                    if(bool.TryParse(value.ToString(), out testBool))
                    {   
                        try { SetBool(testBool); return (true, ""); }
                        catch (Exception e) { return (false, e.Message); }
                    }
                    else return(false, "Property is bool type!");
                default:
                    throw new NotImplementedException();
            }
        }

        private void SetString(string value)
        {
            foreach (var setting in Settings)
            {
                switch (setting.Item1)
                { 
                    case INITIAL_VALUE:
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
            Value = value;
        }
        private void SetInteger(int value)
        {
            foreach (var setting in Settings)
            {
                switch (setting.Item1)
                {
                    case INTEGER_MIN:
                        if (value < (int)setting.Item2) throw new Exception($"Minimum value is {(int)setting.Item2}");
                        break;
                    case INTEGER_MAX:
                        if (value > (int)setting.Item2) throw new Exception($"Maximum value is {(int)setting.Item2}");
                        break;
                    case INITIAL_VALUE:
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
                        if (value < (decimal)setting.Item2) throw new Exception($"Minimum value is {(decimal)setting.Item2}");
                        break;
                    case DECIMAL_MAX:
                        if (value > (decimal)setting.Item2) throw new Exception($"Maximum value is {(decimal)setting.Item2}");
                        break;
                    case INITIAL_VALUE:
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
        public const string INITIAL_VALUE = "InitialValue";

        //Properties
        public const string CyclesToUpdate = "CyclesToUpdate";
        public const string Router = "Router";
        public const string Probability = "Probability";
        public const string Percentage = "Percentage";
        public const string LearningWeight = "LearningWeight";
        public const string PenaltyWeight = "PenaltyWeight";
        public const string LoadAllValues = "LoadAllValues";
        public const string Attacker = "Attacker";
        public const string Defensor = "Defensor";
        public const string Destination = "Destination";
        public const string Random = "Random";
        public const string MinProbability = "MinProbability";
        public const string MaxProbability = "MaxProbability";
        public const string NumberOfNodes = "NumberOfNodes";
        public const string ProbabilityOfLink = "ProbabilityOfLink";
        public const string ShortestLinkLength = "ShortestLinkLength";
    }
}
