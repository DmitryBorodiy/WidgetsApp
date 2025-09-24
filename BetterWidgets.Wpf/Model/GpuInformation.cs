using System.Text;
using BetterWidgets.Extensions;

namespace BetterWidgets.Model
{
    public class GpuInformation
    {
        public string Name { get; set; }
        public ulong TotalMemory { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(Name);
            sb.AppendLine(TotalMemory.ToReadable());

            return sb.ToString();
        }
    }
}
