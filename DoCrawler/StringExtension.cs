using System.Linq;

namespace DoCrawler;

public static class StringExtension
{
    public static string TrimStartEnd(this string strFrom, params char[] trimChars)
    {
        if (strFrom == null) return "";
        var strToRet = strFrom;
        var atLeastOneTrimmed = true;
        while (atLeastOneTrimmed)
        {
            atLeastOneTrimmed = false;
            while (strToRet.Length > 0 && trimChars.Contains(strToRet.First()))
            {
                strToRet = strToRet.Substring(1, strToRet.Length - 1);
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
        //    strToRet = "";
        //  else
        //    strToRet = strToRet.Substring(1, strToRet.Length - 2);
        //}
        return strToRet;
    }
}