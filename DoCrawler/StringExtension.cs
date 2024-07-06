using System.Linq;

namespace DoCrawler;

public static class StringExtension
{
    public static string TrimStartEnd(this string? strFrom, params char[] trimChars)
    {
        if (strFrom == null)
            return string.Empty;
        var strToRet = strFrom;
        var atLeastOneTrimmed = true;
        while (atLeastOneTrimmed)
        {
            atLeastOneTrimmed = false;
            while (strToRet.Length > 0 && trimChars.Contains(strToRet.First()))
            {
                strToRet = strToRet[1..];
                atLeastOneTrimmed = true;
            }

            while (strToRet.Length > 0 && trimChars.Contains(strToRet.Last()))
            {
                strToRet = strToRet[..^1];
                atLeastOneTrimmed = true;
            }
        }

        //while (strToRet.Length > 1 && strToRet.First() == strToRet.Last() && trimChars.Contains(strToRet.First()))
        //{
        //  if (strToRet.Length == 2)
        //    strToRet = string.Empty;
        //  else
        //    strToRet = strToRet.Substring(1, strToRet.Length - 2);
        //}
        return strToRet;
    }

    public static int GetDeterministicHashCode(this string str)
    {
        unchecked
        {
            var hash1 = (5381 << 16) + 5381;
            var hash2 = hash1;

            for (var i = 0; i < str.Length; i += 2)
            {
                hash1 = ((hash1 << 5) + hash1) ^ str[i];
                if (i == str.Length - 1)
                    break;
                hash2 = ((hash2 << 5) + hash2) ^ str[i + 1];
            }

            return hash1 + hash2 * 1566083941;
        }
    }

    public static string? Truncate(this string? value, int maxLength, string? truncationSuffix = "…")
    {
        var onlyValueLength = maxLength - truncationSuffix?.Length ?? 0;
        return value?.Length > onlyValueLength ? $"{value[..onlyValueLength]}{truncationSuffix}" : value;
    }
}