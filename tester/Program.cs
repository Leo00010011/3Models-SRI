// See https://aka.ms/new-console-template for more information
using DP.Classes;


var doc = new ProcesedDocument("casa casa pronombres perro gato");
foreach(var item in doc)
{
    Console.WriteLine(item.Item1 + ": " + item.Item2.ToString());
}