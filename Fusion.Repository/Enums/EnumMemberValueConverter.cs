
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Reflection;
using System.Runtime.Serialization;

namespace Fusion.Repository.Enums
{
    public class EnumMemberValueConverter<TEnum> : ValueConverter<TEnum, string>
        where TEnum : struct, Enum
    {
        public EnumMemberValueConverter() : base(
            v => GetEnumMemberValue(v),
            v => GetEnumFromValue(v))
        { }

        private static TEnum GetEnumFromValue(string value)
        {
            foreach (var field in typeof(TEnum).GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                var attr = field.GetCustomAttribute<EnumMemberAttribute>();
                if (attr?.Value == value)
                    return (TEnum)field.GetValue(null)!;
            }

            // fallback: parse theo tên (case-insensitive)
            return Enum.Parse<TEnum>(value, true);
        }

        private static string GetEnumMemberValue(TEnum value)
        {
            var member = typeof(TEnum).GetMember(value.ToString()).FirstOrDefault();
            var attr = member?.GetCustomAttribute<EnumMemberAttribute>();
            return attr?.Value ?? value.ToString();
        }
    }
}