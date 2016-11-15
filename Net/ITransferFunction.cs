﻿namespace Net
{
    public interface ITransferFunction
    {
        double Beta { get; set; }

        double Calculate(double netInput);

        double CalculateDerivative(double netInput);
    }
}