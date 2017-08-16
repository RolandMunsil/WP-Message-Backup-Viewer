using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextMessageExtractor
{
    public class ContactDatabase
    {
        private Dictionary<String, String> numberToNameMap;
        private HashSet<String> namesWithMultipleNumbers;

        public ContactDatabase()
        {
            numberToNameMap = new Dictionary<string, string>();
            namesWithMultipleNumbers = new HashSet<string>();
        }

        public void Add(String contactPhoneNum, String contactName)
        {
            if (numberToNameMap.TryGetValue(contactPhoneNum, out string nameInDB))
            {
                if (nameInDB != contactName)
                {
                    //There are two different names for the same number, so separate them with a slash
                    numberToNameMap[contactPhoneNum] += $"/{contactName}";
                }
                //Otherwise just don't add it since it's already in the database
            }
            else
            {
                //If there is already a number with this name, mark it
                if (numberToNameMap.Values.Contains(contactName))
                {
                    namesWithMultipleNumbers.Add(contactName);
                }

                //No conflicts, so add it
                numberToNameMap[contactPhoneNum] = contactName;
            }
        }

        //Returns the name of the contact with phoneNumber, otherwise returns the phone number
        public String TryGetContactName(String phoneNumber)
        {
            if(numberToNameMap.TryGetValue(phoneNumber, out String name))
            {
                if (namesWithMultipleNumbers.Contains(name))
                {
                    return $"{name} ({phoneNumber})";
                }
                else
                {
                    return name;
                }
            }
            return phoneNumber;
        }
    }
}
