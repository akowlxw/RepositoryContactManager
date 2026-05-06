namespace ContactManagerProject
{
    public class Contact
    {
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public string Group { get; set; }

        public Contact(string name, string phoneNumber, string group = "Без группы")
        {
            Name = name;
            PhoneNumber = phoneNumber;
            Group = string.IsNullOrWhiteSpace(group) ? "Без группы" : group;
        }

        public override string ToString()
        {
            return $"{Name} - {PhoneNumber} ({Group})";
        }
    }
}
