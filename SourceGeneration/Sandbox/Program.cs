// See https://aka.ms/new-console-template for more information
using Generator.Abstractions;

Console.WriteLine("Hello, World!");

class SomeEnum
{
    public int Value { get; set; }
}

[GenerateViewModel<SomeEnum>]
public partial class SomeEnumViewModel
{

}
