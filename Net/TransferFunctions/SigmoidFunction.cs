using System;

namespace Net.TransferFunctions
{
    public class SigmoidFunction : ITransferFunction
    {
        public double Beta { get; set; }

        public double Calculate(double netInput)
        {
            return 1/(1 + Math.Exp(-Beta*netInput));
        }

        public double CalculateDerivative(double netInput)
        {
            var fx = Calculate(netInput);
            return Beta*(1 - fx)*fx;
        }
    }
}
