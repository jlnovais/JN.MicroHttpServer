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
        public void GetConfigItem_validConfiguration_returnsItem()
        {

            var item = new ConfigItem()
            {
                DelegateToExecute = null,
                HttpMethod = HttpMethod.POST,
                Uri = "http://test"

            };

            var config = new List<ConfigItem> {item};

            var res = config.GetConfigItem("http://test");

            Assert.IsNotNull(res);
            Assert.AreEqual(item, res);
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

            var res = config.GetConfigItem("http://testNotExists");

            Assert.IsNull(res);
        }

        [Test]
        public void GetConfigItem_emptyConfiguration_returnsNull()
        {

            var config = new List<ConfigItem>();

            var res = config.GetConfigItem("http://testNotExists");

            Assert.IsNull(res);
        }

        [Test]
        public void GetConfigItem_nullConfiguration_returnsNull()
        {

            List<ConfigItem> config = null;

            var res = config.GetConfigItem("http://testNotExists");

            Assert.IsNull(res);
        }
    }
}
