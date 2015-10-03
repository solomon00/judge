//using Solomon.Domain.Abstract;
//using Solomon.Domain.Entities;
//using Solomon.WebUI.Controllers;
//using Solomon.WebUI.Models;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using Moq;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Web;
//using System.Web.Mvc;

//namespace Solomon.UnitTests
//{
//    [TestClass]
//    public class ProblemControllerTest
//    {
//        [TestMethod]
//        public void Can_Get_Default_Problem()
//        {
//            // Arrange - create the mock repository.
//            Mock<IRepository> mock = new Mock<IRepository>();
//            var Tournaments = new Tournament[] {
//                new Tournament {TournamentID = 1, Name = "T1"},
//                new Tournament {TournamentID = 2, Name = "T2"},
//                new Tournament {TournamentID = 3, Name = "T3"}
//            };
            
//            var Problems = new Problem[] {
//                new Problem {ProblemID = 1, Name = "P1"},
//                new Problem {ProblemID = 2, Name = "P2"},
//                new Problem {ProblemID = 3, Name = "P3"},
//                new Problem {ProblemID = 4, Name = "P4"},
//                new Problem {ProblemID = 5, Name = "P5"},
//                new Problem {ProblemID = 6, Name = "P6"}
//            };

//            var ProblemTournaments = new ProblemTournament[] {
//                new ProblemTournament {ProblemTournamentID = 1, ProblemID = 1, TournamentID = 1},
//                new ProblemTournament {ProblemTournamentID = 2, ProblemID = 2, TournamentID = 1},
//                new ProblemTournament {ProblemTournamentID = 3, ProblemID = 3, TournamentID = 3},
//                new ProblemTournament {ProblemTournamentID = 4, ProblemID = 4, TournamentID = 2},
//                new ProblemTournament {ProblemTournamentID = 5, ProblemID = 5, TournamentID = 2},
//                new ProblemTournament {ProblemTournamentID = 6, ProblemID = 6, TournamentID = 1}
//            };

//            Problems[0].ProblemTournaments = new List<ProblemTournament>();

//            foreach (var pt in ProblemTournaments)
//            {
//                pt.Problem = Problems
//                    .Where(p => p.ProblemID == pt.ProblemID)
//                    .FirstOrDefault();

//                pt.Tournament = Tournaments
//                    .Where(t => t.TournamentID == pt.TournamentID)
//                    .FirstOrDefault();
//            }

//            foreach (var p in Problems)
//            {
//                var pts = ProblemTournaments
//                    .Where(pt => pt.ProblemID == p.ProblemID);
//                p.ProblemTournaments = new List<ProblemTournament>();
//                foreach (var pt in pts)
//                    p.ProblemTournaments.Add(pt);
//            }

//            foreach (var t in Tournaments)
//            {
//                var pts = ProblemTournaments
//                    .Where(pt => pt.TournamentID == t.TournamentID);
//                t.ProblemTournaments = new List<ProblemTournament>();
//                foreach (var pt in pts)
//                    t.ProblemTournaments.Add(pt);
//            }

//            mock.Setup(m => m.Tournaments).Returns(Tournaments.AsQueryable());

//            mock.Setup(m => m.Problems).Returns(Problems.AsQueryable());

//            mock.Setup(m => m.ProblemTournaments).Returns(ProblemTournaments.AsQueryable());

//            // Create a controller.
//            ProblemController controller = new ProblemController(mock.Object);

//            // Action - get default problem of tournament with id: 1.
//            ProblemViewModel result = (ProblemViewModel)controller.Problem(1).Model;

//            // Assert
//            Assert.AreEqual(result.Problem.Name, "P1");
//        }

//        [TestMethod]
//        public void Can_Get_Custom_Problem()
//        {
//            // Arrange - create the mock repository.
//            Mock<IRepository> mock = new Mock<IRepository>();
//            var Tournaments = new Tournament[] {
//                new Tournament {TournamentID = 1, Name = "T1"},
//                new Tournament {TournamentID = 2, Name = "T2"},
//                new Tournament {TournamentID = 3, Name = "T3"}
//            };

//            var Problems = new Problem[] {
//                new Problem {ProblemID = 1, Name = "P1"},
//                new Problem {ProblemID = 2, Name = "P2"},
//                new Problem {ProblemID = 3, Name = "P3"},
//                new Problem {ProblemID = 4, Name = "P4"},
//                new Problem {ProblemID = 5, Name = "P5"},
//                new Problem {ProblemID = 6, Name = "P6"}
//            };

