using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ContactManagerProject
{
    public class ContactManager
    {
        public List<Contact> Contacts { get; private set; }

        public ContactManager()
        {
            Contacts = new List<Contact>();
            LoadContacts();
        }

        public void AddContact(Contact contact)
        {
            if (contact == null)
            {
                throw new ArgumentNullException(nameof(contact));
            }

            Contacts.Add(contact);
            SaveContacts();
        }

        public void RemoveContact(Contact contact)
        {
            if (contact == null)
            {
                throw new ArgumentNullException(nameof(contact));
            }

            Contacts.Remove(contact);
            SaveContacts();
        }

        public List<Contact> SearchContacts(string query)
        {
            return Contacts.Where(c => c.Name.Contains(query) || c.PhoneNumber.Contains(query)).ToList();
        }

        private void SaveContacts()
        {
            File.WriteAllLines("contacts.txt", Contacts.Select(c => $"{c.Name}|{c.PhoneNumber}"));
        }

        private void LoadContacts()
        {
            if (File.Exists("contacts.txt"))
            {
                var lines = File.ReadAllLines("contacts.txt");
                foreach (var line in lines)
                {
                    var parts = line.Split('|');
                    if (parts.Length == 2)
                    {
                        Contacts.Add(new Contact(parts[0], parts[1]));
                    }
                }
            }
        }
    }
}
