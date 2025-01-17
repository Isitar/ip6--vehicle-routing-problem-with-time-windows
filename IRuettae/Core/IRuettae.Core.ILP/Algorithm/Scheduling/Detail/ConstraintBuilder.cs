﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Google.OrTools.LinearSolver;

namespace IRuettae.Core.ILP.Algorithm.Scheduling.Detail
{
    internal class ConstraintBuilder
    {
        private SolverData solverData;

        public ConstraintBuilder(SolverData solverData)
        {
            this.solverData = solverData;
        }

        public void CreateConstraints()
        {
            // variables
            CreateVisitsConstraint();
            CreateSantaVisitsConstraint();
            CreateUsesSantaConstraint();
            
            // visit
            CreateVisitAvailableConstraint();
            CreateVisitOverallLengthConstraint();
            CreateSingleVisitConstraint();
            CreateOnlyOneSantaPerVisitConstraint();

            // santa
            CreateSantaAvailableConstraint();

            // waytimes
            CreateSantaNeedTimeToFirstVisitConstraint();
            CreateSantaNeedsTimeToGetHomeConstraint();
            CreateSantaNeedTimeBetweenVisitsConstraint();

            CreateSantaEnRouteConstraint();

            CreatePerformanceConstraints();
           
        }

        /// <summary>
        /// Create all constraints that should benefit the performance
        /// </summary>
        private void CreatePerformanceConstraints()
        {
            CreateOrderSantaConstraint();
            CreateSanta0VisitsVisit1Constraint();
        }

        /// <summary>
        /// Visit 1 should be visited by Santa 0
        /// Visit 2 should be visited by Santa 0 or 1
        /// Visit 3 should be visited by Santa 0, 1 or 2
        /// and so on
        /// </summary>
        private void CreateSanta0VisitsVisit1Constraint()
        {
            var visitOffset = 1;
            var numberOfVisits = Math.Min(solverData.NumberOfSantas, solverData.NumberOfVisits - 1);
            for (int visit = 0; visit < numberOfVisits; visit++)
            {
                // 1 == Z1 + Z2 + ...
                var sum = new LinearExpr();
                for (int santa = 0; santa <= visit; santa++)
                {
                    sum += solverData.Variables.SantaVisits[santa, visitOffset + visit];
                }
                solverData.Solver.Add(sum == 1);
            }
        }

        /// <summary>
        /// Santa N can only be used if Santa N-1 is used
        /// </summary>
        private void CreateOrderSantaConstraint()
        {
            for (int day = 0; day < solverData.NumberOfDays; day++)
            {
                for (int santa = 1; santa < solverData.NumberOfSantas; santa++)
                {
                    solverData.Solver.Add(solverData.Variables.UsesSanta[day, santa] <= solverData.Variables.UsesSanta[day, santa - 1]);
                }
            }
        }

