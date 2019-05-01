using System;
using System.Collections.Generic;
using System.Text;
using JN.MicroHttpServer.Entities;
using JN.MicroHttpServer.HelperClasses;
using NUnit.Framework;

namespace JN.MicroHttpServer.Tests
{
    public class ToolsTests
    {


        [Test]
        public void ExistsUrlConfiguredWithOtherMethod_ExistsItemButNotOther_returnsFalse()
        {

            var item = new ConfigItem()
            {
                DelegateToExecute = null,
                HttpMethod = HttpMethod.POST,
                Uri = "http://test"
            };


            var config = new List<ConfigItem>
            {
                item
            };

            var res = config.ExistsUrlConfiguredWithOtherMethod("http://test", "POST");

            Assert.IsFalse(res);
        }

        [Test]
        public void ExistsUrlConfiguredWithOtherMethod_NotExistsItem_returnsFalse()
        {

            var item = new ConfigItem()
            {
                DelegateToExecute = null,
                HttpMethod = HttpMethod.POST,
                Uri = "http://test123"
            };


            var config = new List<ConfigItem>
            {
                item
            };

            var res = config.ExistsUrlConfiguredWithOtherMethod("http://test", "POST");

            Assert.IsFalse(res);
        }

        [Test]
        public void ExistsUrlConfiguredWithOtherMethod_ExistsItemAndExistsOtherItem_returnsTrue()
        {

            var item = new ConfigItem()
            {
                DelegateToExecute = null,
                HttpMethod = HttpMethod.POST,
                Uri = "http://test"
            };

            var item2 = new ConfigItem()
            {
                DelegateToExecute = null,
                HttpMethod = HttpMethod.GET,
                Uri = "http://test"
            };


            var config = new List<ConfigItem>
            {
                item, item2
            };

            var res = config.ExistsUrlConfiguredWithOtherMethod("http://test", "POST");

            Assert.IsTrue(res);
        }

        [Test]
        public void ExistsUrlConfiguredWithOtherMethod_ExistsItemButNotExistsOtherItemConfigured_returnsFalse()
        {

            var item = new ConfigItem()
            {
                DelegateToExecute = null,
                HttpMethod = HttpMethod.POST,
                Uri = "http://test"
            };

            var item2 = new ConfigItem()
            {
                DelegateToExecute = null,
                HttpMethod = HttpMethod.GET,
                Uri = "http://testOther"
            };


            var config = new List<ConfigItem>
            {
                item, item2
            };

            var res = config.ExistsUrlConfiguredWithOtherMethod("http://test", "POST");

            Assert.IsFalse(res);

        }

        [Test]
        public void ExistsUrlConfiguredWithOtherMethod_InvalidConfig_returnsFalse()
        {

            List<ConfigItem> config = null;

            var res = config.ExistsUrlConfiguredWithOtherMethod("http://test", "POST");

            Assert.IsFalse(res);

        }

        //-------------------------

        [Test]
        public void GetConfigItem_validConfiguration_returnsItem()
        {

            var item = new ConfigItem()
            {
                DelegateToExecute = null,
                HttpMethod = HttpMethod.POST,
                Uri = "http://test"
            };

            var item2 = new ConfigItem()
            {
                DelegateToExecute = null,
                HttpMethod = HttpMethod.GET,
                Uri = "http://test"
            };


            var config = new List<ConfigItem>
            {
                item, item2
            };

            var res = config.GetConfigItem("http://test", "POST");

            Assert.IsNotNull(res);
            Assert.AreEqual(item, res);
        }

        [Test]
        public void GetConfigItem_validConfigurationGetWrongMethod_returnsNull()
        {

            var item = new ConfigItem()
            {
                DelegateToExecute = null,
                HttpMethod = HttpMethod.POST,
                Uri = "http://test"

            };

            var config = new List<ConfigItem> { item };

            var res = config.GetConfigItem("http://test", "GET");

            Assert.IsNull(res);
        }

        [Test]
        public void GetConfigItem_validConfigurationItemNotExists_returnsNull()
        {

            var item = new ConfigItem()
            {
                DelegateToExecute = null,
                HttpMethod = HttpMethod.POST,
                Uri = "http://test"

            };

            var config = new List<ConfigItem> { item };

            var res = config.GetConfigItem("http://testNotExists", "POST");

            Assert.IsNull(res);
        }

        [Test]
        public void GetConfigItem_emptyConfiguration_returnsNull()
        {

            var config = new List<ConfigItem>();

            var res = config.GetConfigItem("http://testNotExists", "POST");

            Assert.IsNull(res);
        }

        [Test]
        public void GetConfigItem_nullConfiguration_returnsNull()
        {

            List<ConfigItem> config = null;

            var res = config.GetConfigItem("http://testNotExists", "POST");

            Assert.IsNull(res);
        }
    }
}
