using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Monitoring.Infrastructure
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ExcludeFromInfluxAttribute : Attribute
    {
    }

    public static class InfluxKeyValueHelper
    {
        public static IDictionary<string,string> GetInfluxKeyValuesFromPoco(object poco)
        {
            var result = new Dictionary<string, string>();

            var type = poco.GetType();
            foreach(var property in type.GetProperties())
            {
                if (property.GetCustomAttributes(typeof(ExcludeFromInfluxAttribute), false).Any())
                    continue;

                string name = SnakeCase(property.Name);

                object value = property.GetValue(poco);
                string strValue = GetInfluxValueString(value);

                result.Add(name, strValue);
            }

            return result;
        }

        static string GetInfluxValueString(object value)
        {
            switch (value)
            {
                case string s:
                    return $"\"{s}\"";
                case float f:
                    return f.ToString();
                case int i:
                    return $"{i}i";
                case bool b:
                    return b ? "true" : "false";
                default:
                    throw new NotSupportedException($"Type {value.GetType().Name} not supported");
            }
        }

        static string SnakeCase(string pascalCase)
        {
            var regex = new Regex("([a-z])([A-Z])");    //Lower becoming upper
            return regex.Replace(pascalCase, "$1_$2").ToLower();
        }
    }
}