        /// <summary>
        /// A santa is en route, if he is visiting someone or walking around
        /// </summary>
        private void CreateSantaEnRouteConstraint()
        {
            // santa is walking between visits
            // means, santa is walking if there is at least one visit before and at least one after
            for (int day = 0; day < solverData.NumberOfDays; day++)
            {
                for (int santa = 0; santa < solverData.NumberOfSantas; santa++)
                {
                    var slicesPerDay = solverData.SlicesPerDay[day];
                    for (int timeslice = 0; timeslice < slicesPerDay; timeslice++)
                    {

                        var visitBefore = CreateVisitBefore(day, santa, 0, timeslice);
                        var visitAfter = CreateVisitBefore(day, santa, timeslice + 1, slicesPerDay);

                        var walking = solverData.Variables.SantaEnRoute[day][santa, timeslice];
                        // Z + 1 >= Z1 + Z2
                        solverData.Solver.Add(walking + 1 >= visitBefore + visitAfter);
                    }
                }
            }

            // set all fields in SantaEnRoute, when santa visits visit
            // see GetSumOfEnRoute for further explanation
            for (int day = 0; day < solverData.NumberOfDays; day++)
            {
                for (int visit = 1; visit < solverData.NumberOfVisits; visit++)
                {
                    for (int santa = 0; santa < solverData.NumberOfSantas; santa++)
                    {
                        for (int timeslice = 0; timeslice < solverData.SlicesPerDay[day]; timeslice++)
                        {
                            var isVisiting = solverData.Variables.VisitsPerSanta[day][santa][visit, timeslice];
                            var (santaEnRoute, numberOfSummands) = GetSumOfEnRoute(day, santa, visit, timeslice);
                            solverData.Solver.Add(santaEnRoute >= numberOfSummands * isVisiting);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// The the sum of all fields in SantaEnRoute that must be set, if santa visits the given visit
        /// A field in SantaEnRoute must be set if:
        /// - Santa is visiting visit
        /// - Santa is busy walking from home to visit
        /// - Santa is busy walking from visit to home
        /// </summary>
        /// <param name="day"></param>
        /// <param name="santa"></param>
        /// <param name="visit"></param>
        /// <param name="timesliceVisit"></param>
        /// <returns>sum of all fields that must be set and the number of fields</returns>
        private (LinearExpr, int) GetSumOfEnRoute(int day, int santa, int visit, int timesliceVisit)
        {
            var sum = new LinearExpr();
            var StartEndPoint = solverData.StartEndPoint;
            var timesliceFrom = Math.Max(0, timesliceVisit - solverData.Input.Distances[StartEndPoint, visit]);
            var timesliceTo = Math.Min(solverData.SlicesPerDay[day], timesliceVisit + solverData.Input.Distances[visit, StartEndPoint] + 1);
            for (int timeslice = timesliceFrom; timeslice < timesliceTo; timeslice++)
            {
                sum += solverData.Variables.SantaEnRoute[day][santa, timeslice];
            }
            return (sum, timesliceTo - timesliceFrom);
        }

        /// <summary>
        /// Create a variable, which has the value true, if there is at least one visit in the given range
        /// </summary>
        /// <param name="day"></param>
        /// <param name="santa"></param>
        /// <param name="timesliceFrom">inclusive</param>
        /// <param name="timesliceTo">exclusive</param>
        /// <returns></returns>
        private Variable CreateVisitBefore(int day, int santa, int timesliceFrom, int timesliceTo)
        {
            var hasVisit = solverData.Solver.MakeBoolVar(string.Empty);
            var sum = new LinearExpr();
            for (int timeslice = timesliceFrom; timeslice < timesliceTo; timeslice++)
            {
                var v = solverData.Variables.SantaEnRoute[day][santa, timeslice];
                sum += v;
                // Z >= Z1
                solverData.Solver.Add(hasVisit >= v);
            }
            // Z <= Z1 + Z2 + ...
            solverData.Solver.Add(hasVisit <= sum);
            return hasVisit;
        }

        /// <summary>
        /// Santa is beeing used if he visits anybody on that day
        /// </summary>
        private void CreateUsesSantaConstraint()
        {
            for (int day = 0; day < solverData.NumberOfDays; day++)
            {
                for (int santa = 0; santa < solverData.NumberOfSantas; santa++)
                {
                    var isUsed = solverData.Variables.UsesSanta[day, santa];
                    var sum = new LinearExpr();
                    for (int visit = 1; visit < solverData.NumberOfVisits; visit++)
                    {
                        for (int timeslice = 0; timeslice < solverData.SlicesPerDay[day]; timeslice++)
                        {
                            var current = solverData.Variables.VisitsPerSanta[day][santa][visit, timeslice];
                            sum += current;
                            // Z >= Z1
                            solverData.Solver.Add(isUsed >= current);
                        }
                    }
                    // Z <= Z1 + Z2 + ...
                    solverData.Solver.Add(isUsed <= sum);
                }
            }
        }

        /// <summary>
        /// Visit is beeing visited if it is visited by a santa
        /// </summary>
        private void CreateVisitsConstraint()
        {
            for (int visit = 1; visit < solverData.NumberOfVisits; visit++)
            {
                for (int day = 0; day < solverData.NumberOfDays; day++)
                {
                    for (int timeslice = 0; timeslice < solverData.SlicesPerDay[day]; timeslice++)
                    {
                        // Z = Z1 + Z2 + ...
                        var expr = new LinearExpr();
                        for (int santa = 0; santa < solverData.NumberOfSantas; santa++)
                        {
                            expr += solverData.Variables.VisitsPerSanta[day][santa][visit, timeslice];
                        }
                        solverData.Solver.Add(solverData.Variables.Visits[day][visit, timeslice] == expr);
                    }
                }
            }
        }

        /// <summary>
        /// A visit has to be in one piece
        /// </summary>
        private void CreateSingleVisitConstraint()
        {

            // Todo: MEYERJ das ganze Modell eventuell mehr von VisitStart abhängig machen

            for (int visit = 1; visit < solverData.NumberOfVisits; visit++)
            {
                // Z = Z1 + Z2 + ...
                // one possible start must be true
                var sum = new LinearExpr();

                for (int day = 0; day < solverData.NumberOfDays; day++)
                {
                    for (int timeslice = 0; timeslice < solverData.SlicesPerDay[day] - 1; timeslice++)
                    {
                        var start = solverData.Variables.VisitStart[day][visit, timeslice + 1];
                        var t0 = solverData.Variables.Visits[day][visit, timeslice];
                        var t1 = solverData.Variables.Visits[day][visit, timeslice + 1];


                        // special case if the visit starts at timeslice 0
                        if (timeslice == 0)
                        {
                            var start0 = solverData.Variables.VisitStart[day][visit, timeslice];
                            solverData.Solver.Add(start0 == t0);
                            sum += start0;
                        }

                        // Z <= !Z1
                        solverData.Solver.Add(start <= 1 - t0);
                        // Z <= Z2
                        solverData.Solver.Add(start <= t1);
                        // Z + 1>= !Z1 + Z2
                        solverData.Solver.Add(start + 1 >= 1 - t0 + t1);


                        sum += start;
                    }
                }

                solverData.Solver.Add(1 == sum);
            }
        }

        /// <summary>
        /// Santas need time to go to the first Visit
        /// </summary>
        private void CreateSantaNeedTimeToFirstVisitConstraint()
        {
            for (int visit = 1; visit < solverData.NumberOfVisits; visit++)
            {
                var distance = solverData.Input.Distances[solverData.StartEndPoint, visit];
                for (int day = 0; day < solverData.NumberOfDays; day++)
                {
                    // Z1 + Z2 + ... == 0
                    var sum = new LinearExpr();
                    for (int timeslice = 0; timeslice < Math.Min(distance, solverData.SlicesPerDay[day]); timeslice++)
                    {
                        sum += solverData.Variables.VisitStart[day][visit, timeslice];
                    }
                    solverData.Solver.Add(sum == 0);
                }
            }
        }

        /// <summary>
        /// Santas need to go back home from their last Visit
        /// </summary>
        private void CreateSantaNeedsTimeToGetHomeConstraint()
        {
            for (int visit = 1; visit < solverData.NumberOfVisits; visit++)
            {
                var distance = solverData.Input.Distances[visit, solverData.StartEndPoint];
                var duration = solverData.Input.VisitsDuration[visit];
                for (int day = 0; day < solverData.NumberOfDays; day++)
                {
                    var start = Math.Max(0, solverData.SlicesPerDay[day] - distance - duration + 1);
                    // Z1 + Z2 + ... == 0
                    var sum = new LinearExpr();
                    for (int timeslice = start; timeslice < solverData.SlicesPerDay[day]; timeslice++)
                    {
                        sum += solverData.Variables.VisitStart[day][visit, timeslice];
                    }
                    solverData.Solver.Add(sum == 0);
                }
            }
        }

        /// <summary>
        /// Santas are not able to beam and therefore,
        /// it needs a certain time to get from one visit to another
        /// except if distance is 0
        /// </summary>
        private void CreateSantaNeedTimeBetweenVisitsConstraint()
        {
#if DEBUG
            var constraintCounter = 0;
#endif
            for (int day = 0; day < solverData.NumberOfDays; day++)
            {
                int slicesPerDay = solverData.SlicesPerDay[day];
                for (int santa = 0; santa < solverData.NumberOfSantas; santa++)
                {
                    for (int visit = 1; visit < solverData.NumberOfVisits; visit++)
                    {
                        for (int timeslice = 0; timeslice < slicesPerDay; timeslice++)
                        {
                            int numberOfBs = 0;
                            var A = solverData.Variables.VisitsPerSanta[day][santa][visit, timeslice];
                            var B = new LinearExpr();
                            for (int destination = 1; destination < solverData.NumberOfVisits; destination++)
                            {
                                var distance = solverData.Input.Distances[visit, destination];
                                // don't add unnecessary constraints
                                if (distance <= 0) continue;

                                // 1 because same timeslot is handled by another constraint
                                for (int distCounter = 1; distCounter <= Math.Min(distance, slicesPerDay - timeslice - 1); distCounter++)
                                {
                                    B += solverData.Variables.VisitsPerSanta[day][santa][destination,
                                        timeslice + distCounter];
                                    numberOfBs++;
                                }

                                // A <= 1 - B would be easy but B can be greater than 0 and A has to be >= 0
                                // so we multiply A by numberOfBs, possible values are 0 (if A == 0) or numberOfBs (if A == 1)
                                // if B == 0, A can be 1 (numberOfBs <= numberOfBs), else A has to be 0 (numberOfBs - (at least 1)) is smaller than numberOfBs



                            }
                            solverData.Solver.Add(numberOfBs * A <= numberOfBs - B);

#if DEBUG
                            constraintCounter++;
#endif
                        }
                    }

                }
            }
#if DEBUG
            Debug.WriteLine($"CreateSantaNeedTimeBetweenVisitsConstraint - added {constraintCounter} constraints");
#endif
        }

        /// <summary>
        /// Visit is only available, when the inputs says so
        /// </summary>
        private void CreateVisitAvailableConstraint()
        {
            for (int visit = 1; visit < solverData.NumberOfVisits; visit++)
            {
                for (int day = 0; day < solverData.NumberOfDays; day++)
                {
                    for (int timeslice = 0; timeslice < solverData.SlicesPerDay[day]; timeslice++)
                    {
                        var available = new LinearExpr() + Convert.ToInt32(solverData.Input.Visits[day][visit, timeslice].IsAvailable());
                        // availble >= Z1 + Z2 + ...
                        var sum = new LinearExpr();
                        for (int santa = 0; santa < solverData.NumberOfSantas; santa++)
                        {
                            sum += solverData.Variables.VisitsPerSanta[day][santa][visit, timeslice];
                        }
                        solverData.Solver.Add(available >= sum);
                    }
                }
            }
        }

        /// <summary>
        /// Santa is only available, when the inputs says so
        /// Futhermore, Santa can only be used mostly once per timeslice
        /// </summary>
        private void CreateSantaAvailableConstraint()
        {
            for (int santa = 0; santa < solverData.NumberOfSantas; santa++)
            {
                for (int day = 0; day < solverData.NumberOfDays; day++)
                {
                    for (int timeslice = 0; timeslice < solverData.SlicesPerDay[day]; timeslice++)
                    {
                        var sum = new LinearExpr();
                        for (int visit = 1; visit < solverData.NumberOfVisits; visit++)
                        {
                            sum += solverData.Variables.VisitsPerSanta[day][santa][visit, timeslice];
                        }

                        // (int)available >= Z1 + Z2 + ...
                        var available = Convert.ToInt32(solverData.Input.Santas[day][santa, timeslice]);
                        solverData.Solver.Add(sum <= available);
                    }
                }
            }
        }

        /// <summary>
        /// Each Visit is visited by exactly one Santa
        /// </summary>
        private void CreateOnlyOneSantaPerVisitConstraint()
        {
            for (int visit = 1; visit < solverData.NumberOfVisits; visit++)
            {
                // 1 = Z1 + Z2 + ...
                var sum = new LinearExpr();
                for (int santa = 0; santa < solverData.NumberOfSantas; santa++)
                {
                    sum += solverData.Variables.SantaVisits[santa, visit];
                }
                solverData.Solver.Add(1 == sum);
            }
        }

        /// <summary>
        /// Each Visit is visited by a Santa, if Santa is there at at least one visit-timeslice
        /// </summary>
        private void CreateSantaVisitsConstraint()
        {
            for (int visit = 1; visit < solverData.NumberOfVisits; visit++)
            {
                for (int santa = 0; santa < solverData.NumberOfSantas; santa++)
                {
                    var santaVisits = solverData.Variables.SantaVisits[santa, visit];
                    // Z <= Z1 + Z2 + ...
                    var sumOfSlices = new LinearExpr();
                    for (int day = 0; day < solverData.NumberOfDays; day++)
                    {
                        for (int timeslice = 0; timeslice < solverData.SlicesPerDay[day]; timeslice++)
                        {
                            var slice = solverData.Variables.VisitsPerSanta[day][santa][visit, timeslice];
                            sumOfSlices += slice;

                            // Z >= Z1
                            solverData.Solver.Add(santaVisits >= slice);
                        }
                    }
                    solverData.Solver.Add(santaVisits <= sumOfSlices);
                }
            }
        }

        /// <summary>
        /// Each visit must be overall visited the right number of timeslices
        /// </summary>
        private void CreateVisitOverallLengthConstraint()
        {
            for (int visit = 1; visit < solverData.NumberOfVisits; visit++)
            {
                // X = Z1 + Z2 + ...
                var sum = new LinearExpr();
                for (int day = 0; day < solverData.NumberOfDays; day++)
                {
                    for (int santa = 0; santa < solverData.NumberOfSantas; santa++)
                    {
                        for (int timeslice = 0; timeslice < solverData.SlicesPerDay[day]; timeslice++)
                        {
                            sum += solverData.Variables.VisitsPerSanta[day][santa][visit, timeslice];
                        }
                    }
                }
                solverData.Solver.Add(solverData.Input.VisitsDuration[visit] == sum);
            }
        }
    }
}
