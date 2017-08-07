using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextMessageExtractor
{
    public class PhoneNumber : IComparable<PhoneNumber>, IEquatable<PhoneNumber>
    {
        private String extension;
        private String countryNumber;

        public String FormattedNumber
        {
            get
            {
                String formattedCountry = $"{countryNumber.Substring(0, 3)}-{countryNumber.Substring(3, 3)}-{countryNumber.Substring(6, 4)}";
                if (extension != "1")
                {
                    return $"+{extension} {formattedCountry}";
                }
                else
                {
                    return formattedCountry;
                }
            }
        }

        private String JustNumbers
        {
            get
            {
                return extension + countryNumber;
            }
        }

        public PhoneNumber(String number)
        {
            StringBuilder sb = new StringBuilder();

            //int i = 0;
            //if(number[0] == '+')
            //{
            //    sb.Append("+");
            //    i = 1;
            //}
            //else
            //{
            //    sb.Append("+1");
            //}

            for(int i = 0; i < number.Length; i++)
            {
                if (Char.IsDigit(number[i]))
                    sb.Append(number[i]);
            }

            String numbersOnly = sb.ToString();
            if(numbersOnly.Length > 10)
            {
                extension = numbersOnly.Substring(0, numbersOnly.Length - 10);
                countryNumber = numbersOnly.Substring(extension.Length, 10);
            }
            else
            {
                extension = "1";
                countryNumber = numbersOnly;
            }
            if(countryNumber.Length != 10)
            {
                Console.WriteLine(number);
                //System.Diagnostics.Debugger.Break();
            }
        }

        public override string ToString()
        {
            return FormattedNumber;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is PhoneNumber))
                return false;

            return this.JustNumbers.Equals(((PhoneNumber)obj).JustNumbers);
        }

        public override int GetHashCode()
        {
            return this.JustNumbers.GetHashCode();
        }

        public int CompareTo(PhoneNumber other)
        {
            return this.JustNumbers.CompareTo(other.JustNumbers);
        }

        public bool Equals(PhoneNumber other)
        {
            return this.JustNumbers.Equals(other.JustNumbers);
        }
    }
}
