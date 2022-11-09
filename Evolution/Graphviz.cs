using System.Diagnostics;

namespace Evolution; 

public class Graphviz {
    public static string CreateGraph(string graph) {
        var process = new Process();
        var psi = process.StartInfo;
        psi.FileName = "dot";
        psi.Arguments = "-Tsvg";
        psi.UseShellExecute = false;
        psi.RedirectStandardOutput = true;
        psi.RedirectStandardInput = true;
        psi.RedirectStandardError = true;
        process.Start();
        process.StandardInput.Write(graph);
        process.StandardInput.Close();
        string result = process.StandardOutput.ReadToEnd();
        string error = process.StandardError.ReadToEnd();
        if (error != "") {
            var graphLines = graph.Split("\n").Select((x, index) => $"  {index+1:D2} {x}");
            Console.WriteLine($"Graph:\n{string.Join("\n", graphLines)}\n\nError:\n  {error}");
        }
        return CreateDataURL("image/svg+xml", result);
    }

    public static string CreateDataURL(string contentType, string body) {
        var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(body);
        var data = System.Convert.ToBase64String(plainTextBytes).Trim();
        return $"data:{contentType};base64,{data}";
    }
}
