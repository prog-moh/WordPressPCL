﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WordPressPCL.Models;

namespace WordPressPCL.Tests.Selfhosted.Utility
{
    [TestClass]
    public class HttpHelper_Tests
    {
        private static WordPressClient _client;
        private static WordPressClient _clientAuth;

        [ClassInitialize]
        public static async Task Init(TestContext testContext)
        {
            _client = ClientHelper.GetWordPressClient();
            _clientAuth = await ClientHelper.GetAuthenticatedWordPressClient();
        }

        [TestMethod]
        public async Task HttpHelper_InvalidPreProcessing()
        {
            var client = await ClientHelper.GetAuthenticatedWordPressClient();

            // Create a random tag , must works:
            var random = new Random();
            var tagname = $"Test {random.Next(0, 1000)}";
            var tag = await client.Tags.Create(new Tag()
            {
                Name = tagname,
                Description = "Test Description"
            });
            Assert.IsTrue(tag.Id > 0);
            Assert.IsNotNull(tag);
            Assert.AreEqual(tagname, tag.Name);
            Assert.AreEqual("Test Description", tag.Description);

            // We call Get tag list without pre processing
            var tags = await client.Tags.GetAll();
            Assert.IsNotNull(tags);
            Assert.AreNotEqual(tags.Count(), 0);
            CollectionAssert.AllItemsAreUnique(tags.Select(e => e.Id).ToList());

            // Now we add a PreProcessing task
            client.HttpResponsePreProcessing = (response) =>
            {
                throw new InvalidOperationException("PreProcessing must failed");
            };

            tags = await client.Tags.GetAll();
            Assert.IsNotNull(tags);
            Assert.AreEqual(0, tags.Count());
            CollectionAssert.AllItemsAreUnique(tags.Select(e => e.Id).ToList());
        }

        [TestMethod]
        public async Task HttpHelper_ValidPreProcessing()
        {
            var client = new WordPressClient(ApiCredentials.WordPressUri);
            client.AuthMethod = AuthMethod.JWT;
            await client.RequestJWToken(ApiCredentials.Username, ApiCredentials.Password);

            // Create a random tag
            var random = new Random();
            var tagname = $"Test {random.Next(0, 1000)}";
            var tag = await client.Tags.Create(new Tag()
            {
                Name = tagname,
                Description = "Test Description"
            });
            Assert.IsTrue(tag.Id > 0);
            Assert.IsNotNull(tag);
            Assert.AreEqual(tagname, tag.Name);
            Assert.AreEqual("Test Description", tag.Description);

            // We call Get tag list without pre processing
            var tags = await client.Tags.GetAll();
            Assert.IsNotNull(tags);
            Assert.AreNotEqual(tags.Count(), 0);
            CollectionAssert.AllItemsAreUnique(tags.Select(e => e.Id).ToList());

            // Now we add a PreProcessing task
            client.HttpResponsePreProcessing = (response) =>
            {
                return response;
            };

            tags = await client.Tags.GetAll();
            Assert.IsNotNull(tags);
            Assert.AreNotEqual(tags.Count(), 0);
            CollectionAssert.AllItemsAreUnique(tags.Select(e => e.Id).ToList());
        }
    }
}
