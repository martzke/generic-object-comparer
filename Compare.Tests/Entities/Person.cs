using System.Collections.Generic;

namespace Compare.Tests.Entities
{
    public class Person
    {
        public string FirstName { set; get; }
        public string LastName { set; get; }
        public Address HomeAddress { set; get; }
        public Address WorkAddress { set; get; }
        public WorkInfo WorkInfo { set; get; }
        public string Ssn;
        public int Age;
        public Address MailingAddress;

        public List<Address> Addresses { set; get; }

        public Address GetAddress()
        {
            // This is here for testing only.
            return new Address
            {
                Line1 = "123 Main St."
            };
        }
    }
}
