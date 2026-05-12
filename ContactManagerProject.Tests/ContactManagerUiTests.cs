using FlaUI.Core; 
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.Core.Input;
using FlaUI.Core.WindowsAPI;
using FlaUI.UIA3;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace ContactManagerProject.Tests
{
    [TestClass]
    public class ContactManagerUiTests
    {
        private const string AppPath = @"C:\Users\admin\Desktop\ContactTask_lab1_variant7_fresh\ContactManagerProject\bin\Debug\ContactManagerProject.exe";

        private Application _app;
        private UIA3Automation _automation;
        private Window _mainWindow;

        private string _testDirectory;
        private string _originalDirectory;

        private string ContactsFilePath => Path.Combine(_testDirectory, "contacts.txt");

        [TestInitialize]
        public void TestInitialize()
        {
            _originalDirectory = Environment.CurrentDirectory;

            _testDirectory = Path.Combine(Path.GetTempPath(), "ContactManagerUiTests_" + Guid.NewGuid());
            Directory.CreateDirectory(_testDirectory);

            Environment.CurrentDirectory = _testDirectory;

            _automation = new UIA3Automation();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _automation?.Dispose();
            _app?.Close();

            Environment.CurrentDirectory = _originalDirectory;

            try
            {
                Directory.Delete(_testDirectory, true);
            }
            catch
            {
            }
        }

        // ТК-1. Добавление контакта с корректными данными добавляет контакт в список и сохраняет его в файл.
        [TestMethod]
        public void TC01_AddContact_WithCorrectData_AddsContactToListAndFile()
        {
            LaunchApplication();

            AddContactViaUi("Иван Петров", "8 981 765 43 21");

            AssertListContains("Иван Петров - 8 981 765 43 21 (Без группы)");
            AssertFileContains("CONTACT|Иван Петров|8 981 765 43 21|Без группы");
        }

        // ТК-2. Добавление контакта с пустыми значениями показывает сообщение об ошибке и не добавляет контакт.
        [TestMethod]
        public void TC02_AddContact_WithEmptyValues_ShowsErrorAndDoesNotAddContact()
        {
            LaunchApplication();

            ClickButton("addContactButton");
            string message = ReadAndCloseMessageBox();

            StringAssert.Contains(message, "Заполните все поля");
            Assert.AreEqual(0, GetContactListItems().Count, "Контакт не должен добавляться при пустых полях.");
        }

        // ТК-3. Попытка добавления контакта с пустым именем показывает сообщение об ошибке.
        [TestMethod]
        public void TC03_AddContact_WithEmptyName_ShowsErrorAndDoesNotAddContact()
        {
            LaunchApplication();

            SetTextBoxText("phoneNumberTextBox", "8 981 765 43 21");
            ClickButton("addContactButton");
            string message = ReadAndCloseMessageBox();

            StringAssert.Contains(message, "Заполните все поля");
            Assert.AreEqual(0, GetContactListItems().Count, "Контакт не должен добавляться без имени.");
        }

        // ТК-4. Попытка добавления контакта с пустым телефоном показывает сообщение об ошибке.
        [TestMethod]
        public void TC04_AddContact_WithEmptyPhone_ShowsErrorAndDoesNotAddContact()
        {
            LaunchApplication();

            SetTextBoxText("nameTextBox", "Иван Петров");
            ClickButton("addContactButton");
            string message = ReadAndCloseMessageBox();

            StringAssert.Contains(message, "Заполните все поля");
            Assert.AreEqual(0, GetContactListItems().Count, "Контакт не должен добавляться без телефона.");
        }

        // ТК-5. Удаление существующего контакта удаляет контакт из списка и из файла.
        [TestMethod]
        public void TC05_DeleteExistingContact_RemovesContactFromListAndFile()
        {
            LaunchApplication();

            AddContactViaUi("Анна", "+7 911 111 22 33");

            SelectContactInList("Анна - +7 911 111 22 33 (Без группы)");
            ClickButton("removeContactButton");

            AssertListDoesNotContain("Анна - +7 911 111 22 33 (Без группы)");
            AssertFileDoesNotContain("CONTACT|Анна|+7 911 111 22 33|Без группы");
        }

        // ТК-6. Попытка удаления без выбора контакта показывает сообщение об ошибке.
        [TestMethod]
        public void TC06_DeleteContact_WithoutSelection_ShowsError()
        {
            LaunchApplication();

            AddContactViaUi("Анна", "+7 911 111 22 33");

            ClickButton("removeContactButton");
            string message = ReadAndCloseMessageBox();

            StringAssert.Contains(message, "Выберите контакт для удаления");
            AssertListContains("Анна - +7 911 111 22 33 (Без группы)");
        }

        // ТК-7. Поиск контакта по имени отображает только подходящие контакты.
        [TestMethod]
        public void TC07_SearchContact_ByName_ShowsOnlyMatchingContacts()
        {
            LaunchApplication();

            AddContactViaUi("анна", "+7 999 111-22-33");
            AddContactViaUi("Иван", "+7 888 333-44-55");
            AddContactViaUi("Петр", "+7 999 777-88-99");

            SearchContacts("ан");

            AssertListContains("анна - +7 999 111-22-33 (Без группы)");
            AssertListContains("Иван - +7 888 333-44-55 (Без группы)");
            AssertListDoesNotContain("Петр - +7 888 333-44-55 (Без группы)");
        }

        // ТК-8. Поиск контакта по номеру телефона отображает контакт с совпадающим номером.
        [TestMethod]
        public void TC08_SearchContact_ByPhone_ShowsOnlyMatchingContacts()
        {
            LaunchApplication();

            AddContactViaUi("Ольга", "+7-999-111-22-33");
            AddContactViaUi("Павел", "+7-777-000-11-22");

            SearchContacts("999");

            AssertListContains("Ольга - +7-999-111-22-33 (Без группы)");
            AssertListDoesNotContain("Павел - +7-777-000-11-22 (Без группы)");
        }

        // ТК-9. Поиск контакта по общему совпадению в имени и телефоне находит нужный контакт.
        [TestMethod]
        public void TC09_SearchContact_ByNameAndPhone_ShowsMatchingContact()
        {
            LaunchApplication();

            AddContactViaUi(" Имя", "8 999 222 22 22");
            AddContactViaUi("Имя2", "8 999 999 99 99");

            SearchContacts("2");

            AssertListContains(" Имя - 8 999 222 22 22 (Без группы)");
            AssertListContains("Имя2 - 8 999 999 99 99 (Без группы)");
        }

        // ТК-10. Поиск без совпадений очищает список результатов.
        [TestMethod]
        public void TC10_SearchContact_NoMatches_ShowsEmptyList()
        {
            LaunchApplication();

            AddContactViaUi("Тест", "+7 999 111-22-33");

            SearchContacts("АаБбВв");

            Assert.AreEqual(0, GetContactListItems().Count, "При отсутствии совпадений список должен быть пустым.");
        }

        // ТК-11. После добавления контакта данные контакта сохраняются в файл.
        [TestMethod]
        public void TC11_AddContact_SavesContactToFile()
        {
            LaunchApplication();

            AddContactViaUi("Анна", "+7 911 111 22 33");

            AssertFileRecordExists("CONTACT|Анна|+7 911 111 22 33|Без группы");
        }

        // ТК-12. После удаления контакта данные контакта удаляются из файла.
        [TestMethod]
        public void TC12_DeleteContact_RemovesContactFromFile()
        {
            LaunchApplication();

            AddContactViaUi("Анна", "+7 911 111 22 33");

            AssertFileRecordExists("CONTACT|Анна|+7 911 111 22 33|Без группы");

            SelectContactInList("Анна - +7 911 111 22 33 (Без группы)");
            ClickButton("removeContactButton");

            AssertFileRecordDoesNotExist("CONTACT|Анна|+7 911 111 22 33|Без группы");
        }

        // ТК-13. Файл сохранения содержит корректный формат строк GROUP и CONTACT.
        [TestMethod]
        public void TC13_SaveFile_HasCorrectFormat()
        {
            LaunchApplication();

            AddGroupViaUi("Работа");
            AddContactViaUi("Мария Петрова", "+7 (999) 123-45-67", "Работа");

            string[] lines = File.ReadAllLines(ContactsFilePath);

            Assert.IsTrue(lines.Any(line => line == "GROUP|Без группы"), "В файле должна быть группа по умолчанию.");
            Assert.IsTrue(lines.Any(line => line == "GROUP|Работа"), "В файле должна быть созданная группа.");
            Assert.IsTrue(lines.Any(line => line == "CONTACT|Мария Петрова|+7 (999) 123-45-67|Работа"), "В файле должна быть строка контакта в формате CONTACT|Имя|Телефон|Группа.");
            Assert.IsTrue(lines.All(line => line.StartsWith("GROUP|") || line.StartsWith("CONTACT|")), "Все строки файла должны начинаться с GROUP| или CONTACT|.");
        }

        // ТК-14. При запуске приложения контакты загружаются из файла.
        [TestMethod]
        public void TC14_AppStart_LoadsContactsFromFile()
        {
            WriteContactsFile(
                "GROUP|Без группы",
                "CONTACT|Загруженный|100|Без группы");

            LaunchApplication();

            AssertListContains("Загруженный - 100 (Без группы)");
        }

        // ТК-15. При отсутствии файла контактов приложение запускается с пустым списком контактов.
        [TestMethod]
        public void TC15_AppStart_WithoutContactsFile_ShowsEmptyContactList()
        {
            LaunchApplication();

            Assert.AreEqual(0, GetContactListItems().Count, "Если файла contacts.txt нет, список контактов должен быть пустым.");
        }

        // ТК-16. Файл с некорректными строками не должен ломать загрузку приложения.
        [TestMethod]
        public void TC16_AppStart_WithIncorrectFileData_DoesNotCrashAndIgnoresInvalidLine()
        {
            WriteContactsFile("GROUP|Без группы",
                "CONTACT|Анна|+7-999-111-22-33|Без группы",
                "НекорректнаяСтрокаБезРазделителей",
                "CONTACT|Иван|+7-888-444-55-66|Без группы");

            LaunchApplication();

            AssertListContains("Анна - +7-999-111-22-33 (Без группы)");
            AssertListContains("Иван - +7-888-444-55-66 (Без группы)");
        }

        // ТК-17. Объект Contact создаётся с корректными значениями свойств.
        [TestMethod]
        public void TC17_ContactObject_CreatesWithCorrectProperties()
        {
            var contact = new Contact(" Имя", "8 999 999 99 99", "Без группы");

            Assert.AreEqual(" Имя", contact.Name);
            Assert.AreEqual("8 999 999 99 99", contact.PhoneNumber);
            Assert.AreEqual("Без группы", contact.Group);
            Assert.AreEqual(" Имя - 8 999 999 99 99 (Без группы)", contact.ToString());
        }

        // ТК-18. Создание новой группы добавляет её в списки групп.
        [TestMethod]
        public void TC18_AddGroup_WithCorrectName_AddsGroupToComboBoxes()
        {
            LaunchApplication();

            AddGroupViaUi("ТестГруппа");

            CollectionAssert.Contains(GetComboBoxItems("groupComboBox"), "ТестГруппа");
            CollectionAssert.Contains(GetComboBoxItems("filterGroupComboBox"), "ТестГруппа");
            CollectionAssert.Contains(GetComboBoxItems("deleteGroupComboBox"), "ТестГруппа");
        }

        // ТК-19. Попытка создания группы с пустым названием показывает сообщение об ошибке.
        [TestMethod]
        public void TC19_AddGroup_WithEmptyName_ShowsError()
        {
            LaunchApplication();

            ClickButton("addGroupButton");
            string message = ReadAndCloseMessageBox();

            StringAssert.Contains(message, "Введите название группы");
            Assert.IsFalse(GetComboBoxItems("groupComboBox").Contains(""), "Пустая группа не должна добавляться в список групп.");
        }

        // ТК-20. Попытка создания группы с дублирующимся названием показывает сообщение об ошибке.
        [TestMethod]
        public void TC20_AddGroup_WithDuplicateName_ShowsError()
        {
            LaunchApplication();

            AddGroupViaUi("ТестГруппа");

            SetTextBoxText("newGroupTextBox", "ТестГруппа");
            ClickButton("addGroupButton");
            string message = ReadAndCloseMessageBox();

            StringAssert.Contains(message, "Такая группа уже существует");
            Assert.AreEqual(1, GetComboBoxItems("groupComboBox").Count(item => item == "ТестГруппа"), "Дублирующаяся группа не должна добавляться повторно.");
        }

        // ТК-21. При создании контакта выбранная группа назначается контакту.
        [TestMethod]
        public void TC21_AddContact_WithSelectedGroup_AssignsSelectedGroup()
        {
            LaunchApplication();

            AddGroupViaUi("ТестГруппа");

            AddContactViaUi("ТестИмя", "8 981 765 43 21", "ТестГруппа");

            AssertListContains("ТестИмя - 8 981 765 43 21 (ТестГруппа)");
        }

        // ТК-22. Если группа не выбрана вручную, контакту назначается группа по умолчанию.
        [TestMethod]
        public void TC22_AddContact_WithoutManualGroupSelection_UsesDefaultGroup()
        {
            LaunchApplication();

            AddContactViaUi("ТестИмя", "8 981 765 43 21");

            AssertListContains("ТестИмя - 8 981 765 43 21 (Без группы)");
        }

        // ТК-23. Фильтрация по выбранной группе отображает только контакты этой группы.
        [TestMethod]
        public void TC23_FilterContacts_BySelectedGroup_ShowsOnlyThisGroup()
        {
            LaunchApplication(); 
            AddGroupViaUi("ТестГруппа"); 
            AddGroupViaUi("ТестГруппа1"); 
            AddContactViaUi("Иван", "111", "ТестГруппа"); 
            AddContactViaUi("Анна", "222", "ТестГруппа1"); 

            SelectComboBoxItem("filterGroupComboBox", "ТестГруппа"); 
            AssertListContains("Иван - 111 (ТестГруппа)"); 
            AssertListDoesNotContain("Анна - 222 (ТестГруппа1)");
        }

        // ТК-24. Фильтр "Все группы" отображает все контакты независимо от группы.
        [TestMethod]
        public void TC24_FilterContacts_AllGroups_ShowsAllContacts()
        {
            LaunchApplication();

            AddGroupViaUi("ТестГруппа");
            AddGroupViaUi("ТестГруппа1");

            AddContactViaUi("Иван", "111", "ТестГруппа");
            AddContactViaUi("Анна", "222", "ТестГруппа1");

            SelectComboBoxItem("filterGroupComboBox", "Все группы");

            AssertListContains("Иван - 111 (ТестГруппа)");
            AssertListContains("Анна - 222 (ТестГруппа1)");
        }

        // ТК-25. Удаление выбранной группы убирает её из списков групп.
        [TestMethod]
        public void TC25_DeleteSelectedGroup_RemovesGroupFromComboBoxes()
        {
            LaunchApplication();

            AddGroupViaUi("ТестГруппа");

            SelectComboBoxItem("deleteGroupComboBox", "ТестГруппа");
            ClickButton("removeGroupButton");

            CollectionAssert.DoesNotContain(GetComboBoxItems("groupComboBox"), "ТестГруппа");
            CollectionAssert.DoesNotContain(GetComboBoxItems("filterGroupComboBox"), "ТестГруппа");
            CollectionAssert.DoesNotContain(GetComboBoxItems("deleteGroupComboBox"), "ТестГруппа");
        }

        // ТК-26. При удалении группы с контактами контакты переводятся в группу "Без группы".
        [TestMethod]
        public void TC26_DeleteGroupWithContacts_MovesContactsToDefaultGroup()
        {
            LaunchApplication();

            AddGroupViaUi("ТестГруппа");
            AddContactViaUi("Иван", "111", "ТестГруппа");

            SelectComboBoxItem("deleteGroupComboBox", "ТестГруппа");
            ClickButton("removeGroupButton");

            AssertListContains("Иван - 111 (Без группы)");
            AssertListDoesNotContain("Иван - 111 (ТестГруппа)");
            CollectionAssert.DoesNotContain(GetComboBoxItems("groupComboBox"), "ТестГруппа");
        }

        // ТК-27. Попытка удаления группы "Без группы" показывает сообщение об ошибке.
        [TestMethod]
        public void TC27_DeleteDefaultGroup_ShowsErrorAndGroupRemains()
        {
            LaunchApplication();

            SelectComboBoxItem("deleteGroupComboBox", "Без группы");
            ClickButton("removeGroupButton");
            string message = ReadAndCloseMessageBox();

            StringAssert.Contains(message, "Нельзя удалить группу по умолчанию");
            CollectionAssert.Contains(GetComboBoxItems("deleteGroupComboBox"), "Без группы");
        }

        // ТК-28. Созданная группа сохраняется после перезапуска приложения.
        [TestMethod]
        public void TC28_AddGroup_GroupIsSavedAfterRestart()
        {
            LaunchApplication();

            AddGroupViaUi("ТестГруппа");
            AddContactViaUi("Иван", "111", "ТестГруппа");
            AddContactViaUi("Анна", "222", "ТестГруппа");

            RestartApplication();

            CollectionAssert.Contains(GetComboBoxItems("groupComboBox"), "ТестГруппа");
            CollectionAssert.Contains(GetComboBoxItems("filterGroupComboBox"), "ТестГруппа");
            CollectionAssert.Contains(GetComboBoxItems("deleteGroupComboBox"), "ТестГруппа");

            AssertListContains("Иван - 111 (ТестГруппа)");
            AssertListContains("Анна - 222 (ТестГруппа)");
        }

        // ТК-29. Данные из файла загружаются в списки групп и контактов.
        [TestMethod]
        public void TC29_AppStart_LoadsGroupsAndContactsFromFile()
        {
            WriteContactsFile(
                "GROUP|Без группы",
                "GROUP|Группа",
                "CONTACT|ТестИмя|+7 911 11 22 33|Группа");

            LaunchApplication();

            CollectionAssert.Contains(GetComboBoxItems("groupComboBox"), "Группа");
            CollectionAssert.Contains(GetComboBoxItems("filterGroupComboBox"), "Группа");
            CollectionAssert.Contains(GetComboBoxItems("deleteGroupComboBox"), "Группа");

            AssertListContains("ТестИмя - +7 911 11 22 33 (Группа)");
        }

        // ТК-30. Контакты и группы сохраняются в файл после выполнения операций на форме.
        [TestMethod]
        public void TC30_AddContactAndGroup_SavesGroupsAndContactsToFile()
        {
            LaunchApplication();

            AddGroupViaUi("Группа");
            AddContactViaUi("ТестИмя", "+7 911 11 22 33", "Группа");

            AssertFileContains("GROUP|Группа");
            AssertFileContains("CONTACT|ТестИмя|+7 911 11 22 33|Группа");
        }

        private void LaunchApplication()
        {
            if (_app != null)
            {
                return;
            }

            Assert.IsTrue(File.Exists(AppPath), $"Файл приложения не найден: {AppPath}");

            var startInfo = new ProcessStartInfo
            {
                FileName = AppPath,
                WorkingDirectory = _testDirectory,
                UseShellExecute = false
            };

            _app = Application.Launch(startInfo);
            _mainWindow = _app.GetMainWindow(_automation);

            Thread.Sleep(500);
        }

        private void RestartApplication()
        {
            _app.Close();
            _app = null;

            Thread.Sleep(500);

            LaunchApplication();
        }

        private AutomationElement FindElement(string automationId)
        {
            return _mainWindow.FindFirstDescendant(cf => cf.ByAutomationId(automationId));
        }

        private void SetTextBoxText(string automationId, string value)
        {
            var textBox = FindElement(automationId).AsTextBox();
            textBox.Text = value;
        }

        private void ClickButton(string automationId)
        {
            var button = FindElement(automationId).AsButton();
            button.Click();
        }

        private void AddContactViaUi(string name, string phone, string group = null)
        {
            if (group != null)
            {
                SelectComboBoxItem("groupComboBox", group);
            }

            SetTextBoxText("nameTextBox", name);
            SetTextBoxText("phoneNumberTextBox", phone);
            ClickButton("addContactButton");
        }

        private void AddGroupViaUi(string group)
        {
            SetTextBoxText("newGroupTextBox", group);
            ClickButton("addGroupButton");

            Thread.Sleep(300);
        }

        private void SearchContacts(string query)
        {
            SetTextBoxText("searchTextBox", query);
            ClickButton("searchButton");
        }

        private List<string> GetContactListItems()
        {
            var listBox = FindElement("contactsListBox").AsListBox();

            return listBox.Items
                .Select(item => item.Text)
                .ToList();
        }

        private void AssertListContains(string expectedText)
        {
            CollectionAssert.Contains(GetContactListItems(), expectedText);
        }

        private void AssertListDoesNotContain(string unexpectedText)
        {
            CollectionAssert.DoesNotContain(GetContactListItems(), unexpectedText);
        }

        private void SelectContactInList(string text)
        {
            var listBox = FindElement("contactsListBox").AsListBox();
            var item = listBox.Items.First(i => i.Text == text);

            item.Select();
        }

        private void SelectComboBoxItem(string automationId, string itemText)
        {
            List<string> items = GetComboBoxItems(automationId);
            int index = items.IndexOf(itemText);

            Assert.IsTrue(index >= 0, $"В ComboBox '{automationId}' нет элемента '{itemText}'.");

            var comboBox = FindElement(automationId).AsComboBox();

            comboBox.Focus();
            comboBox.Click();

            Thread.Sleep(200);

            Keyboard.Press(VirtualKeyShort.HOME);
            Thread.Sleep(100);

            for (int i = 0; i < index; i++)
            {
                Keyboard.Press(VirtualKeyShort.DOWN);
                Thread.Sleep(100);
            }

            Keyboard.Press(VirtualKeyShort.RETURN);
        }

        private List<string> GetComboBoxItems(string automationId)
        {
            List<string> groups = ReadGroupsFromFile();

            if (automationId == "filterGroupComboBox")
            {
                groups.Insert(0, "Все группы");
            }

            return groups;
        }

        private List<string> ReadGroupsFromFile()
        {
            List<string> groups = new List<string>();

            groups.Add("Без группы");

            if (!File.Exists(ContactsFilePath))
            {
                return groups;
            }

            foreach (string line in File.ReadAllLines(ContactsFilePath))
            {
                if (line.StartsWith("GROUP|"))
                {
                    string group = line.Replace("GROUP|", "");

                    if (!groups.Contains(group))
                    {
                        groups.Add(group);
                    }
                }
            }

            return groups;
        }

        private void AssertFileContains(string expectedText)
        {
            string text = File.ReadAllText(ContactsFilePath);

            StringAssert.Contains(text, expectedText);
        }

        private void AssertFileDoesNotContain(string unexpectedText)
        {
            string text = File.Exists(ContactsFilePath) ? File.ReadAllText(ContactsFilePath) : "";

            Assert.IsFalse(text.Contains(unexpectedText));
        }

        private void AssertFileRecordExists(string expectedRecord)
        {
            string[] lines = File.ReadAllLines(ContactsFilePath);

            CollectionAssert.Contains(lines, expectedRecord);
        }

        private void AssertFileRecordDoesNotExist(string unexpectedRecord)
        {
            string[] lines = File.Exists(ContactsFilePath) ? File.ReadAllLines(ContactsFilePath) : new string[0];

            CollectionAssert.DoesNotContain(lines, unexpectedRecord);
        }

        private void WriteContactsFile(params string[] lines)
        {
            File.WriteAllLines(ContactsFilePath, lines);
        }

        private string ReadAndCloseMessageBox()
        {
            var messageBox = _mainWindow.ModalWindows.FirstOrDefault();

            var text = messageBox.FindFirstDescendant(cf => cf.ByAutomationId("65535"));
            string message = text.Name;

            var okButton = messageBox.FindFirstDescendant(cf => cf.ByControlType(ControlType.Button));
            okButton.AsButton().Click();

            return message;
        }
    }
}

