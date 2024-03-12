// See https://aka.ms/new-console-template for more information
using Generator.Abstractions;

Console.WriteLine("Hello, World!");

enum SomeEnum
{
    None,
    OptionOne,
    OptionTwo,
}

[GenerateViewModel<SomeEnum>]
public partial class SomeEnumViewModel
{

}
