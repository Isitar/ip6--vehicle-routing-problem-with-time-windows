﻿using System;
using IRuettae.Core.ILPIp5Gurobi.Algorithm;
using System.Linq;
using IRuettae.Core.ILP.Algorithm.Scheduling;
using IRuettae.Core.ILPIp5Gurobi.Algorithm.Scheduling;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using IRuettae.Core.ILPIp5Gurobi.Algorithm.Models;

namespace IRuettae.Core.ILPIp5Gurobi.Tests.Algorithm.Scheduling
{
    [TestClass]
    public class DefaultTargetTest
    {
        public SolverInputData GetModel()
        {
            // 1 Santa, 7 timeslots
            // Visit duration:
            // A : 0
            // B : 1
            // C : 2

            const bool t = true;
            bool[][,] santas =
            {
                new[,]
                {
                    {t, t, t, t, t, t, t},
                }
            };

            VisitState d = VisitState.Default;
            VisitState w = VisitState.Desired;
            VisitState[][,] visits =
            {
                new[,]
                {
                    {d, d, d, d, d, d, d,},
                    {d, d, d, d, d, w, d,},
                    {d, w, w, d, d, d, d,},
                },

            };

            int[,] distances =
            {
                {0, 1, 1},
                {1, 0, 2},
                {1, 1, 0},
            };

            int[] visitLength =
            {
                0, 1, 2,
            };

            return new SolverInputData(santas, visitLength, visits, distances, new[] { 0, 1, 2 }, new[] { 0 }, new int[0], new[] { 0 });
        }

        [TestMethod]
        public void TestTargetFunctionWorksCorrect()
        {
            var model = GetModel();
            var solver = new SchedulingILPSolver(model);
            solver.Solve(0, 60000);
            var result = solver.GetResult();


            var waypoints = result.Waypoints[0, 0];

            // three timeslots in desired should be worth more than one timeslot duration
            Assert.AreEqual(waypoints[1].Visit, 2);
            Assert.AreEqual(waypoints[2].Visit, 1);
        }
    }
}
