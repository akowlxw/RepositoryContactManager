using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ContactManagerProject.Tests
{
    [TestClass]
    public class ContactManagerTests
    {
        [TestInitialize]
        public void SetUp()
        {
            if (File.Exists("contacts.txt"))
                File.Delete("contacts.txt");
        }
        [TestCleanup]
        public void TearDown()
        {
            if (File.Exists("contacts.txt"))
                File.Delete("contacts.txt");
        }

        // Constructor создаёт пустой список контактов
        [TestMethod]
        public void Constructor_InitEmptyList()
        {
            var manager = new ContactManager();

            Assert.IsNotNull(manager.Contacts);
            Assert.AreEqual(0, manager.Contacts.Count);
        }

        // AddContact успешно добавляет контакт в коллекцию
        [TestMethod]
        public void AddContact_Successfull()
        {
            var manager = new ContactManager();
            var contact = new Contact("Тест", "+79891234567");

            manager.AddContact(contact);

            Assert.AreEqual(1, manager.Contacts.Count);
            CollectionAssert.Contains(manager.Contacts, contact);
        }

        // AddContact выбрасывает ArgumentNullException при передаче null
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddContact_Null_ThrowsException()
        {
            var manager = new ContactManager();

            manager.AddContact(null);
        }

        // RemoveContact успешно удаляет контакт из коллекции
        [TestMethod]
        public void RemoveContact_Successfull()
        {
            var manager = new ContactManager();
            var contact = new Contact("Тест", "+79891234567");
            manager.AddContact(contact);

            manager.RemoveContact(contact);

            Assert.AreEqual(0, manager.Contacts.Count);
        }

        // RemoveContact выбрасывает ArgumentNullException при передаче null
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RemoveContact_Null_ThrowsException()
        {
            var manager = new ContactManager();

            manager.RemoveContact(null);
        }

        // SearchContacts находит контакт по имени
        [TestMethod]
        public void SearchContacts_NameMatch_FindsContact()
        {
            var manager = new ContactManager();
            manager.AddContact(new Contact("Имя1", "+7991111111"));
            manager.AddContact(new Contact("Имя2", "+79992222222"));

            var results = manager.SearchContacts("Имя1");

            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("Имя1", results[0].Name);
        }
        // SearchContacts находит контакт по номеру телефона
        [TestMethod]
        public void SearchContacts_PhoneMatch_FindsContact()
        {
            var manager = new ContactManager();
            manager.AddContact(new Contact("Имя1", "+79991111111"));
            manager.AddContact(new Contact("Имя2", "+79992222222"));

            var results = manager.SearchContacts("+79992222222");

            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("Имя2", results[0].Name);
        }

        // SearchContacts возвращает пустой список при отсутствии совпадений
        [TestMethod]
        public void SearchContacts_NoMatches_ReturnEmptyList()
        {
            var manager = new ContactManager();
            manager.AddContact(new Contact("Имя1", "+79991111111"));

            var results = manager.SearchContacts("Тест");

            Assert.IsNotNull(results);
            Assert.AreEqual(0, results.Count);
        }
        // SearchContacts с пустой строкой возвращает все контакты
        [TestMethod]
        public void SearchContacts_EmptyString_ReturnsAllContacts()
        {
            var manager = new ContactManager();
            manager.AddContact(new Contact("Имя1", "+79991111111"));
            manager.AddContact(new Contact("Имя2", "+79992222222"));

            var results = manager.SearchContacts("");

            Assert.AreEqual(2, results.Count);
        }

        //
        // тестирование функционала групп контактов
        //
        // добавление новой группы, появление в списке групп
        [TestMethod]
        public void AddGroup_AddsNewGroupToList()
        {
            var manager = new ContactManager();

            manager.AddGroup("ТестГруппа");

            Assert.IsTrue(manager.GetGroups().Contains("ТестГруппа"));
        }

        // Добавление дублирующей группы вызывает исключение
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void AddGroup_Duplicate_ThrowsException()
        {
            var manager = new ContactManager();

            manager.AddGroup("ТестГруппа");
            manager.AddGroup("ТестГруппа"); // ошибка
        }

        // Добавление группы с пустым именем группы вызывает исключение
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void AddGroup_EmptyName_ThrowsException()
        {
            var manager = new ContactManager();

            manager.AddGroup("");
        }

        // Удаление группы убирает её из списка групп
        [TestMethod]
        public void RemoveGroup_RemovesGroupFromList()
        {
            var manager = new ContactManager();
            manager.AddGroup("ТестГруппа");

            manager.RemoveGroup("ТестГруппа");

            Assert.IsFalse(manager.GetGroups().Contains("ТестГруппа"));
        }

        // AddContact с указанием группы сохраняет переданную группу
        [TestMethod]
        public void AddContact_WithGroup_SetsProvidedGroup()
        {
            var manager = new ContactManager();
            manager.AddGroup("ТестГруппа");
            var expectedGroup = "ТестГруппа";

            manager.AddContact(new Contact("ТестИмя", "12345", expectedGroup));

            Assert.AreEqual(expectedGroup, manager.Contacts[0].Group);
        }


        // Контакту без указания группы присваивается группа по умолчанию
        [TestMethod]
        public void AddContact_WithoutGroup_UsesDefaultGroup()
        {
            var manager = new ContactManager();

            manager.AddContact(new Contact("ТестИмя", "12345", ""));

            Assert.AreEqual(ContactManager.DefaultGroup, manager.Contacts[0].Group);
        }

        // Фильтрация по названию группы возвращает соответствующие контакты
        [TestMethod]
        public void SearchContacts_FindsByGroup()
        {
            var manager = new ContactManager();
            manager.AddGroup("ТестГруппа");
            manager.AddContact(new Contact("ТестИмя", "12345", "ТестГруппа"));

            var result = manager.SearchContacts("ТестГруппа");

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("ТестИмя", result.First().Name);
        }

        // Фильтрация по "Все группы" возвращает все контакты
        [TestMethod]
        public void GetContactsByGroup_AllGroups_ReturnsAll()
        {
            var manager = new ContactManager();
            manager.AddContact(new Contact("ТестИмя1", "111", ContactManager.DefaultGroup));
            manager.AddContact(new Contact("ТестИмя2", "222", ContactManager.DefaultGroup));

            var result = manager.GetContactsByGroup(ContactManager.AllGroups);

            Assert.AreEqual(manager.Contacts.Count, result.Count);
        }

        // Загрузка данных сохраняет группы, даже если в них нет контактов
        [TestMethod]
        public void LoadContacts_LoadsSavedGroupWithoutContacts()
        {
            var manager = new ContactManager();
            manager.AddGroup("ТестГруппа");

            var loadedManager = new ContactManager();

            Assert.IsTrue(loadedManager.GetGroups().Contains("ТестГруппа"));
        }

        // Загрузка и сохранение групп с контактами с использованием рефлексии для доступа к приватному методу SaveContacts и приватным полям Groups и Contacts
        [TestMethod]
        public void SaveAndLoad_GroupsAndContacts_Reflection()
        {
            var manager = new ContactManager();

            manager.AddGroup("ТестГруппа");
            manager.AddContact(new Contact("ТестИмя", "99999", "ТестГруппа"));

            var saveMethod = typeof(ContactManager).GetMethod(
                "SaveContacts",
                BindingFlags.NonPublic | BindingFlags.Instance);

            saveMethod.Invoke(manager, null);

            var loaded = new ContactManager();

            var groupsField = typeof(ContactManager).GetProperty(
                "Groups",
                BindingFlags.Public | BindingFlags.Instance);

            var contactsField = typeof(ContactManager).GetProperty(
                "Contacts",
                BindingFlags.Public | BindingFlags.Instance);

            var groups = (List<string>)groupsField.GetValue(loaded);
            var contacts = (List<Contact>)contactsField.GetValue(loaded);

            Assert.IsTrue(groups.Contains("ТестГруппа"));
            Assert.AreEqual("ТестГруппа", contacts[0].Group);
            Assert.AreEqual("ТестИмя", contacts[0].Name);
            Assert.AreEqual("99999", contacts[0].PhoneNumber);
        }
    }
}
