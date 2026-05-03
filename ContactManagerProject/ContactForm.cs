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

        private readonly string _namePlaceholder = "Имя";
        private readonly string _phonePlaceholder = "Телефон";
        private readonly string _searchPlaceholder = "Поиск";

        public ContactForm()
        {
            this.Text = "Управление контактами";
            this.Width = 500;
            this.Height = 400;

            nameTextBox = new TextBox { Location = new Point(10, 10), Width = 150, TabIndex = 2 };
            nameTextBox.Text = _namePlaceholder;
            nameTextBox.ForeColor = Color.Gray;
            nameTextBox.Enter += TextBox_Enter;
            nameTextBox.Leave += TextBox_Leave;

            phoneNumberTextBox = new TextBox { Location = new Point(170, 10), Width = 150, TabIndex = 3 };
            phoneNumberTextBox.Text = _phonePlaceholder;
            phoneNumberTextBox.ForeColor = Color.Gray;
            phoneNumberTextBox.Enter += TextBox_Enter;
            phoneNumberTextBox.Leave += TextBox_Leave;

            addContactButton = new Button { Location = new Point(10, 40), Text = "Добавить", Width = 100, TabIndex = 0 };
            removeContactButton = new Button { Location = new Point(120, 40), Text = "Удалить", Width = 100, TabIndex = 1 };

            searchTextBox = new TextBox { Location = new Point(10, 70), Width = 200, TabIndex = 4 };
            searchTextBox.Text = _searchPlaceholder;
            searchTextBox.ForeColor = Color.Gray;
            searchTextBox.Enter += TextBox_Enter;
            searchTextBox.Leave += TextBox_Leave;

            searchButton = new Button { Location = new Point(220, 70), Text = "Искать", Width = 80, TabIndex = 5 };
            contactsListBox = new ListBox { Location = new Point(10, 100), Width = 450, Height = 200, TabIndex = 6 };

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

        private void TextBox_Enter(object sender, EventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            if (textBox.Text == GetPlaceholderForTextBox(textBox))
            {
                textBox.Text = "";
                textBox.ForeColor = Color.Black;
            }
        }

        private void TextBox_Leave(object sender, EventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            if (string.IsNullOrWhiteSpace(textBox.Text))
            {
                textBox.Text = GetPlaceholderForTextBox(textBox);
                textBox.ForeColor = Color.Gray;
            }
        }

        private string GetPlaceholderForTextBox(TextBox textBox)
        {
            if (textBox == nameTextBox) return _namePlaceholder;
            if (textBox == phoneNumberTextBox) return _phonePlaceholder;
            if (textBox == searchTextBox) return _searchPlaceholder;
            return "";
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
            if (string.IsNullOrWhiteSpace(nameTextBox.Text) ||
                string.IsNullOrWhiteSpace(phoneNumberTextBox.Text) ||
                nameTextBox.Text == _namePlaceholder ||
                phoneNumberTextBox.Text == _phonePlaceholder)
            {
                MessageBox.Show("Заполните все поля!");
                return;
            }

            try
            {
                contactManager.AddContact(new Contact(nameTextBox.Text, phoneNumberTextBox.Text));
                nameTextBox.Text = _namePlaceholder;
                nameTextBox.ForeColor = Color.Gray;
                phoneNumberTextBox.Text = _phonePlaceholder;
                phoneNumberTextBox.ForeColor = Color.Gray;
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
            if (string.IsNullOrWhiteSpace(searchTextBox.Text) || searchTextBox.Text == _searchPlaceholder)
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
