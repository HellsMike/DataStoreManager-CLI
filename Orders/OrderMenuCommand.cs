using Erp.Orders;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erp
{
    public enum Command
    {
        ViewOrders,
        CreateOrders,
        EditOrder,
        ChangeStatus,
        StoreOrShipPallet,
        Back
    }

    internal class OrderMenuCommand : BaseCommand
    {
        public override void Execute()
        {
            BaseCommand command = null;
            var result = AnsiConsole
                .Prompt(new SelectionPrompt<Command>()
                .Title("[bold lightseagreen]Orders[/]")
                .AddChoices(Enum.GetValues(typeof(Command)).Cast<Command>()));
            switch (result)
            {
                case Command.CreateOrders:
                    command = new CreateOrdersCommand();
                    break;

                case Command.EditOrder:
                    command = new EditOrderCommand();
                    break;

                case Command.ChangeStatus:
                    command = new ChangeOrderStatusCommand();
                    break;

                case Command.ViewOrders:
                    command = new ViewOrdersCommand();
                    break;

                case Command.StoreOrShipPallet:
                    command = new StoreShipPalletCommand();
                    break;

                default:
                    break;
            }
            if (command is not null)
                command.Execute();
        }
    }
}