using System.Numerics;

namespace SpreadsheetEngine
{
    public class Cell
    {
        public string Expression { get; set; } = "";
        public BigInteger Value { get; set; }
        public string Error { get; set; } = "";
    }
}
