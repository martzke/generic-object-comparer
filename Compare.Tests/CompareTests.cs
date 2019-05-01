using System;
using Compare.Tests.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;

namespace Compare.Tests
{
    [TestClass]
    public class CompareTests
    {
        [TestMethod]
        public void IgnorePropertyByExpression()
        {
            var p1 = GetDefaultPerson();
            var p2 = GetDefaultPerson();

            p1.HomeAddress.City = "ldsfkjdslkj";
            p2.HomeAddress.City = "Grand Rapids";

            var target = new EqualityChecker<Person>()
                .Ignore(p => p.HomeAddress.City);
            Assert.IsTrue(target.HaveEqualValues(p1, p2));
        }

        [TestMethod]
        public void OverridePropertyByExpression()
        {
            var p1 = GetDefaultPerson();
            var p2 = GetDefaultPerson();

            p1.HomeAddress.City = "GRAND RAPIDS";
            p2.HomeAddress.City = "Grand Rapids";

            var target = new EqualityChecker<Person>()
                .Override(p => p.HomeAddress.City, (string s1, string s2) => s1.Equals(s2, StringComparison.InvariantCultureIgnoreCase))
                .Override(p => p.FirstName, (string s1, string s2) => true);
            Assert.IsTrue(target.HaveEqualValues(p1, p2));
        }

        [TestMethod]
        public void AddressDifferentCaseNotEqual()
        {
            var addr1 = GetDefaultAddress();
            var addr2 = GetDefaultAddress();
            addr1.City = addr1.City.ToLower();
            addr2.City = addr2.City.ToUpper();
            var target = new EqualityChecker<Address>();
            Assert.IsFalse(target.HaveEqualValues(addr1, addr2));
        }

        [TestMethod]
        public void AddressDifferentCaseOverrideString()
        {
            var addr1 = GetDefaultAddress();
            var addr2 = GetDefaultAddress();
            addr1.City = addr1.City.ToLower();
            addr2.City = addr2.City.ToUpper();
            var target = new EqualityChecker<Address>()
                .Override((string s1, string s2) => s1.Equals(s2, StringComparison.InvariantCultureIgnoreCase));
            Assert.IsTrue(target.HaveEqualValues(addr1, addr2));
        }

