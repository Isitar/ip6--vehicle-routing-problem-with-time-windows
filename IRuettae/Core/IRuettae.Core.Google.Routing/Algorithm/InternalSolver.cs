﻿using System;
using System.Collections.Generic;
using System.Linq;
using Google.OrTools.ConstraintSolver;
using IRuettae.Core.Google.Routing.Models;
using IRuettae.Core.Models;

namespace IRuettae.Core.Google.Routing.Algorithm
{
    internal static class InternalSolver
    {
        // dimensions
        public const String DimensionTime = "time";

        /// <summary>
        /// requires data.SantaIds
        /// requires data.Visits
        /// requires data.HomeIndex
        /// requires data.HomeIndexAdditional
        /// requires data.Unavailable
        /// requires data.Start
        /// requires data.End
        /// </summary>
        public static OptimizationResult Solve(RoutingData data, long timeLimitMilliseconds)
        {
            if (false
                || data.SantaIds == null
                || data.Visits == null
                || data.Unavailable == null
                || data.SantaStartIndex == null
                || data.SantaEndIndex == null
                )
            {
                throw new ArgumentNullException();
            }

            var model =
                new RoutingModel(data.Visits.Length, data.SantaIds.Length,
                                 data.SantaStartIndex, data.SantaEndIndex);

            // setting up dimensions
            var maxTime = GetMaxTime(data);
            var timeCallback = new TimeEvaluator(data, 1);
            model.AddDimension(timeCallback, maxTime, maxTime, false, DimensionTime);

            // dimensions for breaks
            var breakCallbacks = new List<BreakEvaluator>();
            var breakDimensions = new List<string>();
            for (int santa = 0; santa < data.NumberOfSantas; santa++)
            {
                var maxBreaks = GetNumberOfBreaks(data, santa);
                if (maxBreaks == 0)
                {
                    // no breaks
                    continue;
                }

                var evaluator = new BreakEvaluator(data, santa);
                var dimension = GetSantaBreakDimension(santa);
                model.AddDimension(evaluator, 0, maxBreaks, true, dimension);
                breakCallbacks.Add(evaluator);
                breakDimensions.Add(dimension);
            }


            // setting up santas (=vehicles)
            var costCallbacks = new NodeEvaluator2[data.NumberOfSantas];
            for (int santa = 0; santa < data.NumberOfSantas; santa++)
            {
                // must be a new instance per santa
                NodeEvaluator2 costCallback;
                if (data.Input.IsAdditionalSanta(data.SantaIds[santa]))
                {
                    costCallback = new TimeEvaluator(data, data.Cost.CostWorkPerHour + data.Cost.CostAdditionalSantaPerHour);
                }
                else
                {
                    costCallback = new TimeEvaluator(data, data.Cost.CostWorkPerHour);
                }
                costCallbacks[santa] = costCallback;
                model.SetVehicleCost(santa, costCallback);

                // limit time per santa
                var day = data.GetDayFromSanta(santa);
                var start = GetDayStart(data, day);
                var end = GetDayEnd(data, day);
                model.CumulVar(model.End(santa), DimensionTime).SetRange(start, end);
                model.CumulVar(model.Start(santa), DimensionTime).SetRange(start, end);

                // avoid visiting breaks of other santas
                var breakDimension = GetSantaBreakDimension(santa);
                foreach (var dimension in breakDimensions.Except(new[] { breakDimension }))
                {
                    model.CumulVar(model.End(santa), dimension).SetMax(0);
                }
            }

            // setting up visits (=orders)
            for (int i = 0; i < data.NumberOfVisits; ++i)
            {
                model.CumulVar(i, DimensionTime).SetRange(data.OverallStart, data.OverallEnd);
                model.AddDisjunction(new int[] { i }, data.Cost.CostNotVisitedVisit);
            }

            // Solving
            var search_parameters = RoutingModel.DefaultSearchParameters();
            search_parameters.FirstSolutionStrategy =
                FirstSolutionStrategy.Types.Value.Automatic; // maybe try AllUnperformed or PathCheapestArc
            search_parameters.TimeLimitMs = timeLimitMilliseconds;

            var solution = model.SolveWithParameters(search_parameters);

            // protect callbacks from the GC
            GC.KeepAlive(timeCallback);
            foreach (var costCallback in costCallbacks)
            {
                GC.KeepAlive(costCallback);
            }
            foreach (var breakCallback in breakCallbacks)
            {
                GC.KeepAlive(breakCallback);
            }

            // Todo maybe work with GetFixedCostOfVehicle


            return CreateResult(data, model, solution);
        }

