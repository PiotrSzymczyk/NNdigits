using System.Collections.Generic;

namespace Net
{
    class NeuralNetwork
    {
        public IList<Layer> Layers { get; set; }

        public NeuralNetwork(int numOfInputs, int numOfHiddenLayerNeurons, int numOfOutputs, ITransferFunction transferFun, double minWeight, double maxWeight)
        {
            Layers.Add(new Layer(numOfInputs, transferFun));
            Layers.Add(new Layer(numOfHiddenLayerNeurons, transferFun));
            Layers.Add(new Layer(numOfOutputs, transferFun));

            for (int i = 1; i < Layers.Count; i++)
            {
                Layers[i].ConenctToPreviousLayer(Layers[i-1], minWeight, maxWeight);
            }
        }

        public NeuralNetwork(int numOfInputs, int numOfHiddenLayerNeurons, int numOfOutputs, ITransferFunction transferFun)
            : this(numOfInputs, numOfHiddenLayerNeurons, numOfOutputs, transferFun, -0.5d, 0.5d)
        {
        }
    }
}
