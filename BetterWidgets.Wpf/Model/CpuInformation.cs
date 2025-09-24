using System.Text;

namespace BetterWidgets.Model
{
    public class CpuInformation
    {
        public string Name { get; set; }
        public int Cores { get; set; }
        public int Load { get; set; }
        public double Threads { get; set; }
        public double MaxClock { get; set; }
        public double CurrentClock { get; set; }

        public bool HasCurrentClock  => CurrentClock != default;

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine($"Name: {Name}");
            builder.AppendLine($"Cores: {Cores}");
            builder.AppendLine($"Threads: {Threads}");
            builder.AppendLine($"Load: {Load}%");
            builder.AppendLine($"Max Clock: {MaxClock} MHz");

            if(HasCurrentClock)
               builder.AppendLine($"Current Clock: {CurrentClock} MHz");

            return builder.ToString();
        }
    }
}
