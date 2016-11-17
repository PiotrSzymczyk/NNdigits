namespace Net.Base
{
    public class Weight
    {
        public double Value { get; set; }

        public double PreviousValue { get; set; }

        public Weight(double min = -0.5d, double max = 0.5d)
        {
            Randomize(min, max);
        }

        public void Randomize(double min, double max)
        {
            this.Value = min + RandomGenerator.NextDouble() * (max - min);
            this.PreviousValue = this.Value;
        }
    }
}
