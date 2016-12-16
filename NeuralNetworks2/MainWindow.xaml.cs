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
        private const string PatternsPath = "..\\..\\..\\Patterns\\";
        private bool[] inputBoard;
        private NeuralNetwork net;
        private bool paused = false;
        private bool isRunning = false;

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
            InitializeNet();
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
            if (!isRunning)
            {
                isRunning = true;
                paused = false;
                BlockSetupUI(true);
                bProcess.IsEnabled = false;
                bLearn.Content = "Stop";

                InitializeNet();

                var trainingSet = ImageLoader.LoadTrainingElementsFromDirectory(this.trainingSet.Text,
                    int.Parse(tbOutputCount.Text));
                var validationSet = ImageLoader.LoadTrainingElementsFromDirectory(this.testSet.Text,
                    int.Parse(tbOutputCount.Text));
                var precision = double.Parse(tbPrecision.Text);

                await Task.Run(() =>
                {
                    Teach(trainingSet, validationSet, precision);
                    return true;
                });

                BlockSetupUI(false);
                bProcess.IsEnabled = true;
                bLearn.Content = "Learn";
                isRunning = false;
            }
            else if (paused)
            {
                paused = false;
                bProcess.IsEnabled = false;
                bLearn.Content = "Pause";
            }
            else
            {
                paused = true;
                bProcess.IsEnabled = true;
                bLearn.Content = "Continue";
            }
        }

        private void BlockSetupUI(bool disable)
        {
            tbInputCount.IsEnabled = !disable;
            tbHiddenCount.IsEnabled = !disable;
            tbOutputCount.IsEnabled = !disable;
            cbTransferFunction.IsEnabled = !disable;
            tbWeightsRange.IsEnabled = !disable;
            tbBeta.IsEnabled = !disable;
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
            tbResult.Text = net.Output.ToString();
        }

        public void Teach(IList<TrainingElement> trainingSet, IList<TrainingElement> validationSet, double precision)
        {
            for (int i = 0; i < net.MaxNumberOfEpoch && isRunning; i++)
            {
                net.DoLearningEpoch(trainingSet.OrderBy(val => RandomGenerator.NextDouble()).ToList());
                net.Validate(validationSet);
                UpdateGUI(i, precision);

                if (net.ValidationAccuracy > (1-precision)*100)
                {
                    break;
                }
                while (paused)
                {
                    Thread.Sleep(1000);
                }
            }
        }

        public void TeachOneEpoch(IList<TrainingElement> trainingSet)
        {
            net.DoLearningEpoch(trainingSet.OrderBy(val => RandomGenerator.NextDouble()).ToList());
        }

        private void UpdateGUI(int numOfEpoch, double precision)
        {
            this.InvokeIfRequired(val => tbCurrentEpoch.Text = val.ToString(), numOfEpoch);
            this.InvokeIfRequired(val => tbTotalNetworkError.Text = val, net.ValidationAccuracy.ToString());
            this.InvokeIfRequired(val => progress.Value = val, net.ValidationAccuracy/(1 - precision)*100);
        }

        private void cbPattern_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var image = ImageLoader.LoadImage(PatternsPath + cbPattern.SelectedIndex + ".png");
            var newInputBoard = ImageLoader.ParseImageToVector(image).Select(val => val == 1).ToArray();
            for (int i = 0; i < inputBoard.Length; i++)
            {
                if (newInputBoard[i] != inputBoard[i])
                {
                    ToggleButton(i);
                }
            }
        }

        private void tbLearningRate_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (net != null)
            {
                net.LearningRate = double.Parse(tbLearningRate.Text);
            }
        }

        private void tbNumOfEpoch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (net != null)
            {
                net.MaxNumberOfEpoch = int.Parse(tbNumOfEpoch.Text);
            }
        }

        private void tbMomentum_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (net != null)
            {
                net.Momentum = double.Parse(tbMomentum.Text);
            }
        }

        private void bReset_Click(object sender, RoutedEventArgs e)
        {
            isRunning = false;
            paused = false;
            InitializeNet();
            bProcess.IsEnabled = true;
            progress.Value = 0;
            tbCurrentEpoch.Text = "0";
            tbTotalNetworkError.Text = "0";
        }

        private void bDistort_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < 3; i++)
            {
                ToggleButton(RandomGenerator.Next(70));
            }
        }

        private void ToggleButton(int index)
        {
            var column = index % 7;
            var row = index / 7;
            var button = (Button) buttonInputBoard.Children.Cast<UIElement>()
                .First(b => Grid.GetRow(b) == row && Grid.GetColumn(b) == column);
            ToggleColor(button);
            ToggleValue(button);
        }
    }
}
