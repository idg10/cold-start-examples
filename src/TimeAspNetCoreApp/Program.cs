
using System.Diagnostics;

//string commandLine = "dotnet-trace collect --providers System.Net.Http,Endjin.StartupTests -- " + args[0];
//string providers = "System.Net.Http,Endjin.StartupTests";
//string providers = "Microsoft.AspNetCore";
//string arguments = $"collect --providers {providers} --show-child-io -- {args[0]}";
//ProcessStartInfo psi = new("dotnet-trace")
ProcessStartInfo psi = new(args[0])
{
    //Arguments = arguments,
    UseShellExecute = false,
    RedirectStandardOutput = true,
    RedirectStandardInput = true,
};

Console.WriteLine("About to launch:");
Thread.Sleep(2000);
Stopwatch sw = Stopwatch.StartNew();
Process childProc = Process.Start(psi)!;
Console.WriteLine(sw.Elapsed);
ManualResetEventSlim done = new();

async Task UseAndThenStop(string url)
{
    var baseUrl = new Uri(url);
    var endpointUrl = new Uri(baseUrl, "/imm/github/corvus-dotnet/Corvus.Identity/total");
    try
    {
        using HttpClient http = new();
        HttpResponseMessage response = await http.GetAsync(endpointUrl);
        sw.Stop();
        Console.WriteLine(response.StatusCode);
        //Console.WriteLine(await response.Content.ReadAsStringAsync());
    }
    catch (Exception x)
    {
        Console.WriteLine(x);
    }

    Console.WriteLine(sw.Elapsed);

    childProc.StandardInput.WriteLine("");

    Console.WriteLine("Waiting for web app to shut down");
    await childProc.WaitForExitAsync();
    Console.WriteLine("Web app shut down");
    done.Set();
}


childProc.OutputDataReceived += (_, e) =>
{
    const string match = "Now listening on: ";
    if (e.Data is string line)
    {
        //Console.WriteLine(line);
        int p = line.IndexOf(match);
        if (p >= 0)
        {
            string url = line[(p + match.Length)..];
            if (url.StartsWith("http:"))
            {
                _ = Task.Run(() => UseAndThenStop(url));

                return;
            }
        }
    }
};
childProc.BeginOutputReadLine();

////using Microsoft.Diagnostics.NETCore.Client;
////using Microsoft.Diagnostics.Tracing;
////using Microsoft.Diagnostics.Tracing.Parsers;

////using System.Diagnostics;
////using System.Diagnostics.Tracing;

////string diagnosticTransportName = $"ColdStartTool-{Process.GetCurrentProcess().Id}-{DateTime.Now:yyyyMMdd_HHmmss}.socket";

//////var b = new DiagnosticsClientBuilder()

////var childProc = new Process();
////childProc.StartInfo.FileName = args[0];
////childProc.StartInfo.UseShellExecute = false;
////childProc.StartInfo.RedirectStandardOutput = true;
////childProc.StartInfo.RedirectStandardError = true;
////childProc.StartInfo.RedirectStandardInput = true;
//////childProc.StartInfo.Environment.Add("DOTNET_DiagnosticPorts", $"{diagnosticTransportName},suspend");
//////childProc.StartInfo.Environment.Add("DOTNET_DiagnosticPorts", $"suspend");


////childProc.OutputDataReceived += (_, e) =>
////{
////    const string match = "Now listening on: ";
////    if (e.Data is string line)
////    {
////        Console.WriteLine(line);
////        int p = line.IndexOf(match);
////        if (p >= 0)
////        {
////            string url = line[(p + match.Length)..];
////            if (url.StartsWith("http:"))
////            {
////                Console.WriteLine($"URL: {url}");
////                return;
////            }
////        }
////    }
////};
//////Task.Run(async () =>
//////{
//////    const string match = "Now listening on: ";
//////    Console.WriteLine("Watching stdout...");
//////    while ((await childProc.StandardOutput.ReadLineAsync()) is string line)
//////    {
//////        int p = line.IndexOf(match);
//////        if (p >= 0)
//////        {
//////            string url = line[(p + match.Length)..];
//////            if (url.StartsWith("http:"))
//////            {
//////                Console.WriteLine($"URL: {url}");
//////                return;
//////            }
//////        }
//////    }
//////});

////await Task.Delay(3000);
////childProc.Start();
////childProc.BeginOutputReadLine();

////var client = new DiagnosticsClient(childProc.Id);
////var providers = new List<EventPipeProvider>()
////{
////    new EventPipeProvider("Microsoft-Windows-DotNETRuntime",
////        EventLevel.Informational, (long)ClrTraceEventParser.Keywords.GC),
////    new EventPipeProvider("System.Net.Http", EventLevel.Informational, -1),
////    new EventPipeProvider("Endjin.StartupTests", EventLevel.Informational, -1),
////};

////using (EventPipeSession session = client.StartEventPipeSession(providers, false))
////{
////    var source = new EventPipeEventSource(session.EventStream);

////    source.Clr.All += (TraceEvent obj) => Console.WriteLine(obj.ToString());
////    source.Dynamic.All += (TraceEvent obj) => Console.WriteLine(obj.ToString());

////    //client.ResumeRuntime();

////    try
////    {
////        source.Process();
////    }
////    catch (Exception e)
////    {
////        Console.WriteLine("Error encountered while processing events");
////        Console.WriteLine(e.ToString());
////    }
////}

//Console.WriteLine("!!");

//Console.ReadLine();
done.Wait();