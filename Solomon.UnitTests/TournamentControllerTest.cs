using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Solomon.Domain.Abstract;
using Solomon.Domain.Entities;
using Solomon.WebUI.Controllers;

namespace Solomon.UnitTests
{
    [TestClass]
    public class TournamentControllerTest
    {
        [TestMethod]
        public void Can_Get_List_Of_Tournaments()
        {
            // Arrange
            // - create the mock repository.
            Mock<IRepository> mock = new Mock<IRepository>();
            mock.Setup(m => m.Tournaments).Returns(new Tournament[] {
                new Tournament {TournamentID = 1, Name = "T1"},
                new Tournament {TournamentID = 2, Name = "T2"},
                new Tournament {TournamentID = 3, Name = "T3"},
                new Tournament {TournamentID = 4, Name = "T4"},
                new Tournament {TournamentID = 5, Name = "T5"}
            }.AsQueryable());

            // Create a controller.
            TournamentController controller = new TournamentController(mock.Object);

            // Action
            IQueryable<Tournament> result = (IQueryable<Tournament>)controller.List().Model;

            // Assert
            Tournament[] tourArray = result.ToArray();
            Assert.AreEqual(tourArray[0].Name, "T1");
            Assert.AreEqual(tourArray[1].Name, "T2");
            Assert.AreEqual(tourArray[2].Name, "T3");
            Assert.AreEqual(tourArray[3].Name, "T4");
            Assert.AreEqual(tourArray[4].Name, "T5");
        }
    }
}
