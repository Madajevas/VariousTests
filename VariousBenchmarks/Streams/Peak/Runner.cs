using System.Diagnostics;

var dry = Run(5, dry: true, size: 1024 * 1024).Average();
var run = Run(5, dry: false, size: 1024 * 1024).Average();
Console.WriteLine((run - dry) / 1024);


IEnumerable<long> Run(int iterations, bool dry, int size)
{
    List<string> arguments = ["run", "-c", "Release", "--no-build", "./Test.cs", "--", size.ToString()];
    if (dry)
    {
        arguments.Add("dry");
    }

    long peakPagedMem = 0,
         peakWorkingSet = 0,
         peakVirtualMem = 0;

    for (int i = 0; i < iterations; i++)
    {
        using (Process myProcess = Process.Start("dotnet", string.Join(' ', arguments)))
        {
            do
            {
                if (!myProcess.HasExited)
                {
                    myProcess.Refresh();

                    // peakPagedMem = myProcess.PeakPagedMemorySize64;
                    peakVirtualMem = myProcess.PeakVirtualMemorySize64;
                    // peakWorkingSet = myProcess.PeakWorkingSet64;
                }
            }
            while (!myProcess.WaitForExit(100));

            yield return myProcess.PeakWorkingSet64;

            // yield return peakVirtualMem;

            // Console.WriteLine($"  Process exit code          : {myProcess.ExitCode}");
            // Console.WriteLine($"  Peak physical memory usage : {peakWorkingSet}");
            // Console.WriteLine($"  Peak paged memory usage    : {peakPagedMem}");
            // Console.WriteLine($"  Peak virtual memory usage  : {peakVirtualMem}");
        }
    }
}

