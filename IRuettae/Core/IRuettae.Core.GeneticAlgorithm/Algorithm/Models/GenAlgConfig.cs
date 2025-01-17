﻿using System;
using System.Linq;
using IRuettae.Core.Models;

namespace IRuettae.Core.GeneticAlgorithm.Algorithm.Models
{
    public class GenAlgConfig : ISolverConfig
    {
        public int MaxNumberOfSantas { get; set; }
        public long MaxNumberOfGenerations { get; set; } = long.MaxValue;
        public int PopulationSize { get; set; } = 0;

        public double ElitismPercentage { get; } = 0.357;
        public double DirectMutationPercentage { get; } = 0.378;
        public double RandomPercentage { get; } = 0.0;

        public double OrderBasedCrossoverProbability { get; } = 0.884;
        public double MutationProbability { get; } = 0.0;
        public double PositionMutationProbability { get; } = 0.886;

        public GenAlgConfig(OptimizationInput input, int maxNumberOfAdditionalSantas = 0)
        {
            if (maxNumberOfAdditionalSantas < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxNumberOfAdditionalSantas), maxNumberOfAdditionalSantas, "must not be negative");
            }
            PopulationSize = CalculatePopulationSize(input);
            MaxNumberOfSantas = input.Santas.Length + maxNumberOfAdditionalSantas;
        }

        /// <summary>
        /// Only use this, if you really need to.
        /// </summary>
        /// <param name="maxNumberOfSantas"></param>
        /// <param name="maxNumberOfGenerations"></param>
        /// <param name="populationSize"></param>
        public GenAlgConfig(OptimizationInput input, int maxNumberOfAdditionalSantas, int maxNumberOfGenerations, int populationSize)
        {
            MaxNumberOfSantas = input.Santas.Length + maxNumberOfAdditionalSantas;
            MaxNumberOfGenerations = maxNumberOfGenerations;
            PopulationSize = populationSize;
        }

        /// <summary>
        /// Should only be used by unit tests.
        /// </summary>
        /// <param name="maxNumberOfSantas"></param>
        /// <param name="maxNumberOfGenerations"></param>
        /// <param name="populationSize"></param>
        /// <param name="elitismPercentage"></param>
        /// <param name="directMutationPercentage"></param>
        /// <param name="randomPercentage"></param>
        /// <param name="orderBasedCrossoverProbability"></param>
        /// <param name="mutationProbability"></param>
        /// <param name="positionMutationProbability"></param>
        public GenAlgConfig(int maxNumberOfSantas, long maxNumberOfGenerations, int populationSize, double elitismPercentage, double directMutationPercentage, double randomPercentage, double orderBasedCrossoverProbability, double mutationProbability, double positionMutationProbability)
        {
            MaxNumberOfSantas = maxNumberOfSantas;
            MaxNumberOfGenerations = maxNumberOfGenerations;
            PopulationSize = populationSize;
            ElitismPercentage = elitismPercentage;
            DirectMutationPercentage = directMutationPercentage;
            RandomPercentage = randomPercentage;
            OrderBasedCrossoverProbability = orderBasedCrossoverProbability;
            MutationProbability = mutationProbability;
            PositionMutationProbability = positionMutationProbability;
        }

        private static int CalculatePopulationSize(OptimizationInput input)
        {
            const int sizeDefault = 262144;
            const int sizeBigger = 16;

            // number of visits
            var x = input.Visits.Count(v => !v.IsBreak) + input.Visits.Count(v => v.IsBreak) * input.Days.Length;
            // PopulationSize
            double y;

            // x -> y
            // 10 -> 262144
            // 20 -> 262144
            // 31 -> 262144
            // 34 -> 262144
            // 50 -> 131072
            // 100 -> 16384
            // 200 -> 16
            // 1000 -> 16

            if (x <= 34) // [-inf,34]
            {
                y = sizeDefault;
            }
            else if (x > 34 && x <= 50) // (34,50]
            {
                // linear interpolation
                // generated with https://mycurvefit.com/
                // linear fit method: linear regression
                // y = -8192*x + 540672
                y = -8192 * x + 540672;
            }
            else if (x > 50 && x < 200) // (50,200)
            {
                // non-linear approximation
                // generated with https://mycurvefit.com/
                // non-linear fit method: exponential basic
                // y = -250.92 + 1036717 * e ^ (-0.0413231 * x)
                y = Math.Round(-250.92 + 1036717 * Math.Pow(Math.E, (-0.0413231 * x)));
            }
            else // [200,inf]
            {
                y = sizeBigger;
            }

            return (int)y;
        }

        /// <summary>
        /// Returns if this is a valid configuration.
        /// </summary>
        /// <returns></returns>
        public bool IsValid()
        {
            if (MaxNumberOfSantas < 1)
            {
                return false;
            }
            if (MaxNumberOfGenerations < 0)
            {
                return false;
            }
            // one individual for elitism plus one "normal" individual
            if (PopulationSize < 2)
            {
                return false;
            }

            // check percentages
            if (ElitismPercentage <= 0 || ElitismPercentage >= 1)
            {
                return false;
            }
            if (DirectMutationPercentage < 0 || DirectMutationPercentage > 1)
            {
                return false;
            }
            if (RandomPercentage < 0 || RandomPercentage > 1)
            {
                return false;
            }
            if (RandomPercentage < 0 || RandomPercentage > 1)
            {
                return false;
            }
            if (MutationProbability < 0 || MutationProbability > 1)
            {
                return false;
            }
            if (OrderBasedCrossoverProbability < 0 || OrderBasedCrossoverProbability > 1)
            {
                return false;
            }
            if (PositionMutationProbability < 0 || PositionMutationProbability > 1)
            {
                return false;
            }

            // make sure PopulationSize is stable
            var size = (int)Math.Max(1, ElitismPercentage * PopulationSize) + (int)(DirectMutationPercentage * PopulationSize) + (int)(RandomPercentage * PopulationSize);
            if (size < 0 || size > PopulationSize)
            {
                return false;
            }

            return true;
        }

        public override string ToString() => string.Join(Environment.NewLine, GetType().GetProperties().Select(p => $"{p.Name}: {(p.GetIndexParameters().Length > 0 ? "Indexed Property cannot be used" : p.GetValue(this, null))}"));
    }
}
