using System;
using System.IO;
using System.Text;

class Program
{
    static void Main(string[] args)
    {
        string directoryPath = @"..\WebConsultasMedicas\Views";
        string[] files = Directory.GetFiles(directoryPath, "*.cshtml", SearchOption.AllDirectories);

        foreach (string file in files)
        {
            string content = File.ReadAllText(file, Encoding.UTF8);

            content = content.Replace("Ã¡", "á");
            content = content.Replace("Ã©", "é");
            content = content.Replace("Ã³", "ó");
            content = content.Replace("Ãº", "ú");
            content = content.Replace("Ã±", "ñ");
            content = content.Replace("Ã\x81", "Á"); // Á
            content = content.Replace("Ã‰", "É");
            content = content.Replace("Ã\x8D", "Í"); // Í
            content = content.Replace("Ã“", "Ó");
            content = content.Replace("Ãš", "Ú");
            content = content.Replace("Ã‘", "Ñ");
            content = content.Replace("Â¿", "¿");
            content = content.Replace("Â¡", "¡");
            
            // The invisible char for í is \u00AD
            content = content.Replace("Ã\u00AD", "í"); 

            File.WriteAllText(file, content, Encoding.UTF8);
        }
        Console.WriteLine("Done replacing mojibake!");
    }
}
