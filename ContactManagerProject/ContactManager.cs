using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ContactManagerProject
{
    public class ContactManager
    {
        public const string DefaultGroup = "Без группы";
        public const string AllGroups = "Все группы";

        public List<Contact> Contacts { get; private set; }
        public List<string> Groups { get; private set; }

        public ContactManager()
        {
            Contacts = new List<Contact>();
            Groups = new List<string>();
            Groups.Add(DefaultGroup);
            LoadContacts();
        }

        public void AddContact(Contact contact)
        {
            if (contact == null)
            {
                throw new ArgumentNullException(nameof(contact));
            }

            if (string.IsNullOrWhiteSpace(contact.Group))
            {
                contact.Group = DefaultGroup;
            }

            if (!Groups.Contains(contact.Group))
            {
                throw new ArgumentException("Сначала добавьте группу.");
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

        public void AddGroup(string group)
        {
            if (string.IsNullOrWhiteSpace(group))
            {
                throw new ArgumentException("Введите название группы.");
            }

            group = group.Trim();
            if (Groups.Contains(group))
            {
                throw new ArgumentException("Такая группа уже существует.");
            }

            Groups.Add(group);
            SaveContacts();
        }

        public void RemoveGroup(string group)
        {
            if (string.IsNullOrWhiteSpace(group))
            {
                throw new ArgumentException("Группа не выбрана.");
            }

            if (!Groups.Contains(group))
            {
                throw new ArgumentException("Группа не найдена.");
            }

            if (group == DefaultGroup)
            {
                throw new ArgumentException("Нельзя удалить группу по умолчанию.");
            }
            Groups.Remove(group);

            foreach (var contact in Contacts)
            {
                if (contact.Group == group)
                {
                    contact.Group = DefaultGroup;
                }
            }
            SaveContacts();
        }
        public List<Contact> SearchContacts(string query)
        {
            return Contacts.Where(c => c.Name.Contains(query) || c.PhoneNumber.Contains(query) || c.Group.Contains(query)).ToList();
        }

        public List<Contact> GetContactsByGroup(string group)
        {
            List<Contact> result = new List<Contact>();

            if (string.IsNullOrWhiteSpace(group) || group == AllGroups)
            {
                foreach (Contact contact in Contacts)
                {
                    result.Add(contact);
                }
                return result;
            }

            foreach (Contact contact in Contacts)
            {
                if (contact.Group == group)
                {
                    result.Add(contact);
                }
            }

            return result;
        }

        public List<string> GetGroups()
        {
            List<string> result = new List<string>();
            foreach (string group in Groups)
            {
                result.Add(group);
            }
            return result;
        }

        private void SaveContacts()
        {
            List<string> lines = new List<string>();

            foreach (string group in Groups)
            {
                lines.Add("GROUP|" + group);
            }

            foreach (Contact contact in Contacts)
            {
                lines.Add("CONTACT|" + contact.Name + "|" + contact.PhoneNumber + "|" + contact.Group);
            }

            File.WriteAllLines("contacts.txt", lines);
        }

        private void LoadContacts()
        {
            if (!File.Exists("contacts.txt"))
            {
                return;
            }

            string[] lines = File.ReadAllLines("contacts.txt");
            foreach (string line in lines)
            {
                string[] parts = line.Split('|');

                if (parts.Length == 2 && parts[0] == "GROUP")
                {
                    if (!Groups.Contains(parts[1]))
                    {
                        Groups.Add(parts[1]);
                    }
                }
                else if (parts.Length >= 4 && parts[0] == "CONTACT")
                {
                    string group = string.IsNullOrWhiteSpace(parts[3]) ? DefaultGroup : parts[3];
                    if (!Groups.Contains(group))
                    {
                        Groups.Add(group);
                    }

                    Contacts.Add(new Contact(parts[1], parts[2], group));
                }
                else if (parts.Length == 2)
                {
                    Contacts.Add(new Contact(parts[0], parts[1], DefaultGroup));
                }
                else if (parts.Length >= 3)
                {
                    string group = string.IsNullOrWhiteSpace(parts[2]) ? DefaultGroup : parts[2];
                    if (!Groups.Contains(group))
                    {
                        Groups.Add(group);
                    }
                    Contacts.Add(new Contact(parts[0], parts[1], group));
                }
            }
        }
    }
}
