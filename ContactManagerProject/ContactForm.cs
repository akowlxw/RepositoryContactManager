using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace ContactManagerProject
{
    public partial class ContactForm : Form
    {
        private ContactManager contactManager;
        private TextBox nameTextBox;
        private TextBox phoneNumberTextBox;
        private ComboBox groupComboBox;
        private TextBox newGroupTextBox;
        private Button addGroupButton;
        private Button addContactButton;
        private Button removeContactButton;
        private TextBox searchTextBox;
        private Button searchButton;
        private ComboBox filterGroupComboBox;
        private ListBox contactsListBox;
        private Label deleteGroupLabel;
        private Label addGroupLabel;
        private ComboBox deleteGroupComboBox;
        private Button removeGroupButton;

        private readonly string _namePlaceholder = "Имя";
        private readonly string _phonePlaceholder = "Телефон";
        private readonly string _searchPlaceholder = "Поиск";
        private readonly string _newGroupPlaceholder = "Новая группа";

        public ContactForm()
        {
            this.Text = "Управление контактами";
            this.Width = 700;
            this.Height = 500;

            nameTextBox = new TextBox
            {
                Name = "nameTextBox",
                Location = new Point(10, 10),
                Width = 150,
                TabIndex = 2,
                Text = _namePlaceholder,
                ForeColor = Color.Gray
            };
            nameTextBox.Enter += TextBox_Enter;
            nameTextBox.Leave += TextBox_Leave;

            phoneNumberTextBox = new TextBox
            {
                Name = "phoneNumberTextBox",
                Location = new Point(170, 10),
                Width = 150,
                TabIndex = 3,
                Text = _phonePlaceholder,
                ForeColor = Color.Gray
            };
            phoneNumberTextBox.Enter += TextBox_Enter;
            phoneNumberTextBox.Leave += TextBox_Leave;

            groupComboBox = new ComboBox
            {
                Name = "groupComboBox",
                Location = new Point(330, 10),
                Width = 140,
                TabIndex = 4,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            addContactButton = new Button
            {
                Name = "addContactButton",
                Location = new Point(480, 10),
                Text = "Добавить контакт",
                Width = 150,
                TabIndex = 0
            };
            addContactButton.Click += AddContactButton_Click;

            addGroupLabel = new Label
            {
                Location = new Point(10, 60),
                Text = "Введите название для добавления группы:",
                AutoSize = true
            };

            newGroupTextBox = new TextBox
            {
                Name = "newGroupTextBox",
                Location = new Point(240, 55),
                Width = 200,
                TabIndex = 5,
                Text = _newGroupPlaceholder,
                ForeColor = Color.Gray
            };
            newGroupTextBox.Enter += TextBox_Enter;
            newGroupTextBox.Leave += TextBox_Leave;

            addGroupButton = new Button
            {
                Name = "addGroupButton",
                Location = new Point(450, 55),
                Text = "Добавить группу",
                Width = 130,
                TabIndex = 1
            };
            addGroupButton.Click += AddGroupButton_Click;

            deleteGroupLabel = new Label
            {
                Location = new Point(10, 95),
                Text = "Выберите группу для удаления:",
                AutoSize = true
            };

            deleteGroupComboBox = new ComboBox
            {
                Name = "deleteGroupComboBox",
                Location = new Point(210, 90),
                Width = 200,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            removeGroupButton = new Button
            {
                Name = "removeGroupButton",
                Location = new Point(420, 90),
                Text = "Удалить группу",
                Width = 130
            };
            removeGroupButton.Click += RemoveGroupButton_Click;

            searchTextBox = new TextBox
            {
                Name = "searchTextBox",
                Location = new Point(10, 140),
                Width = 200,
                TabIndex = 7,
                Text = _searchPlaceholder,
                ForeColor = Color.Gray
            };
            searchTextBox.Enter += TextBox_Enter;
            searchTextBox.Leave += TextBox_Leave;

            searchButton = new Button
            {
                Name = "searchButton",
                Location = new Point(220, 140),
                Text = "Искать",
                Width = 80,
                TabIndex = 8
            };
            searchButton.Click += SearchButton_Click;

            filterGroupComboBox = new ComboBox
            {
                Name = "filterGroupComboBox",
                Location = new Point(310, 140),
                Width = 160,
                TabIndex = 9,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            filterGroupComboBox.SelectedIndexChanged += FilterGroupComboBox_SelectedIndexChanged;

            removeContactButton = new Button
            {
                Name = "removeContactButton",
                Location = new Point(480, 140),
                Text = "Удалить контакт",
                Width = 150,
                TabIndex = 6
            };
            removeContactButton.Click += RemoveContactButton_Click;

            contactsListBox = new ListBox
            {
                Name = "contactsListBox",
                Location = new Point(10, 175),
                Width = 650,
                Height = 240,
                TabIndex = 10
            };

            Controls.Add(nameTextBox);
            Controls.Add(phoneNumberTextBox);
            Controls.Add(groupComboBox);
            Controls.Add(addContactButton);
            Controls.Add(newGroupTextBox);
            Controls.Add(addGroupButton);
            Controls.Add(removeContactButton);
            Controls.Add(searchTextBox);
            Controls.Add(searchButton);
            Controls.Add(filterGroupComboBox);
            Controls.Add(contactsListBox);
            Controls.Add(deleteGroupLabel);
            Controls.Add(addGroupLabel);
            Controls.Add(removeGroupButton);
            Controls.Add(deleteGroupComboBox);

            contactManager = new ContactManager();
            UpdateGroups();
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
            if (textBox == newGroupTextBox) return _newGroupPlaceholder;
            return "";
        }

        private void UpdateGroups()
        {
            string selectedGroup = groupComboBox.SelectedItem == null ? ContactManager.DefaultGroup : groupComboBox.SelectedItem.ToString();
            string selectedFilterGroup = filterGroupComboBox.SelectedItem == null ? ContactManager.AllGroups : filterGroupComboBox.SelectedItem.ToString();

            groupComboBox.Items.Clear();
            filterGroupComboBox.Items.Clear();
            filterGroupComboBox.Items.Add(ContactManager.AllGroups);
            deleteGroupComboBox.Items.Clear();

            foreach (string group in contactManager.GetGroups())
            {
                groupComboBox.Items.Add(group);
                filterGroupComboBox.Items.Add(group);
                deleteGroupComboBox.Items.Add(group);
            }

            groupComboBox.SelectedItem = groupComboBox.Items.Contains(selectedGroup) ? selectedGroup : ContactManager.DefaultGroup;
            filterGroupComboBox.SelectedItem = filterGroupComboBox.Items.Contains(selectedFilterGroup) ? selectedFilterGroup : ContactManager.AllGroups;
        }

        private void UpdateContactsList()
        {
            List<Contact> contacts;

            if (filterGroupComboBox.SelectedItem == null || filterGroupComboBox.SelectedItem.ToString() == ContactManager.AllGroups)
            {
                contacts = contactManager.Contacts;
            }
            else
            {
                contacts = contactManager.GetContactsByGroup(filterGroupComboBox.SelectedItem.ToString());
            }

            contactsListBox.Items.Clear();
            foreach (Contact contact in contacts)
            {
                contactsListBox.Items.Add(contact);
            }
        }

        private void AddGroupButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(newGroupTextBox.Text) || newGroupTextBox.Text == _newGroupPlaceholder)
            {
                MessageBox.Show("Введите название группы!");
                return;
            }

            try
            {
                contactManager.AddGroup(newGroupTextBox.Text);
                newGroupTextBox.Text = _newGroupPlaceholder;
                newGroupTextBox.ForeColor = Color.Gray;
                UpdateGroups();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void AddContactButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(nameTextBox.Text) || string.IsNullOrWhiteSpace(phoneNumberTextBox.Text) || nameTextBox.Text == _namePlaceholder || phoneNumberTextBox.Text == _phonePlaceholder)
            {
                MessageBox.Show("Заполните все поля!");
                return;
            }

            string group = groupComboBox.SelectedItem == null ? ContactManager.DefaultGroup : groupComboBox.SelectedItem.ToString();
            try
            {
                contactManager.AddContact(new Contact(nameTextBox.Text, phoneNumberTextBox.Text, group));
                nameTextBox.Text = _namePlaceholder;
                nameTextBox.ForeColor = Color.Gray;
                phoneNumberTextBox.Text = _phonePlaceholder;
                phoneNumberTextBox.ForeColor = Color.Gray;
                groupComboBox.SelectedItem = ContactManager.DefaultGroup;
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

            Contact contact = contactsListBox.SelectedItem as Contact;
            if (contact != null)
            {
                contactManager.RemoveContact(contact);
                UpdateContactsList();
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
            foreach (Contact contact in results)
            {
                if (filterGroupComboBox.SelectedItem == null || filterGroupComboBox.SelectedItem.ToString() == ContactManager.AllGroups || contact.Group == filterGroupComboBox.SelectedItem.ToString())
                {
                    contactsListBox.Items.Add(contact);
                }
            }
        }

        private void FilterGroupComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(searchTextBox.Text) || searchTextBox.Text == _searchPlaceholder)
            {
                UpdateContactsList();
                return;
            }

            SearchButton_Click(sender, e);
        }

        private void RemoveGroupButton_Click(object sender, EventArgs e)
        {
            if (deleteGroupComboBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите группу для удаления!");
                return;
            }

            string groupToRemove = deleteGroupComboBox.SelectedItem.ToString();

            if (groupToRemove == ContactManager.DefaultGroup)
            {
                MessageBox.Show("Нельзя удалить группу по умолчанию!");
                return;
            }
            try
            {
                contactManager.RemoveGroup(groupToRemove);
                UpdateGroups();
                UpdateContactsList();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
