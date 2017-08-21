using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextMessageExtractor
{
    /// <summary>
    /// A normalizer is just a method that converts phone numbers into a comparable format.
    /// E.g. the UnitedStates method should convert "(123) 456 7890", "+11234567890", 
    /// and "123-456-7890" into the same string
    /// 
    /// The reason I can't just write one normalizer method is that there doesn't appear to be 
    /// any way of taking a number without a country code and figuring out the country code
    /// (without knowing where the user is living, obviously)
    /// 
    /// So this system is designed such that you use the normalizer for the country you live in
    /// (or were living in when you had your phone).
    /// </summary>
    public static class PhoneNumberNormalizers
    {
        public static Func<String, String> DontNormalize = (s) => s;

        /// <summary>
        /// NOTE: This function is just my best guess at how to normalize US numbers.
        /// I haven't checked to make sure it works for all edge cases, so until I 
        /// check it I recommend looking through your numbers and making sure they got
        /// normalized properly
        /// </summary>
        public static Func<String, String> UnitedStates = delegate (String str)
        {
            //Remove everything except digits
            StringBuilder sb = new StringBuilder();
            for(int i = 0; i < str.Length; i++)
            {
                if (Char.IsDigit(str[i]))
                    sb.Append(str[i]);
            }
            String onlyNumbers = sb.ToString();

            //If the number has more than 10 digits, assume it either has a +1
            //Or is a non-us number
            if (onlyNumbers.Length > 10)
            {
                int lengthOfExt = onlyNumbers.Length - 10;
                String ext = onlyNumbers.Substring(0, lengthOfExt);
                //If the country extension is a 1, remove it and return
                if (ext == "1")
                {
                    String last10 = onlyNumbers.Substring(ext.Length, 10);
                    return last10;
                }
                else //Otherwise keep the country code
                {
                    return onlyNumbers;
                }
            }
            return onlyNumbers;
        };
    }
}