//            var ProblemTournaments = new ProblemTournament[] {
//                new ProblemTournament {ProblemTournamentID = 1, ProblemID = 1, TournamentID = 1},
//                new ProblemTournament {ProblemTournamentID = 2, ProblemID = 2, TournamentID = 1},
//                new ProblemTournament {ProblemTournamentID = 3, ProblemID = 3, TournamentID = 3},
//                new ProblemTournament {ProblemTournamentID = 4, ProblemID = 4, TournamentID = 2},
//                new ProblemTournament {ProblemTournamentID = 5, ProblemID = 5, TournamentID = 2},
//                new ProblemTournament {ProblemTournamentID = 6, ProblemID = 6, TournamentID = 1}
//            };

//            Problems[0].ProblemTournaments = new List<ProblemTournament>();

//            foreach (var pt in ProblemTournaments)
//            {
//                pt.Problem = Problems
//                    .Where(p => p.ProblemID == pt.ProblemID)
//                    .FirstOrDefault();

//                pt.Tournament = Tournaments
//                    .Where(t => t.TournamentID == pt.TournamentID)
//                    .FirstOrDefault();
//            }

//            foreach (var p in Problems)
//            {
//                var pts = ProblemTournaments
//                    .Where(pt => pt.ProblemID == p.ProblemID);
//                p.ProblemTournaments = new List<ProblemTournament>();
//                foreach (var pt in pts)
//                    p.ProblemTournaments.Add(pt);
//            }

//            foreach (var t in Tournaments)
//            {
//                var pts = ProblemTournaments
//                    .Where(pt => pt.TournamentID == t.TournamentID);
//                t.ProblemTournaments = new List<ProblemTournament>();
//                foreach (var pt in pts)
//                    t.ProblemTournaments.Add(pt);
//            }

//            mock.Setup(m => m.Tournaments).Returns(Tournaments.AsQueryable());

//            mock.Setup(m => m.Problems).Returns(Problems.AsQueryable());

//            mock.Setup(m => m.ProblemTournaments).Returns(ProblemTournaments.AsQueryable());

//            // Create a controller.
//            ProblemController controller = new ProblemController(mock.Object);

//            // Action - get problem with id: 2 of tournament with id: 1.
//            ProblemViewModel result = (ProblemViewModel)controller.Problem(1, 2).Model;

//            // Assert
//            Assert.AreEqual(result.Problem.Name, "P2");
//        }

//        [TestMethod]
//        public void Can_Get_Invalid_Problem()
//        {
//            // Arrange - create the mock repository.
//            Mock<IRepository> mock = new Mock<IRepository>();
//            var Tournaments = new Tournament[] {
//                new Tournament {TournamentID = 1, Name = "T1"},
//                new Tournament {TournamentID = 2, Name = "T2"},
//                new Tournament {TournamentID = 3, Name = "T3"}
//            };

//            var Problems = new Problem[] {
//                new Problem {ProblemID = 1, Name = "P1"},
//                new Problem {ProblemID = 2, Name = "P2"},
//                new Problem {ProblemID = 3, Name = "P3"},
//                new Problem {ProblemID = 4, Name = "P4"},
//                new Problem {ProblemID = 5, Name = "P5"},
//                new Problem {ProblemID = 6, Name = "P6"}
//            };

//            var ProblemTournaments = new ProblemTournament[] {
//                new ProblemTournament {ProblemTournamentID = 1, ProblemID = 1, TournamentID = 1},
//                new ProblemTournament {ProblemTournamentID = 2, ProblemID = 2, TournamentID = 1},
//                new ProblemTournament {ProblemTournamentID = 3, ProblemID = 3, TournamentID = 3},
//                new ProblemTournament {ProblemTournamentID = 4, ProblemID = 4, TournamentID = 2},
//                new ProblemTournament {ProblemTournamentID = 5, ProblemID = 5, TournamentID = 2},
//                new ProblemTournament {ProblemTournamentID = 6, ProblemID = 6, TournamentID = 1}
//            };

//            Problems[0].ProblemTournaments = new List<ProblemTournament>();

//            foreach (var pt in ProblemTournaments)
//            {
//                pt.Problem = Problems
//                    .Where(p => p.ProblemID == pt.ProblemID)
//                    .FirstOrDefault();

//                pt.Tournament = Tournaments
//                    .Where(t => t.TournamentID == pt.TournamentID)
//                    .FirstOrDefault();
//            }

//            foreach (var p in Problems)
//            {
//                var pts = ProblemTournaments
//                    .Where(pt => pt.ProblemID == p.ProblemID);
//                p.ProblemTournaments = new List<ProblemTournament>();
//                foreach (var pt in pts)
//                    p.ProblemTournaments.Add(pt);
//            }

