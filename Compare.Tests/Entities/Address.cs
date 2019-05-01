namespace Compare.Tests.Entities
{
    public class Address
    {
        public Address()
        {
        }

        public string Line1 { set; get; }
        public string Line2 { set; get; }
        public string City { set; get; }
        public State State { set; get; }
        public string ZipCode { set; get; }

        public AddressInfo AddressInfo { set; get; }

        // This is just here to test circular references.
        public Address AddressRef { set; get; }
    }
}
