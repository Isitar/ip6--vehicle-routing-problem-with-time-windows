﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using IRuettae.Core.Google.Routing.Algorithm;
using IRuettae.Core.Google.Routing.Algorithm.TimeWindow;
using IRuettae.Core.Google.Routing.Models;
using IRuettae.Core.Models;

namespace IRuettae.Core.Google.Routing
{
    public class GoogleRoutingSolver : ISolver
    {
        private readonly OptimizationInput input;
        private readonly GoogleRoutingConfig config;

        public GoogleRoutingSolver(OptimizationInput input, GoogleRoutingConfig config)
        {
            if (config.MaxNumberOfAdditionalSantas < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(config), config, $"{nameof(config.MaxNumberOfAdditionalSantas)} must not be negative");
            }

            this.input = input;
            this.config = config;
        }

        public OptimizationResult Solve(long timeLimitMilliseconds, EventHandler<ProgressReport> progress, EventHandler<string> consoleProgress)
        {
            // convenience
            void LogPercentage(double percentage)
            {
                progress?.Invoke(this, new ProgressReport(percentage));
            }
            void LogMessage(string message)
            {
                consoleProgress?.Invoke(this, message);
            }

            var sw = Stopwatch.StartNew();

            LogMessage("Solving started.");
            LogPercentage(0.0);

            // Create input data for internal solver.
            // Use mostly one per core.
            var runs = GetStrategies(Environment.ProcessorCount).Select(s => (data: Converter.Convert(input, config.MaxNumberOfAdditionalSantas), strategy: s)).ToArray();

            // adapt timelimit so that no overdraw is made
            if (runs.Length > Environment.ProcessorCount)
            {
                timeLimitMilliseconds /= (long)Math.Ceiling((double)runs.Length / Environment.ProcessorCount);
            }

            LogMessage("Conversion of input finished.");
            LogPercentage(0.01);

            // solve
            var results = runs.AsParallel().Select(r => InternalSolver.Solve(r.data, timeLimitMilliseconds, r.strategy)).ToArray();

            // print strategies / results
            LogPercentage(0.99);
            LogMessage("Runs finished. Printing result:");
            for (int i = 0; i < runs.Length; i++)
            {
                LogMessage($"Strategy: {runs[i].strategy.GetType().Name} Cost: {results[i].Cost()}");
            }

            // get best result
            var bestResult = results.OrderBy(r => r.Cost()).First();

            bestResult.TimeElapsed = (int)sw.Elapsed.TotalSeconds;
            LogMessage("Solver finished.");
            LogPercentage(1);

            return bestResult;
        }

        /// <summary>
        /// Returns the time window strategies that should be used.
        /// </summary>
        /// <param name="number">number of strategies</param>
        /// <returns></returns>
        private List<ITimeWindowStrategy> GetStrategies(int number)
        {
            List<ITimeWindowStrategy> strategies;
            switch (config.Mode)
            {
                case SolvingMode.Default:
                    strategies = GetStrategiesDefault();
                    break;
                case SolvingMode.Fast:
                    strategies = GetStrategiesFast();
                    break;
                case SolvingMode.All:
                    strategies = GetStrategiesDefault();
                    break;
                default:
                    throw new NotImplementedException("unknown SolvingMode");
            }

            // limit number of strategies
            number = Math.Max(1, Math.Min(strategies.Count, number));

            if (config.Mode == SolvingMode.All)
            {
                // remove limit
                number = strategies.Count;
            }

            return strategies.Take(number).ToList();
        }

        private List<ITimeWindowStrategy> GetStrategiesDefault()
        {
            return new List<ITimeWindowStrategy>()
            {
                new DesiredSoftStrategy(),
                new DesiredHardStrategy(),
                new UnavailableOnlyStrategy(),
                new NoneStrategy(),
            };
        }

        private List<ITimeWindowStrategy> GetStrategiesFast()
        {
            return new List<ITimeWindowStrategy>()
            {
                new NoneStrategy(),
                new UnavailableOnlyStrategy(),
                new DesiredHardStrategy(),
                new DesiredSoftStrategy(),
            };
        }
    }
}
