using BenchmarkDotNet.Attributes;
using Elders.Cronus.EventStore.Index;
using System.Text;

BenchmarkDotNet.Running.BenchmarkRunner.Run<ByteArreyLookupBenchmanr>();
ByteArreyLookupBenchmanr.FindPatternIndexOf();

[MemoryDiagnoser]
public class ByteArreyLookupBenchmanr
{
    const string text = $"e5d3c367-a87b-4bf3-9dbb-34d3a7262b79____ef39b561-3420-49b0-be79-5c31749d9986_4121dd8f-1e61-4b6d-b17a-0eb956a451c8";
    static byte[] textBytes = Encoding.UTF8.GetBytes(text);
    static byte[] patternBytes = Encoding.UTF8.GetBytes("asd");

    [Benchmark]
    public static void FindPattern()
    {
        //var asd = ByteArrayLookup.FindSequence(textBytes, 0, patternBytes);
    }


    [Benchmark]
    public static void FindPatternNew()
    {
        //var asd = ByteArrayLookup.FindSequenceNew(textBytes, 0, patternBytes);
    }

    [Benchmark]
    public static void FindPatternIndexOf()
    {
        // var asd = ByteArrayLookup.FindSequenceIndexOf(textBytes, 0, patternBytes);
    }
}
