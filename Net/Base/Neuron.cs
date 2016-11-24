using System.Collections.Generic;
using System.Linq;
using Net.TransferFunctions;

namespace Net.Base
{
    public class Neuron
    {
        public IList<Connection> InputConnections { get; set; } = new List<Connection>();

        public IList<Connection> OutConnections { get; set; } = new List<Connection>();

        public double NetInput { get; set; }

        public double Output { get; set; }

        public double Error { get; set; }

        public ITransferFunction TransferFunction { get; set; }

        public bool HasInputConnections => this.InputConnections.Count > 0;

        public Connection GetConnectionFrom(Neuron fromNeuron)
            => InputConnections.FirstOrDefault(c => c.Source == fromNeuron);

        public IEnumerable<double> WeightsVector => InputConnections.Select(c => c.Weight.Value);

        public Neuron()
            : this(new SigmoidFunction())
        {
        }
        
        public Neuron(ITransferFunction transferFunction)
        {
            this.TransferFunction = transferFunction;
        }
        
        public void Process()
        {
            if (this.HasInputConnections)
            {
                this.NetInput = InputConnections.Sum(connection => connection.Weight.Value * connection.Source.Output );
            }

            this.Output = this.TransferFunction.Calculate(this.NetInput);
        }
        
        public void Reset()
        {
            this.NetInput = 0d;
            this.Output = 0d;
        }

        public void AddInputConnection(Connection connection)
        {
            this.InputConnections.Add(connection);
            connection.Source.AddOutputConnection(connection);
        }

        public void AddInputConnection(Neuron fromNeuron, double min, double max)
        {
            Connection connection = new Connection(fromNeuron, this, min, max);
            this.AddInputConnection(connection);
        }
        
        protected void AddOutputConnection(Connection connection)
        {
            this.OutConnections.Add(connection);
        }

        public void RandomizeInputWeights(double min, double max)
        {
            foreach (Connection connection in this.InputConnections)
            {
                connection.Weight.Randomize(min, max);
            }
        }

        public void DeleteConnectionFrom(Neuron prevNeuron)
        {
            var connection = InputConnections.FirstOrDefault(n => n.Source == prevNeuron);
            connection?.Source.OutConnections.Remove(connection);
            connection?.Destination.InputConnections.Remove(connection);
        }

        public override string ToString()
        {
            return $"N(i:{NetInput}; o:{Output})";
        }
    }
}
