namespace BetterWidgets.ViewModel.Components
{
    public sealed class ClockHourMark
    {
        public ClockHourMark(int number)
        {
            Number = number;
        }

        public int Number { get; }
        public double Angle => Number * 30;

        private const double Center = 60;
        private const double Radius = 50;
        private const double Offset = 7;

        public double X => Center + Math.Sin(Angle * Math.PI / 180) * Radius - Offset;
        public double Y => Center - Math.Cos(Angle * Math.PI / 180) * Radius - Offset;
    }
}
