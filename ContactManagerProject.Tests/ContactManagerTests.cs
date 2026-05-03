using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace ContactManagerProject.Tests
{
    [TestClass]
    public class ContactManagerTests
    {
        private string _startDir;
        private string _testDir;

        [TestInitialize]
        public void Setup()
        {
            _startDir = Directory.GetCurrentDirectory();
            _testDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_testDir);
            Directory.SetCurrentDirectory(_testDir);
        }

        [TestCleanup]
        public void Cleanup()
        {
            Directory.SetCurrentDirectory(_startDir);
            if (Directory.Exists(_testDir))
            {
                Directory.Delete(_testDir, true);
            }
        }

        [TestMethod]
        public void AddContact_AddsContact()
        {
            var manager = new ContactManager();
            manager.AddContact(new Contact("Иван", "12345"));
            Assert.AreEqual(1, manager.Contacts.Count);
        }

        [TestMethod]
        public void SearchContacts_FindsByName()
        {
            var manager = new ContactManager();
            manager.AddContact(new Contact("Иван", "12345"));
            var result = manager.SearchContacts("Иван");
            Assert.AreEqual(1, result.Count);
        }
    }
}
