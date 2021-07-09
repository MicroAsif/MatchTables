using System;
using System.Collections.Generic;
using System.Text;

namespace MatchTables.Models
{
    public class Customer
    {
        public string SocialSecurityNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Department { get; set; }

        public bool AreEqual(Customer other)
        {
            var otherType = other as Customer;
            if (otherType == null)
                return false;
            return SocialSecurityNumber == otherType.SocialSecurityNumber && FirstName == otherType.FirstName &&
                   LastName == otherType.LastName && Department == otherType.Department;
        }
    }
}
