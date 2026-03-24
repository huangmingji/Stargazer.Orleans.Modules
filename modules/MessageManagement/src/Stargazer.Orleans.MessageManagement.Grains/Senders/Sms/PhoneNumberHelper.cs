namespace Stargazer.Orleans.MessageManagement.Grains.Senders.Sms;

public static class PhoneNumberHelper
{
    public static string FormatForChina(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            return phoneNumber;
        }

        phoneNumber = phoneNumber.Trim();

        if (phoneNumber.StartsWith("+86"))
        {
            return phoneNumber;
        }

        if (phoneNumber.StartsWith("86"))
        {
            return $"+{phoneNumber}";
        }

        return $"+86{phoneNumber}";
    }

    public static bool IsValidChinaPhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            return false;
        }

        var normalized = FormatForChina(phoneNumber);

        if (normalized.StartsWith("+86"))
        {
            var digits = normalized[3..];
            return digits.Length == 11 && digits.All(char.IsDigit);
        }

        return false;
    }
}
