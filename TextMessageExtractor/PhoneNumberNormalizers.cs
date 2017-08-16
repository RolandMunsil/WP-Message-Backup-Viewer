using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextMessageExtractor
{
    static class PhoneNumberNormalizers
    {
        public static Func<String, String> DontNormalize = (s) => s;

        public static Func<String, String> UnitedStates = delegate (String str)
        {
            String onlyNumbers = new String(str.Where(c => Char.IsDigit(c)).ToArray());
            if (onlyNumbers.Length > 10)
            {
                int lengthOfExt = onlyNumbers.Length - 10;
                String ext = onlyNumbers.Substring(0, lengthOfExt);
                if (ext == "1")
                {
                    String last10 = onlyNumbers.Substring(ext.Length, 10);
                    return last10;
                }
                else
                {
                    return onlyNumbers;
                }
            }
            return onlyNumbers;
        };
    }
}
