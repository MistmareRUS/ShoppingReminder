using System.Globalization;

namespace ShoppingReminder.Languages
{
    public interface ILocalize
    {
        CultureInfo GetCurrentCultureInfo();
    }
}
