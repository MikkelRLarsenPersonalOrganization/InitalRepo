using System;
using System.Collections.Generic;
using System.Text;

namespace CiCd.Domain
{
    public class Person
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public int PhoneNumber { get; private set; }
        public DateTime Date { get; private set; }

        public Person(string name, int phoneNumber, DateTime date)
        {
            Id = Guid.NewGuid();
            Name = name;
            PhoneNumber = phoneNumber;
            Date = date;
        }

    }
}
