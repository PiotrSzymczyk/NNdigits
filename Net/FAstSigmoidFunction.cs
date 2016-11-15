﻿using System;

namespace Net
{
    public class FastSigmoidFunction : ITransferFunction
    {
        public double Beta { get; set; }
        public double Calculate(double netInput)
        {
            return Beta*netInput/(1 + Math.Abs(Beta*netInput));
        }

        public double CalculateDerivative(double netInput)
        {
            var abs = Math.Abs(Beta*netInput);
            return Beta/(1 + abs) - abs/Math.Pow(1 + abs, 2);
        }
    }
}
