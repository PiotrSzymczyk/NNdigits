using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Net.Base;
using Net.TransferFunctions;

namespace Net
{
    public class NeuralNetwork
    {
        public IList<Layer> Layers { get; set; }

        public double LearningRate { get; set; }

        public double Momentum { get; set; }

        public Layer InputLayer => Layers.First();

        public Layer OutputLayer => Layers.Last();

        public IList<Layer> HiddenLayers => (IList<Layer>) Layers.Except(new[]
        {
            InputLayer,
            OutputLayer
        });

        public IEnumerable<double> PaternError => OutputLayer.Neurons.Select(neuron => neuron.Error);

        public double TotalNetworkError { get; set; }

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

        protected void LearnPattern(TrainingElement trainingElement)
        {
            IEnumerable<double> input = trainingElement.Input.ToArray();
            this.SetInput(input);
            this.Calculate();
            var patternError = this.GetPatternError(trainingElement.ExpectedOutput);
            this.UpdateNetwork(patternError.ToList());
        }

        private void SetInput(IEnumerable<double> input)
        {
            if (input.Count() != InputLayer.Count)
                throw new Exception("Input vector size does not match network input dimension!");

            using (var inputs = input.GetEnumerator())
            {
                for (int index = 0; index < InputLayer.Count; index++, inputs.MoveNext())
                {
                    InputLayer.Neurons[index].NetInput = inputs.Current;
                }
            }
            
        }

        private void Calculate()
        {
            for (int layer = 1; layer < Layers.Count; layer++)
            {
                foreach (var neuron in Layers[layer].Neurons)
                {
                    neuron.Process();
                }
            }
        }

        private IEnumerable<double> GetPatternError(IList<int> expectedOutput)
        {
            var patternError = new List<double>();
            for (int index = 0; index < OutputLayer.Neurons.Count; index++)
            {
                Base.Neuron neuron = OutputLayer.Neurons[index];
                patternError.Add(expectedOutput[index] - neuron.Output);
            }
            return patternError;
        }

        private void UpdateNetwork(IList<double> patternError)
        {
            this.TotalNetworkError += patternError.Sum();
            this.SetOutputLayerNeuronsErrors(patternError.GetEnumerator());
            this.SetHiddenLayerNeuronsErrors();
            this.UpdateWeights();
        }
        
        private void SetOutputLayerNeuronsErrors(IEnumerator<double> patternError)
        {
            foreach (Base.Neuron neuron in this.OutputLayer.Neurons)
            {
                var outputError = patternError.Current;
                neuron.Error = outputError != 0 ? outputError*GetDerivative(neuron) : 0;
                patternError.MoveNext();
            }				
        }

        private void SetHiddenLayerNeuronsErrors()
        {
            foreach (var layer in HiddenLayers.Reverse())
            {
                foreach (var neuron in layer.Neurons)
                {
                    var deltaSum = neuron.OutConnections.Sum(connection 
                        => connection.Destination.Error * connection.Weight.Value);

                    neuron.Error = GetDerivative(neuron) * deltaSum;
                }
            }
        }

        private void UpdateWeights()
        {
            foreach (var layer in Layers)
            {
                foreach (var neuron in layer.Neurons)
                {
                    UpdateNeuronWeights(neuron);
                }
            }
        }

        private void UpdateNeuronWeights(Base.Neuron neuron)
        {
            foreach (Connection connection in neuron.InputConnections)
            {
                var input = connection.Source.Output;
                if (input != 0)
                {
                    var weight = connection.Weight;

                    var currentWeighValue = weight.Value;
                    var previousWeightValue = weight.PreviousValue;
                    var deltaWeight = this.LearningRate * neuron.Error * input +
                        this.Momentum * (currentWeighValue - previousWeightValue);

                    weight.PreviousValue = currentWeighValue;
                    weight.Value += deltaWeight;
                }
            }
        }

        private double GetDerivative(Base.Neuron neuron)
        {
            var transferFunction = neuron.TransferFunction;
            var netInput = neuron.NetInput;
            return transferFunction.CalculateDerivative(netInput);
        }
    }
}
