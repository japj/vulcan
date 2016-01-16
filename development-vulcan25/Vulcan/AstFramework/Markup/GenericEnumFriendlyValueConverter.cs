namespace AstFramework.Markup
{
    public class EnumFriendlyValueConverter<TEnum> : EnumFriendlyValueConverter
    {
        public EnumFriendlyValueConverter() : base(typeof(TEnum))
        {
        }
    }
}