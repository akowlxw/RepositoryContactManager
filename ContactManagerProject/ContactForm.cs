using System;
using System.Drawing;
using System.Windows.Forms;

namespace ContactManagerProject
{
    public partial class ContactForm : Form
    {
        private ContactManager contactManager;
        private TextBox nameTextBox;
        private TextBox phoneNumberTextBox;
        private Button addContactButton;
        private Button removeContactButton;
        private TextBox searchTextBox;
        private Button searchButton;
        private ListBox contactsListBox;

        public ContactForm()
        {
            this.Text = "Управление контактами";
            this.Width = 500;
            this.Height = 400;

            nameTextBox = new TextBox { Location = new Point(10, 10), Width = 150 };
            phoneNumberTextBox = new TextBox { Location = new Point(170, 10), Width = 150 };
            addContactButton = new Button { Location = new Point(10, 40), Text = "Добавить", Width = 100 };
            removeContactButton = new Button { Location = new Point(120, 40), Text = "Удалить", Width = 100 };
            searchTextBox = new TextBox { Location = new Point(10, 70), Width = 200 };
            searchButton = new Button { Location = new Point(220, 70), Text = "Искать", Width = 80 };
            contactsListBox = new ListBox { Location = new Point(10, 100), Width = 450, Height = 200 };

            addContactButton.Click += AddContactButton_Click;
            removeContactButton.Click += RemoveContactButton_Click;
            searchButton.Click += SearchButton_Click;

            Controls.Add(nameTextBox);
            Controls.Add(phoneNumberTextBox);
            Controls.Add(addContactButton);
            Controls.Add(removeContactButton);
            Controls.Add(searchTextBox);
            Controls.Add(searchButton);
            Controls.Add(contactsListBox);

            contactManager = new ContactManager();
            UpdateContactsList();
        }

        private void UpdateContactsList()
        {
            contactsListBox.Items.Clear();
            foreach (var contact in contactManager.Contacts)
            {
                contactsListBox.Items.Add($"{contact.Name} - {contact.PhoneNumber}");
            }
        }

        private void AddContactButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(nameTextBox.Text) || string.IsNullOrEmpty(phoneNumberTextBox.Text))
            {
                MessageBox.Show("Заполните все поля!");
                return;
            }

            try
            {
                contactManager.AddContact(new Contact(nameTextBox.Text, phoneNumberTextBox.Text));
                nameTextBox.Clear();
                phoneNumberTextBox.Clear();
                UpdateContactsList();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void RemoveContactButton_Click(object sender, EventArgs e)
        {
            if (contactsListBox.SelectedIndex == -1)
            {
                MessageBox.Show("Выберите контакт для удаления!");
                return;
            }

            string selectedItem = contactsListBox.SelectedItem.ToString();
            string[] parts = selectedItem.Split(new[] { '-' }, StringSplitOptions.None);
            if (parts.Length >= 2)
            {
                string name = parts[0].Trim();
                string phone = parts[1].Trim();
                var contact = contactManager.Contacts.Find(c => c.Name == name && c.PhoneNumber == phone);
                if (contact != null)
                {
                    contactManager.RemoveContact(contact);
                    UpdateContactsList();
                }
            }
        }

        private void SearchButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(searchTextBox.Text))
            {
                UpdateContactsList();
                return;
            }

            var results = contactManager.SearchContacts(searchTextBox.Text);
            contactsListBox.Items.Clear();
            foreach (var contact in results)
            {
                contactsListBox.Items.Add($"{contact.Name} - {contact.PhoneNumber}");
            }
        }
    }
}
