using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Net;
using Net.TransferFunctions;

namespace NeuralNetworks2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool[] inputBoard;
        private NeuralNetwork net;
        private bool paused = false;

        public IList<ITransferFunction> TransferFunctions { get; set; } = new List<ITransferFunction>
        {
            new SigmoidFunction(),
            new FastSigmoidFunction()
        };

        public string Output => string.Format(net.OutputLayer.Neurons.Select(n => n.Output).Aggregate("", (s, s1) => s + " " + s1));

        public MainWindow()
        {
            inputBoard = new bool[70];
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button) sender;
            ToggleColor(button);
            ToggleValue(button);
            this.b0x0.Focus();
        }

        private void ToggleValue(Button button)
        {
            var position = button.Name.TrimStart('b').Split('x').Select(val => int.Parse(val)).ToArray();
            inputBoard[7*position[0] + position[1]] = !inputBoard[7*position[0] + position[1]];
        }

        private void ToggleColor(Button button)
        {
            if (button.Background == Brushes.Black)
            {
                button.Background = Brushes.White;
            }
            else
            {
                button.Background = Brushes.Black;
            }
        }

        private async void bLearn_Click(object sender, RoutedEventArgs e)
        {
            InitializeNet();
            var trainingSet = ImageLoader.LoadTrainingElementsFromDirectory(this.trainingSet.Text, int.Parse(tbOutputCount.Text));

            await Task.Run(() =>
            {
                Teach(trainingSet);
                return true;
            });

            UpdateGUI(1);
            
            
            var testSet = ImageLoader.LoadTrainingElementsFromDirectory(this.testSet.Text, int.Parse(tbOutputCount.Text));
        }

        private void InitializeNet()
        {
            var range = double.Parse(tbWeightsRange.Text);
            var transferFun = TransferFunctions[cbTransferFunction.SelectedIndex];
            transferFun.Beta = double.Parse(tbBeta.Text);

            net = new NeuralNetwork(int.Parse(tbInputCount.Text), int.Parse(tbHiddenCount.Text),
                int.Parse(tbOutputCount.Text), transferFun, -range, range)
            {
                LearningRate = double.Parse(tbLearningRate.Text),
                MaxNumberOfEpoch = int.Parse(tbNumOfEpoch.Text),
                Momentum = double.Parse(tbMomentum.Text)
            };
        }

        private void bProcess_Click(object sender, RoutedEventArgs e)
        {
            var input = inputBoard.Select(val => val ? (byte) 1 : (byte) 0).ToList();
            net.Process(input);
        }

        public void Teach(IList<TrainingElement> trainingSet)
        {
            for (int i = 0; i < net.MaxNumberOfEpoch; i++)
            {
                net.DoLearningEpoch(trainingSet.OrderBy(val => RandomGenerator.NextDouble()).ToList());
                //UpdateGUI(i);
                while (paused)
                {
                    Thread.Sleep(1000);
                }
            }
        }

        private void UpdateGUI(int numOfEpoch)
        {
            tbCurrentEpoch.Text = numOfEpoch.ToString();
            tbTotalNetworkError.Text = net.TotalNetworkError.ToString();
        }

        public void TeachOneEpoch(IList<TrainingElement> trainingSet)
        {
            net.DoLearningEpoch(trainingSet.OrderBy(val => RandomGenerator.NextDouble()).ToList());
        }
    }
}
