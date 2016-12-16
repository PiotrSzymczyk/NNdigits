using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Net;
using Net.TransferFunctions;
using Brushes = System.Windows.Media.Brushes;
using Color = System.Windows.Media.Color;

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

                var trainingSet = ImageLoader.LoadTrainingElementsFromDirectoryWithoutLabels(this.trainingSet.Text);

                await Task.Run(() =>
                {
                    Teach(trainingSet);
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

            var hiddenLayerImage = ImageLoader.GetHiddenLayerImage(7, 10, 7, 10,
            net.HiddenLayers.First()
                .Neurons.Select(n => n.ShowLearnedFunction()).GetEnumerator(), 2);
            hiddenLayerImage.Save("img.bmp");
            this.hiddenLayer.Source =
                ImageLoader.ToBitmapImage(hiddenLayerImage);
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
            output.Source = ImageLoader.ToBitmapImage(ImageLoader.ParseVectorToImage(net.OutputLayer.Neurons.Select(neuron => (byte)(neuron.Output*255)).ToArray()));
        }

        public void Teach(IList<TrainingElement> trainingSet)
        {
            for (int i = 0; i < net.MaxNumberOfEpoch && isRunning; i++)
            {
                net.DoLearningEpoch(trainingSet.OrderBy(val => RandomGenerator.NextDouble()).ToList());
                UpdateGUI(i);
                while (paused)
                {
                    Thread.Sleep(1000);
                }
            }
        }

        private void UpdateGUI(int numOfEpoch)
        {
            this.InvokeIfRequired(val => tbCurrentEpoch.Text = val.ToString(), numOfEpoch);
            this.InvokeIfRequired(val => tbTotalNetworkError.Text = val, net.ValidationAccuracy.ToString());
        }

        private void cbPattern_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var desiredState = ImageLoader.ParseImageToVector(
                ImageLoader.LoadImage(PatternsPath + cbPattern.SelectedIndex + ".png"))
                .Select(val => val == 1).
                ToArray();
            UpdateBoard(desiredState);
        }

        private void UpdateBoard(bool[] desiredState)
        {
            
            for (int i = 0; i < inputBoard.Length; i++)
            {
                if (desiredState[i] != inputBoard[i])
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

        public static BitmapSource ToWpfBitmap(Bitmap bitmap)
        {
            var hBitmap = bitmap.GetHbitmap();
            var result = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, 
                System.Windows.Int32Rect.Empty, System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
            return result;
        }
    }
}
