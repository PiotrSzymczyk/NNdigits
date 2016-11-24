using System.Linq;
using System.Threading;
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

        private void bLearn_Click(object sender, RoutedEventArgs e)
        {
            net = new NeuralNetwork(70, 15, 10, new FastSigmoidFunction());
            net.LearningRate = 0.1;
            var trainingSet = ImageLoader.LoadTrainingElementsFromDirectory(this.trainingSet.Text, 10);

            net.Learn(trainingSet);
            
            var testSet = ImageLoader.LoadTrainingElementsFromDirectory(this.testSet.Text, 10);
            output.Text = ((int)(net.Validate(testSet)*10000)/100d).ToString();
        }

        private void bProcess_Click(object sender, RoutedEventArgs e)
        {
            var input = inputBoard.Select(val => val ? (byte) 0 : (byte) 255).ToList();
            net.Process(input);
            this.output.Text = net.OutputLayer.Neurons.Select(n => n.Output).Aggregate("", (s, s1) => s + " " + s1);
            textBox.Text = Img();
            textBox1.Text = net.ToString();
        }

        private string Img()
        {
            string res = "";
            for (int y = 0; y < 10; y++)
            {
                for (int x = 0; x < 7; x++)
                {
                    res += inputBoard[y*7 + x] ? "1" : "0";
                }
                res += "\n";
            }
            return res;
        }
    }
}