//            foreach (var t in Tournaments)
//            {
//                var pts = ProblemTournaments
//                    .Where(pt => pt.TournamentID == t.TournamentID);
//                t.ProblemTournaments = new List<ProblemTournament>();
//                foreach (var pt in pts)
//                    t.ProblemTournaments.Add(pt);
//            }

//            mock.Setup(m => m.Tournaments).Returns(Tournaments.AsQueryable());

//            mock.Setup(m => m.Problems).Returns(Problems.AsQueryable());

//            mock.Setup(m => m.ProblemTournaments).Returns(ProblemTournaments.AsQueryable());

//            // Create a controller.
//            ProblemController controller = new ProblemController(mock.Object);

//            // Action - get invalid problem.
//            ActionResult result;
//            try
//            {
//                result = controller.Problem(1, 7);
//            }
//            catch (HttpException e)
//            {
//                if (e == new HttpException(404, "Problem not found"))
//                    result = null;
//            }

//            // Assert
//            //Assert.IsNull(result);
//        }

//        [TestMethod]
//        public void Can_Get_Problems_List()
//        {
//            // Arrange - create the mock repository.
//            Mock<IRepository> mock = new Mock<IRepository>();
//            var Tournaments = new Tournament[] {
//                new Tournament {TournamentID = 1, Name = "T1"},
//                new Tournament {TournamentID = 2, Name = "T2"},
//                new Tournament {TournamentID = 3, Name = "T3"}
//            };

//            var Problems = new Problem[] {
//                new Problem {ProblemID = 1, Name = "P1"},
//                new Problem {ProblemID = 2, Name = "P2"},
//                new Problem {ProblemID = 3, Name = "P3"},
//                new Problem {ProblemID = 4, Name = "P4"},
//                new Problem {ProblemID = 5, Name = "P5"},
//                new Problem {ProblemID = 6, Name = "P6"}
//            };

//            var ProblemTournaments = new ProblemTournament[] {
//                new ProblemTournament {ProblemTournamentID = 1, ProblemID = 1, TournamentID = 1},
//                new ProblemTournament {ProblemTournamentID = 2, ProblemID = 2, TournamentID = 1},
//                new ProblemTournament {ProblemTournamentID = 3, ProblemID = 3, TournamentID = 3},
//                new ProblemTournament {ProblemTournamentID = 4, ProblemID = 4, TournamentID = 2},
//                new ProblemTournament {ProblemTournamentID = 5, ProblemID = 5, TournamentID = 2},
//                new ProblemTournament {ProblemTournamentID = 6, ProblemID = 6, TournamentID = 1}
//            };

//            Problems[0].ProblemTournaments = new List<ProblemTournament>();

//            foreach (var pt in ProblemTournaments)
//            {
//                pt.Problem = Problems
//                    .Where(p => p.ProblemID == pt.ProblemID)
//                    .FirstOrDefault();

//                pt.Tournament = Tournaments
//                    .Where(t => t.TournamentID == pt.TournamentID)
//                    .FirstOrDefault();
//            }

//            foreach (var p in Problems)
//            {
//                var pts = ProblemTournaments
//                    .Where(pt => pt.ProblemID == p.ProblemID);
//                p.ProblemTournaments = new List<ProblemTournament>();
//                foreach (var pt in pts)
//                    p.ProblemTournaments.Add(pt);
//            }

//            foreach (var t in Tournaments)
//            {
//                var pts = ProblemTournaments
//                    .Where(pt => pt.TournamentID == t.TournamentID);
//                t.ProblemTournaments = new List<ProblemTournament>();
//                foreach (var pt in pts)
//                    t.ProblemTournaments.Add(pt);
//            }

//            mock.Setup(m => m.Tournaments).Returns(Tournaments.AsQueryable());

//            mock.Setup(m => m.Problems).Returns(Problems.AsQueryable());

//            mock.Setup(m => m.ProblemTournaments).Returns(ProblemTournaments.AsQueryable());

//            // Create a controller.
//            ProblemController controller = new ProblemController(mock.Object);

//            // Action - get problems of tournament with id: 1.
//            ProblemsListViewModel result = (ProblemsListViewModel)controller.ProblemsList(1).Model;
//            var resultProblems = result.Problems.ToArray();

//            // Assert
//            Assert.IsTrue(resultProblems.Length == 3);
//            Assert.AreEqual(resultProblems[0].Name, "P1");
//            Assert.AreEqual(resultProblems[1].Name, "P2");
//            Assert.AreEqual(resultProblems[2].Name, "P6");
//        }
//    }
//}
