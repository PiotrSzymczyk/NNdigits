using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
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

        public IList<Layer> HiddenLayers => Layers.Except(new[]
        {
            InputLayer,
            OutputLayer
        }).ToList();

        public IEnumerable<double> PaternError => OutputLayer.Neurons.Select(neuron => neuron.Error);

        public double TotalNetworkError { get; set; }

        public int Output
            => OutputLayer.Neurons.IndexOf(
                OutputLayer.Neurons.First(neuron => neuron.Output == OutputLayer.Neurons.Max(n => n.Output)));

        public NeuralNetwork(int numOfInputs, int numOfHiddenLayerNeurons, int numOfOutputs, ITransferFunction transferFun, double minWeight, double maxWeight)
        {
            Layers = new List<Layer>();
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

        public void Process(IList<byte> inputs)
        {
            this.SetInput(inputs);
            this.Process();
        }

        public double Validate(IList<TrainingElement> testSet)
        {
            double hits = 0;
            foreach (var test in testSet)
            {
                this.SetInput(test.Input);
                this.Process();
                if (Output == test.ExpectedOutput.IndexOf(1)) hits++;
            }
            return hits/testSet.Count;
        }

        public void Learn(IList<TrainingElement> trainingSet)
        {
            for (int i = 0; i < 1000; i++)
            {
                DoLearningEpoch(trainingSet);
            }
        }

        public void DoLearningEpoch(IList<TrainingElement> trainingSet)
        {
            this.TotalNetworkError = 0d;

            var iterator = trainingSet.GetEnumerator();
            while (iterator.MoveNext())
            {
                var trainingElement = iterator.Current;
                this.LearnPattern(trainingElement);
            }
        }

        public void LearnPattern(TrainingElement trainingElement)
        {
            this.SetInput(trainingElement.Input);
            this.Process();
            this.UpdateNetwork(this.GetPatternError(trainingElement.ExpectedOutput));
        }

        private void SetInput(IEnumerable<byte> input)
        {
            if (input.Count() != InputLayer.Count)
                throw new Exception("Input vector size does not match network input dimension!");

            using (var inputs = input.GetEnumerator())
            {
                for (int index = 0; index < InputLayer.Count; index++)
                {
                    inputs.MoveNext();
                    InputLayer.Neurons[index].Output = inputs.Current;
                }
            }
            
        }

        private void Process()
        {
            for (int layer = 1; layer < Layers.Count; layer++)
            {
                foreach (var neuron in Layers[layer].Neurons)
                {
                    neuron.Process();
                }
            }
        }

        private IList<double> GetPatternError(IList<int> expectedOutput)
        {
            var patternError = new List<double>();
            for (int index = 0; index < OutputLayer.Neurons.Count; index++)
            {
                Neuron neuron = OutputLayer.Neurons[index];
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
            foreach (Neuron neuron in this.OutputLayer.Neurons)
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

        private void UpdateNeuronWeights(Neuron neuron)
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

        private double GetDerivative(Neuron neuron)
        {
            var transferFunction = neuron.TransferFunction;
            var netInput = neuron.NetInput;
            return transferFunction.CalculateDerivative(netInput);
        }

        public override string ToString()
        {
            string net = "";
            foreach (var layer in Layers)
            {
                net += "Layer\n";
                foreach (var neuron in layer.Neurons)
                {
                    net += $"\t{neuron}\n";
                }
            }
            return net;
        }
    }
}
