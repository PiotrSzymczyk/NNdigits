using System;
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
        private Thread t;

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
            net = new NeuralNetwork(70, 15, 10, new SigmoidFunction());
            //net = new NeuralNetwork(2, 4, 2, new SigmoidFunction());
            net.LearningRate = double.Parse(learaningRate.Text);
            var trainingSet = ImageLoader.LoadTrainingElementsFromDirectory(this.trainingSet.Text, 10);

            var result = await Task.Run(() =>
            {
                net.Learn(trainingSet);
                return true;
            });
            
            
            var testSet = ImageLoader.LoadTrainingElementsFromDirectory(this.testSet.Text, 10);
            output.Text = ((int)(net.Validate(testSet)*10000)/100d).ToString();
        }

        private void bProcess_Click(object sender, RoutedEventArgs e)
        {
            var input = inputBoard.Select(val => val ? (byte) 1 : (byte) 0).ToList();
            net.Process(input);
            this.output.Text = net.OutputLayer.Neurons.Select(n => Math.Round(n.Output)).Aggregate("", (s, s1) => s + " " + s1);
            textBox1.Text = net.ToString();
        }
    }
}
