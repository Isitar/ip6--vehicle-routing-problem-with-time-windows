﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading;
using IRuettae.Core.LocalSolverTests;
using IRuettae.Core.Models;
using IRuettae.Core.LocalSolver.Models;

namespace IRuettae.Core.LocalSolver.Tests
{
    [TestClass]
    public class SolverTests
    {
        private static readonly object Mutex = new object();

        [TestInitialize]
        public void TestStart()
        {
            // Mutex used to run tests sequentially
            Monitor.Enter(Mutex);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Monitor.Exit(Mutex);
        }

        [TestMethod]
        public void TestRouteCostsCorrect()
        {
            //   B -1- A
            //   2|  /3
            //    |/
            //    H

            const int hour = 3600;
            var input = new OptimizationInput
            {
                Visits = new[]
                {
                    new Visit
                    {
                        Id = 0,
                        Duration = 4 * hour,
                        Desired = new (int from, int to)[0],
                        Unavailable = new (int from, int to)[0],
                        WayCostToHome = 3*hour,
                        WayCostFromHome = 3*hour
                    },
                    new Visit
                    {
                        Id = 1,
                        Duration = 5*hour,
                        Desired = new (int from, int to)[0],
                        Unavailable = new (int from, int to)[0],
                        WayCostToHome = 2*hour,
                        WayCostFromHome = 2*hour,
                    },
                },
                Santas = new[] { new Santa { Id = 0 }, },
                Days = new (int from, int to)[] { (0, 100 * hour) },
                RouteCosts = new[,]
                {
                    {0, hour},
                    {hour, 0},
                },
            };

            var solver = new Solver(input, new LocalSolverConfig
            {
                VrpTimeLimitFactor = 0,
                VrptwTimeLimitFactor = 1,
            });
            var output = solver.Solve(3000L, null, null);
            Assert.IsNotNull(output);
            Assert.IsNotNull(output.Routes);
            Assert.IsTrue(output.NonEmptyRoutes.Any());

            Assert.AreEqual(1050, output.Cost());
        }

        [TestMethod]
        public void TestRouteDesiredCorrect()
        {
            //   B -1- A
            //   2|  /3
            //    |/
            //    H

            const int hour = 3600;
            var input = new OptimizationInput
            {
                Visits = new[]
                {
                    new Visit
                    {
                        Id = 0, //A
                        Duration = 4 * hour,
                        Desired = new (int from, int to)[0],
                        Unavailable = new (int from, int to)[0],
                        WayCostToHome = 3*hour,
                        WayCostFromHome = 3*hour
                    },
                    new Visit
                    {
                        Id = 1, // B
                        Duration = 5*hour,
                        Desired = new (int from, int to)[] {(2*hour,4*hour)},
                        Unavailable = new (int from, int to)[0],
                        WayCostToHome = 2*hour,
                        WayCostFromHome = 2*hour,
                    },
                },
                Santas = new[] { new Santa { Id = 0 }, },
                Days = new (int from, int to)[] { (0, 100 * hour) },
                RouteCosts = new[,]
                {
                    {0, hour},
                    {hour, 0},
                },
            };

            var solver = new Solver(input, new LocalSolverConfig
            {
                VrpTimeLimitFactor = 0,
                VrptwTimeLimitFactor = 1,
            });
            var output = solver.Solve(3000L, null, null);
            Assert.IsNotNull(output);
            Assert.IsNotNull(output.Routes);
            Assert.AreEqual(1, output.NonEmptyRoutes.Count());
            Assert.AreEqual(1010, output.Cost());
            Assert.AreEqual(1, output.Routes[0].Waypoints[1].VisitId);
            Assert.AreEqual(0, output.Routes[0].Waypoints[0].StartTime);
            Assert.AreEqual(2 * hour, output.Routes[0].Waypoints[1].StartTime);
        }

        [TestMethod]
        public void TestRouteUnavailableCorrect()
        {
            //   B -1- A
            //   2|  /3
            //    |/
            //    H

            const int hour = 3600;
            var input = new OptimizationInput
            {
                Visits = new[]
                {
                    new Visit
                    {
                        Id = 0, //A
                        Duration = 4 * hour,
                        Desired = new (int from, int to)[0],
                        Unavailable = new (int from, int to)[] {(7*hour, 100*hour)},
                        WayCostToHome = 3*hour,
                        WayCostFromHome = 3*hour
                    },
                    new Visit
                    {
                        Id = 1, // B
                        Duration = 5*hour,
                        Desired = new (int from, int to)[0],
                        Unavailable = new (int from, int to)[0],
                        WayCostToHome = 2*hour,
                        WayCostFromHome = 2*hour,
                    },
                },
                Santas = new[] { new Santa { Id = 0 }, },
                Days = new (int from, int to)[] { (0, 100 * hour) },
                RouteCosts = new[,]
                {
                    {0, hour},
                    {hour, 0},
                },
            };

            var solver = new Solver(input, new LocalSolverConfig
            {
                VrpTimeLimitFactor = 0,
                VrptwTimeLimitFactor = 1,
            });
            var output = solver.Solve(3000L, null, null);
            Assert.IsNotNull(output);
            Assert.IsNotNull(output.Routes);
            Assert.AreEqual(1, output.NonEmptyRoutes.Count());
            Assert.AreEqual(1050, output.Cost());
            Assert.AreEqual(0, output.Routes[0].Waypoints[1].VisitId);
            Assert.AreEqual(0, output.Routes[0].Waypoints[0].StartTime);
            Assert.AreEqual(3 * hour, output.Routes[0].Waypoints[1].StartTime);
        }

        [TestMethod]
        public void TestBreaksCorrect()
        {
            var (input, _) = DatasetFactory.LocalSolverBreakDataSet();
            var solver = new Solver(input, new LocalSolverConfig
            {
                VrpTimeLimitFactor = 0,
                VrptwTimeLimitFactor = 1,
            });
            var output = solver.Solve(10000L, null, null);
            Assert.IsNotNull(output);
            Assert.IsNotNull(output.Routes);
            var santa0Break = input.Visits.First(v => v.IsBreak && v.SantaId == 0);
            var santa1Break = input.Visits.First(v => v.IsBreak && v.SantaId == 1);
            var santa2Break = input.Visits.First(v => v.IsBreak && v.SantaId == 2);
            Assert.IsTrue(output.Routes.Where(r => r.SantaId == 0).All(r => r.Waypoints == null || r.Waypoints.Length == 0 || r.Waypoints.Select(wp => wp.VisitId).Contains(santa0Break.Id)));
            Assert.IsTrue(output.Routes.Where(r => r.SantaId == 1).All(r => r.Waypoints == null || r.Waypoints.Length == 0 || r.Waypoints.Select(wp => wp.VisitId).Contains(santa1Break.Id)));
            Assert.IsTrue(output.Routes.Where(r => r.SantaId == 2).All(r => r.Waypoints == null || r.Waypoints.Length == 0 || r.Waypoints.Select(wp => wp.VisitId).Contains(santa2Break.Id)));
        }
    }
}
