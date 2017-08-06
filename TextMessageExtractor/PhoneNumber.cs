using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextMessageExtractor
{
    class PhoneNumber
    {
        private String fullNumber;

        public String FormattedNumber
        {
            get
            {
                return $"{fullNumber.Substring(2, 3)}-{fullNumber.Substring(5, 3)}-{fullNumber.Substring(8, 3)}";
            }
        }

        public PhoneNumber(String number)
        {
            StringBuilder sb = new StringBuilder();

            int i = 0;
            if(number[0] != '+')
            {
                sb.Append("+1");
                i = 1;
            }

            while(i < number.Length)
            {
                if (Char.IsDigit(number[i]))
                    sb.Append(number[i]);
                i++;
            }

            fullNumber = sb.ToString();
            if(fullNumber.Length != 12)
            {
                throw new ArgumentException();
            }
        }

        public override string ToString()
        {
            return FormattedNumber;
        }
    }
}