        /// <summary>
        /// Returns the total lenght of the time dimension.
        /// </summary>
        /// <returns></returns>
        public static int GetMaxTime(RoutingData data)
        {
            return data.OverallEnd - data.OverallStart + 2 * data.Input.MaxWayDuration();
        }

        /// <summary>
        /// Returns the earliest time for the santas to start.
        /// </summary>
        /// <returns></returns>
        public static int GetDayStart(RoutingData data, int day)
        {
            return data.Input.Days[day].from - data.Input.MaxWayDuration();
        }

        /// <summary>
        /// Returns the lastest time for the santas to return home.
        /// </summary>
        /// <returns></returns>
        public static int GetDayEnd(RoutingData data, int day)
        {
            return data.Input.Days[day].to + data.Input.MaxWayDuration();
        }

        /// <summary>
        /// Returns the total number of breaks of the given santa.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="santa"></param>
        /// <returns></returns>
        private static int GetNumberOfBreaks(RoutingData data, int santa)
        {
            return data.Visits.Count(v => v.IsBreak && v.SantaId == santa);
        }

        /// <summary>
        /// Returns the total number of breaks of the given santa.
        /// </summary>
        /// <param name="santa">index of the santa</param>
        /// <returns></returns>
        private static string GetSantaBreakDimension(int santa)
        {
            return $"Santa{santa}Break";
        }

        /// <summary>
        /// Builds the OptimizationResult.
        /// Note: OptimizationResult.TimeElapsed will not be set.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="model"></param>
        /// <param name="solution"></param>
        /// <returns></returns>
        private static OptimizationResult CreateResult(RoutingData data, RoutingModel model, Assignment solution)
        {
            var ret = new OptimizationResult
            {
                OptimizationInput = data.Input,
                ResultState = GetResultState(model),
                Routes = GetRoutes(data, model, solution),
            };

            return ret;
        }

        /// <summary>
        /// Returns the ResultState matching the given routingModel
        /// </summary>
        /// <param name="routingModel"></param>
        /// <returns></returns>
        private static ResultState GetResultState(RoutingModel routingModel)
        {
            // timeout
            if (routingModel.Status() == RoutingModel.ROUTING_FAIL_TIMEOUT)
            {
                return ResultState.TimeLimitReached;
            }
            // success
            if (routingModel.Status() == RoutingModel.ROUTING_FAIL_TIMEOUT)
            {
                return ResultState.Finished;
            }
            // error, may be one of those:
            // ROUTING_NOT_SOLVED
            // ROUTING_FAIL
            // ROUTING_INVALID
            return ResultState.Error;
        }

        /// <summary>
        /// Returns the Routes from the solution.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="model"></param>
        /// <param name="solution"></param>
        /// <returns></returns>
        private static Route[] GetRoutes(RoutingData data, RoutingModel model, Assignment solution)
        {
            if (solution == null)
            {
                return new Route[0];
            }

            // Routes
            var routes = new List<Route>();
            for (int santa = 0; santa < data.NumberOfSantas; ++santa)
            {
                long visit = model.Start(santa);
                if (model.IsEnd(solution.Value(model.NextVar(visit))))
                {
                    // empty
                }
                else
                {
                    var waypoints = new List<Waypoint>();
                    void AddWaypoint(long v)
                    {
                        var startTime = solution.Value(model.CumulVar(v, DimensionTime));
                        var visitId = (v >= data.Visits.Length || v < 0) ? Constants.VisitIdHome : data.Visits[v].Id;
                        waypoints.Add(new Waypoint
                        {
                            StartTime = (int)startTime,
                            VisitId = visitId,
                        });
                    }

                    for (; !model.IsEnd(visit); visit = solution.Value(model.NextVar(visit)))
                    {
                        AddWaypoint(visit);
                    }

                    // add end
                    {
                        AddWaypoint(visit);
                    }
                    routes.Add(new Route
                    {
                        SantaId = data.SantaIds[santa],
                        Waypoints = waypoints.ToArray(),
                    });
                }
            }

            return routes.ToArray();
        }
    }
}