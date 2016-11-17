namespace Net.Base
{
    public class Connection
    {

        public Connection(Neuron src, Neuron dest, Weight weight)
        {
            this.Source = src;
            this.Destination = dest;
            this.Weight = weight;
        }

        public Connection(Neuron src, Neuron dest, double min, double max)
            : this(src, dest, new Weight(min, max))
        {
        }

        public Connection(Neuron src, Neuron dest)
            : this(src, dest, new Weight())
        {
        }

        public Weight Weight { get; set; }

        public Neuron Source { get; set; }

        public Neuron Destination { get; set; }
    }
}
