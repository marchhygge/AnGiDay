namespace AGD.Repositories.Helpers
{
    public static class EnumConverters
    {
        public static string ToDbLabel<TEnum>(TEnum value) where TEnum : Enum
            => value.ToString().ToLowerInvariant();

        public static TEnum FromDbLabel<TEnum>(string dbValue) where TEnum : Enum
        {
            if (string.IsNullOrEmpty(dbValue)) throw new ArgumentException();
            var pascal = char.ToUpperInvariant(dbValue[0]) + dbValue.Substring(1);
            return (TEnum)Enum.Parse(typeof(TEnum), pascal);
        }
    }
}
