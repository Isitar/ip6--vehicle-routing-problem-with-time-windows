﻿using System;
using System.Collections.Generic;
using IRuettae.Core.GeneticAlgorithm.Algorithm.Models;

namespace IRuettae.Core.GeneticAlgorithm.Algorithm
{
    public class MutationOperation
    {
        private readonly double positionMutationProbability;
        private readonly Random random;

        /// <summary>
        /// Factor to be multiplied with population size
        /// to get the stdev for the mutation size
        /// </summary>
        private const double MutationSizeStdevFactor = 1d / 4d;

        public MutationOperation(Random random, double positionMutationProbability)
        {
            this.random = random;
            this.positionMutationProbability = positionMutationProbability;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="population"></param>
        /// <param name="probability">between 0 and 1</param>
        public void Mutate(List<Genotype> population, double probability)
        {
            foreach (var individual in population)
            {
                Mutate(individual, probability);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="individual"></param>
        /// <param name="probability">between 0 and 1</param>
        public void Mutate(Genotype individual, double probability)
        {
            if (random.NextDouble() < probability)
            {
                // no mutation
                return;
            }

            var p = random.NextDouble();
            if (p < positionMutationProbability)
            {
                PositionMutate(individual);
            }
            else
            {
                InversionMutate(individual);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="individual">not null and size > 0</param>
        private void PositionMutate(Genotype individual)
        {
            var mutationSize = GetMutationSize(1, individual.Count * MutationSizeStdevFactor);
            while (mutationSize-- > 0)
            {
                var position1 = random.Next(0, individual.Count);
                var position2 = random.Next(0, individual.Count);

                // swap
                var temp = individual[position1];
                individual[position1] = individual[position2];
                individual[position2] = temp;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="individual">not null > 0</param>
        private void InversionMutate(Genotype individual)
        {
            var count = individual.Count;
            var inversionSize = Math.Min(count, GetMutationSize(2, count * MutationSizeStdevFactor));
            var inversionStart = random.Next(0, count - inversionSize + 1);

            var mutationSubset = individual.GetRange(inversionStart, inversionSize);
            var afterSubset = individual.GetRange(inversionStart + inversionSize, count - (inversionStart + inversionSize));

            // remove everything after inversionStart
            individual.RemoveRange(inversionStart, count - inversionStart);

            // add reverse again
            mutationSubset.Reverse();
            individual.AddRange(mutationSubset);

            // add subset after mutation again
            individual.AddRange(afterSubset);
        }

        /// <summary>
        /// Returns a random, gaussian distributed number which is at least min.
        /// Source: https://stackoverflow.com/questions/218060/random-gaussian-variables
        /// </summary>
        /// <param name="min"></param>
        /// <param name="stdev"></param>
        /// <returns></returns>
        private int GetMutationSize(int min, double stdev)
        {
            var u1 = random.NextDouble();
            double u2;
            do
            {
                u2 = random.NextDouble();
            } while (u2 == 0.0);

            var randomNormal = Math.Cos(2d * Math.PI * u1) * Math.Sqrt(-2d * Math.Log(u2));

            return min + (int)Math.Abs(stdev * randomNormal);
        }
    }
}