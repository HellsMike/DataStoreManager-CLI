using Erp.Items;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erp
{
    public enum ItemCommands
    {
        ShowItems,
        LoadFromFile,
        InsertSimpleItem,
        InsertAdvancedItem,
        EditItem,
        DeleteItem,
        Back
    }

    internal class ItemMenuCommand : BaseCommand
    {
        public override void Execute()
        {
            BaseCommand command = null;
            var result = AnsiConsole
                .Prompt(new SelectionPrompt<ItemCommands>()
                .Title("[bold lightseagreen]Items[/]")
                .AddChoices(new ItemCommands[] { ItemCommands.ShowItems, ItemCommands.LoadFromFile, ItemCommands.InsertSimpleItem,
                    ItemCommands.InsertAdvancedItem, ItemCommands.EditItem, ItemCommands.DeleteItem, ItemCommands.Back }));
            
            switch (result)
            {
                case ItemCommands.ShowItems:
                    command = new ViewItemsCommand();
                    break;

                case ItemCommands.LoadFromFile:
                    command = new LoadItemsCommand();
                    break;

                case ItemCommands.InsertSimpleItem:
                    command = new InsertSimpleItemCommand();
                    break;

                case ItemCommands.InsertAdvancedItem:
                    command = new InsertAdvancedItem();
                    break;

                case ItemCommands.EditItem:
                    command = new EditItemCommand();
                    break;

                case ItemCommands.DeleteItem:
                    command = new DeleteItemCommand();
                    break;

                case ItemCommands.Back:
                    break;
            }
            if (command is not null)
                command.Execute();
        }
    }
}