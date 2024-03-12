// See https://aka.ms/new-console-template for more information

using Sandbox.ViewModels;

Console.WriteLine(SomeEnumViewModelWrapper.SomeEnumViewModel.OptionTwo);

enum SomeEnum
{
    None,
    OptionOne,
    OptionTwo,
}
