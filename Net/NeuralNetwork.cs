using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Controls;
using Net.Base;
using Net.TransferFunctions;

namespace Net
{
    public class NeuralNetwork
    {
        private double totalNetworkError;
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

        public double CurrNetworkError => Layers.SelectMany(l => l.Neurons).Sum(neuron => Math.Pow(neuron.Error,2));

        public virtual double TotalNetworkError
        {
            get; set;  }

        public int MaxNumberOfEpoch { get; set; } = 5000;

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
            var rand = new Random();
            for (int i = 0; i < MaxNumberOfEpoch; i++)
            {
                DoLearningEpoch(trainingSet.OrderBy(val => rand.Next()).ToList());
            }
        }

        public void DoLearningEpoch(IList<TrainingElement> trainingSet)
        {
            this.TotalNetworkError = 0d;

            using (var iterator = trainingSet.GetEnumerator())
            {
                while (iterator.MoveNext())
                {
                    var trainingElement = iterator.Current;
                    this.LearnPattern(trainingElement);
                }
            }
        }

        public void LearnPattern(TrainingElement trainingElement)
        {
            this.SetInput(trainingElement.Input);
            this.Process();
            this.UpdateNetwork(trainingElement.ExpectedOutput);
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

        private void UpdateNetwork(IList<int> expectedResult)
        {
            this.SetOutputLayerNeuronsErrors(expectedResult);
            this.SetHiddenLayerNeuronsErrors();
            this.UpdateWeights();
            this.TotalNetworkError += CurrNetworkError;
        }
        
        private void SetOutputLayerNeuronsErrors(IList<int> expectedResult)
        {
            for (int i = 0; i < expectedResult.Count; i++)
            {
                OutputLayer.Neurons[i].Error = (expectedResult[i] - OutputLayer.Neurons[i].Output)*
                                               GetDerivative(OutputLayer.Neurons[i]);
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
            foreach (var layer in Layers.Reverse())
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
                var weight = connection.Weight;

                var currentWeighValue = weight.Value;
                var previousWeightValue = weight.PreviousValue;
                var deltaWeight = this.LearningRate*neuron.Error*connection.Source.Output +
                                  this.Momentum*(currentWeighValue - previousWeightValue);

                weight.PreviousValue = currentWeighValue;
                weight.Value += deltaWeight;
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
