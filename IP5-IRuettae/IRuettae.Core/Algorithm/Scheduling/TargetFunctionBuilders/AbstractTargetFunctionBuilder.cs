﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IRuettae.Core.Algorithm.Scheduling.Detail;
using GLS = Google.OrTools.LinearSolver;

namespace IRuettae.Core.Algorithm.Scheduling.TargetFunctionBuilders
{
    internal abstract class AbstractTargetFunctionBuilder
    {
        public abstract void CreateTargetFunction(SolverData solverData);
    }
}