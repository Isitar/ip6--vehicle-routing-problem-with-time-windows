﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IRuettae.Core.ILP.Algorithm;
using IRuettae.Core.ILP.Algorithm.Models;
using IRuettae.Core.ILP.Algorithm.Scheduling;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IRuettae.Core.ILP.Tests.Algorithm.Scheduling
{
    [TestClass]
    public class StarterTest
    {

        public SolverInputData GetModel()
        {
            const bool t = true;
            //const bool f = false;
            bool[][,] santas = {
                new bool[,] {
                    { t, t, t, t, t, t },
                },
                new bool[,] {
                    { t, t, t, t, t, t, t },
                },
            };

            VisitState d = VisitState.Default;
            //VisitState n = VisitState.NotAvailable;
            VisitState[][,] visits = {
                new VisitState[,] {
                    { d, d, d, d, d, d },
                    { d, d, d, d, d, d },
                    { d, d, d, d, d, d },
                    { d, d, d, d, d, d },
                    { d, d, d, d, d, d },
                },
                new VisitState[,] {
                    { d, d, d, d, d, d, d },
                    { d, d, d, d, d, d, d },
                    { d, d, d, d, d, d, d },
                    { d, d, d, d, d, d, d },
                    { d, d, d, d, d, d, d },
                },
            };

            var X = int.MaxValue;
            int[,] distances = {
                { 0, 1, 1, 1, 1},
                { 1, 0, 1, X, X},
                { 1, X, 0, X, X},
                { 1, X, X, 0, 1},
                { 1, X, X, X, 0},
            };

            int[] visitLength =
            {
                0, 1, 2, 2, 2,
            };

            return new SolverInputData(santas, visitLength, visits, distances, new[] { 0, 1, 2, 3, 4 }, new[] { 0 }, new int[0], new[] { 0 });
        }

        [TestMethod]
        public void TestSolve()
        {
            var model = GetModel();
            var solver = new SchedulingILPSolver(model);
            var resultState = solver.Solve(0, 60000);
            var actual = solver.GetResult();
            var expected = new Route(model.Santas.Length, model.Santas[0].GetLength(0))
            {
                Waypoints = new List<Waypoint>[,] {
                    { // santa 1
                        new List<Waypoint>()
                        { // day 1
                            new Waypoint (0,-1),
                            new Waypoint (1, 1),
                            new Waypoint (2, 3),
                            new Waypoint (0, 6),
                        },
                        new List<Waypoint>()
                        { // day 2
                            new Waypoint (0,-1),
                            new Waypoint (3, 1),
                            new Waypoint (4, 4),
                            new Waypoint (0, 7),
                        },
                    },
                }
            };

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestModel()
        {
            Assert.IsTrue(GetModel().IsValid());
        }

        [TestMethod]
        public void TestIsFeasible()
        {
            var model = GetModel();
            // shouldn't throw an exception
            var solver = new SchedulingILPSolver(model);
            var resultState = solver.Solve(0, 60000);

            Assert.AreNotEqual(ResultState.Infeasible, resultState);
        }
    }
}
