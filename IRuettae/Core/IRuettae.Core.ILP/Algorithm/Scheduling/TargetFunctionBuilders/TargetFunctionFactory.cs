﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.OrTools.LinearSolver;
using IRuettae.Core.ILP.Algorithm.Scheduling.Detail;

namespace IRuettae.Core.ILP.Algorithm.Scheduling.TargetFunctionBuilders
{
    internal class TargetFunctionFactory
    {
        private readonly SolverData solverData;

        public TargetFunctionFactory(SolverData solverData)
        {
            this.solverData = solverData;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="target"></param>
        /// <param name="weight">1 is minimal weight, use higher weight for more important targets</param>
        /// <returns>LinearExpr which should be minimised</returns>
        public LinearExpr CreateTargetFunction(TargetType target, int weight = 1)
        {
            switch (target)
            {
                case TargetType.MinTime:
                    return CreateTargetFunctionMinTime(weight);
                case TargetType.TryVisitEarly:
                    return CreateTargetFunctionTryVisitEarly(weight);
                case TargetType.TryVisitDesired:
                    return CreateTargetFunctionTryVisitDesired(weight);
                default:
                    throw new NotSupportedException($"The type {target} is not supported.");
            }
        }

        private LinearExpr CreateTargetFunctionTryVisitDesired(int weight)
        {
            var sum = new LinearExpr[solverData.NumberOfVisits];
            sum[0] = new LinearExpr();
            for (int visit = 1; visit < solverData.NumberOfVisits; visit++)
            {
                sum[visit] = new LinearExpr();
                for (int day = 0; day < solverData.NumberOfDays; day++)
                {
                    for (int timeslice = 1; timeslice < solverData.SlicesPerDay[day]; timeslice++)
                    {
                        var isDesired = solverData.Input.Visits[day][visit, timeslice] == VisitState.Desired;
                        sum[visit] -= solverData.Variables.Visits[day][visit, timeslice] * Convert.ToInt32(isDesired);
                    }
                }
            }

            return sum.Sum() * weight;
        }

        private LinearExpr CreateTargetFunctionTryVisitEarly(int weight)
        {
            var maxWeight = 0.0;
            var sum = new LinearExpr[solverData.NumberOfDays];
            for (int day = 0; day < solverData.NumberOfDays; day++)
            {
                sum[day] = new LinearExpr();
                for (int timeslice = 1; timeslice < solverData.SlicesPerDay[day]; timeslice++)
                {
                    for (int santa = 0; santa < solverData.NumberOfSantas; santa++)
                    {
                        sum[day] += solverData.Variables.SantaEnRoute[day][santa, timeslice] * timeslice;

                    }
                }
                double slices = solverData.SlicesPerDay[day];

                // gauss sum formula
                maxWeight += (slices * slices + slices) / 2;
            }

            return sum.Sum() * weight;
        }

        private LinearExpr CreateTargetFunctionMinTime(int weight)
        {
            var sum = new LinearExpr[solverData.NumberOfDays];
            for (int day = 0; day < solverData.NumberOfDays; day++)
            {
                sum[day] = new LinearExpr();
                foreach (var v in solverData.Variables.SantaEnRoute[day])
                {
                    sum[day] += v;
                }
            }
            return sum.Sum() * weight;
        }
    }
}
