using Erp;
using Spectre.Console;
using System.Collections.Specialized;

var exit = false;

while (!exit)
{
    AnsiConsole.Clear();
    var rule = new Rule("[green]ERP[/]").RuleStyle("grey42");
    AnsiConsole.Write(rule);
    BaseCommand baseCommand = null;
    var command = AnsiConsole.Prompt(new SelectionPrompt<CommandMenu>().Title("[bold lightseagreen]Navigation Menu[/]").AddChoices(new CommandMenu[] { CommandMenu.Items, CommandMenu.Orders, CommandMenu.Exit }));
    switch (command)
    {
        case CommandMenu.Items:
            baseCommand = new ItemMenuCommand();
            break;

        case CommandMenu.Orders:
            baseCommand = new OrderMenuCommand();
            break;

        default:
            exit = true;
            break;
    }
    if (baseCommand is not null)
        baseCommand.Execute();
}

public enum CommandMenu
{
    Items,
    Orders,
    Exit
}