        [TestMethod]
        public void IgnorePropertyTypes()
        {
            var addr1 = GetDefaultAddress();
            var addr2 = GetDefaultAddress();

            addr1.State = State.IN;
            addr2.State = State.MI;

            var target = new EqualityChecker<Address>()
                .Ignore(typeof(State));

            var result = target.HaveEqualValues(addr1, addr2);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CompareObjectsWithStringsAndEnums()
        {
            var person1 = GetDefaultPerson();
            var person2 = GetDefaultPerson();
            var equalityChecker = new EqualityChecker<Person>();
            var equal = equalityChecker.HaveEqualValues(person1, person2);
            Assert.IsTrue(equal);
        }

        [TestMethod]
        public void CompareObjectsOnlyEnumDifferent()
        {
            var person1 = GetDefaultPerson();
            var person2 = GetDefaultPerson();
            person1.HomeAddress.State = State.IN;
            person2.HomeAddress.State = State.MI;
            var equalityChecker = new EqualityChecker<Person>();
            var equal = equalityChecker.HaveEqualValues(person1, person2);
            Assert.IsFalse(equal);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CircularReferenceLongChainBreaks()
        {
            var addr1 = GetDefaultAddress();
            var addr2 = GetDefaultAddress();
            var addr3 = GetDefaultAddress();
            addr1.AddressRef = addr2;
            addr2.AddressRef = addr3;
            addr3.AddressRef = addr1;

            var addr4 = GetDefaultAddress();
            var addr5 = GetDefaultAddress();
            var addr6 = GetDefaultAddress();
            addr4.AddressRef = addr5;
            addr5.AddressRef = addr6;
            addr6.AddressRef = addr4;

            // These object graphs have circular references.
            var target = new EqualityChecker<Address>();
            target.HaveEqualValues(addr1, addr4);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CircularReferenceBreaks()
        {
            var obj1 = new SelfReferencingObject();
            var obj2 = new SelfReferencingObject();
            var target = new EqualityChecker<SelfReferencingObject>();
            target.HaveEqualValues(obj1, obj2);
        }

        [TestMethod]
        public void CheckCircularReferenceNoExceptionThrown()
        {
            var obj1 = GetDefaultPerson();
            var obj2 = GetDefaultPerson();
            var addrInfo1 = new AddressInfo
            {
                PropertyOwner = new PropertyOwner { Name = "Joe Smith" },
                PropertyValue = 50000
            };

            obj1.HomeAddress.AddressInfo = addrInfo1;
            obj2.HomeAddress.AddressInfo = addrInfo1;

    //        Person
    //           |
    //HomeAddress   MailingAddress
    //     |              |
    //     |              |
    //            |
    //        AddressInfo
    //            |
    //        PropertyOwner


            // There is no circular reference, so no exception should be thrown.
            var target = new EqualityChecker<Person>();
            Assert.IsTrue(target.HaveEqualValues(obj1, obj2));

                           //   [ ]
                           //    |
                           // _______
                           // |     |
                           //[ ]   [ ]
                           // |     |
                           // ______
                           //    |
                           //   [ ]  <-- This test tests this scenario to make sure it is not treated as a circular reference.
                           //    |
                           //   [ ]
    }

        [TestMethod]
        public void ReferenceSharedByBothObjectsNoExceptionThrown()
        {
            var obj1 = GetDefaultPerson();
            var obj2 = GetDefaultPerson();
            var address = GetDefaultAddress();
            obj1.HomeAddress = address;
            obj2.WorkAddress = address;
            var target = new EqualityChecker<Person>();
            target.HaveEqualValues(obj1, obj2);
        }

        [TestMethod]
        public void SharedLeafNodeNoExceptionThrown()
        {
            var obj1 = GetDefaultPerson();
            var obj2 = GetDefaultPerson();
            var address = GetDefaultAddress();
            obj1.WorkAddress = address;
            obj1.HomeAddress = address;
            var target = new EqualityChecker<Person>();
            target.HaveEqualValues(obj1, obj2);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ValidateExpressionFail()
        {
            var target = new EqualityChecker<Person>();
            var person = new Person();

            // This expression does not use the parameter of the lambda expression, p.
            target.Ignore(p => person.HomeAddress.City);
        }

        [TestMethod]
        public void ValidateExpressionSuccess()
        {
            var target = new EqualityChecker<Person>();

            // This expression is OK.
            target.Ignore(p => p.HomeAddress.City);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ValidateExpressionWithMethodCall()
        {
            var target = new EqualityChecker<Person>();

            // Method calls are not allowed in the expression.
            target.Ignore(p => p.GetAddress().Line1);
        }

        [TestMethod]
        public void CompareModelWithList()
        {
            var p1 = GetDefaultPerson();
            var p2 = GetDefaultPerson();

            p1.Addresses = new List<Address>
            {
                GetDefaultAddress(),
                GetDefaultAddress(),
                GetDefaultAddress(),
            };

            p2.Addresses = new List<Address>
            {
                GetDefaultAddress(),
                GetDefaultAddress(),
                GetDefaultAddress(),
            };

            var target = new EqualityChecker<Person>();
            Assert.IsTrue(target.HaveEqualValues(p1, p2));
        }

        private Address GetDefaultAddress()
        {
            return new Address
            {
                Line1 = "82 Marshall Dr.",
                Line2 = "asdf",
                City = "Etna",
                State = State.OH,
                ZipCode = "43068"
            };
        }

        private Person GetDefaultPerson()
        {
            return new Person
            {
                FirstName = "Joe",
                LastName = "Schmo",
                HomeAddress = GetDefaultAddress(),
                WorkAddress = GetDefaultAddress(),
                MailingAddress = GetDefaultAddress(),
                Ssn = "429582716",
                Age = 30,
                Addresses = new List<Address>()
            };
        }
    }
}
