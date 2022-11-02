// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");

string[] docsID = Directory.GetFiles("URL", ".txt", new EnumerationOptions() { RecurseSubdirectories = true });

foreach (var id in docsID)
{
    
}