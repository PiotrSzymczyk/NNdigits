using System.Collections.Generic;

namespace Net
{
    public class Layer
    {
        public IList<Neuron> Neurons { get; set; }

        public int Count => Neurons.Count;

        public Layer(int numOfNeurons, ITransferFunction transferFun)
        {
            Neurons = new List<Neuron>();
            for (int i = 0; i < numOfNeurons; i++)
            {
                Neurons.Add(new Neuron(transferFun));
            }
        }

        public Layer(int numOfNeurons)
            : this(numOfNeurons, new SigmoidFunction())
        {
        }

        public void ConenctToPreviousLayer(Layer previous, double minWeight, double maxWeight)
        {
            foreach (var prevNeuron in previous.Neurons)
            {
                foreach (var neuron in Neurons)
                {
                    neuron.DeleteConnectionFrom(prevNeuron);
                    neuron.AddInputConnection(prevNeuron, minWeight, maxWeight);
                }
            }
        }
    }
}
