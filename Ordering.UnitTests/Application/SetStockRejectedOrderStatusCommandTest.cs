﻿using eShop.Ordering.API.Application.Commands;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Ordering.UnitTests.Application
{
    [TestClass]
    public class SetStockRejectedOrderStatusCommandTest
    {
        [TestMethod]
        public void Set_Stock_Rejected_Order_Check_Serialization()
        {
            //Arrange
            var command = new SetStockRejectedOrderStatusCommand(123, new List<int> { 1, 2, 3 });

            //Act
            var json = JsonSerializer.Serialize(command);
            var deserializedCommand = JsonSerializer.Deserialize<SetStockRejectedOrderStatusCommand>(json);


            //Assert for List<int>
            Assert.IsNotNull(deserializedCommand.OrderStockItems);
            Assert.AreEqual(command.OrderStockItems.Count, deserializedCommand.OrderStockItems.Count);

            for (int i = 0; i < command.OrderStockItems.Count; i++)
            {
                Assert.AreEqual(command.OrderStockItems[i], deserializedCommand.OrderStockItems[i]);
            }
        }
    }
}
