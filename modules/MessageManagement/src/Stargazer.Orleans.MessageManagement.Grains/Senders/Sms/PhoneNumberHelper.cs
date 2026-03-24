namespace Stargazer.Orleans.MessageManagement.Grains.Senders.Sms;

public static class PhoneNumberHelper
{
    public static string FormatForChina(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            return string.Empty;
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

        if (phoneNumber.StartsWith("+"))
        {
            return phoneNumber;
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

        if (!normalized.StartsWith("+86"))
        {
            return false;
        }

        var digits = normalized[3..];
        return digits.Length >= 7 && digits.Length <= 15 && digits.All(char.IsDigit);
    }
}
