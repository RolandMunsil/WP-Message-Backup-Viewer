using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextMessageExtractor
{
    class ContactDatabase
    {
        private Dictionary<String, String> numberToNameMap;

        public ContactDatabase(Dictionary<String, String> numberToNameMap)
        {
            this.numberToNameMap = numberToNameMap;
        }

        //Returns the name of the contact with phoneNumber, otherwise returns the phone number
        public String TryGetContactName(String phoneNumber)
        {
            if(numberToNameMap.TryGetValue(phoneNumber, out String name))
            {
                return name;
            }
            return phoneNumber;
        }
    }
}
