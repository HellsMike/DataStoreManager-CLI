using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Orders
{
    public enum ActionCommands
    {
        AddPallet,
        RemovePallet,
        Back,
    }

    internal class EditOrderCommand : BaseCommand
    {
        public override void Execute()
        {
            using var context = AwsClient.GetContext();
            var conditions = new List<ScanCondition>();

            var orders = context.ScanAsync<Order>(conditions).GetRemainingAsync().GetAwaiter().GetResult();
            var orderNumbers = orders.Select(x => x.Number).ToList();
            orderNumbers.Add("Back");
            var orderNumber = AnsiConsole
                .Prompt(new SelectionPrompt<string>()
                .Title("[bold lightseagreen]Order[/]")
                .AddChoices(orderNumbers));
            
            if (orderNumber != "Back")
            {
                BaseCommand command = null;
                var result = AnsiConsole
                    .Prompt(new SelectionPrompt<ActionCommands>()
                    .Title("[bold lightseagreen]Possible Action[/]")
                    .AddChoices(Enum.GetValues(typeof(ActionCommands)).Cast<ActionCommands>()));
                switch (result)
                {
                    case ActionCommands.AddPallet:
                        command = new AddPalletCommand(orderNumber);
                        break;
                    case ActionCommands.RemovePallet:
                        command = new RemovePalletCommand(orderNumber);
                        break;
                    case ActionCommands.Back:
                        break;
                }
                if (command is not null)
                    command.Execute();
            }
        }
    }

    internal class AddPalletCommand : BaseCommand
    {
        private static readonly HttpClient client = new HttpClient();
        private readonly string _orderNumber;
        public AddPalletCommand(string orderNumber)
        {
            _orderNumber = orderNumber;
        }

        public override void Execute()
        {
            var conditions = new List<ScanCondition>();
            using var context = AwsClient.GetContext();
            
            var items = context.ScanAsync<Item>(conditions).GetRemainingAsync().GetAwaiter().GetResult();
            var names = items.Select(x => x.Name).ToList();
            names.Add("Back");
            
            var lpn = AnsiConsole.Ask<string>("Lpn: ");
            var item = AnsiConsole
                .Prompt(new SelectionPrompt<string>()
                .Title("[bold lightseagreen]Item[/]")
                .AddChoices(names));
            
            if (item != "Back")
                AnsiConsole.Status().Spinner(Spinner.Known.Dots).StartAsync("[bold sandybrown]Editing...[/]", _ => AddPallet(_orderNumber, lpn, item, items)).Wait();
        }

        private async Task AddPallet (string orderNumber, string lpn, string item, IEnumerable<Item> items)
        {
            using var context = AwsClient.GetContext();
            var conditions = new List<ScanCondition>(1) { new ScanCondition("Number", ScanOperator.Equal, orderNumber) };
            var order = context.ScanAsync<Order>(conditions).GetRemainingAsync().GetAwaiter().GetResult().First();
            order.Pallets.Add(new OrderPallet
            {
                Lpn = lpn,
                Item = item
            });

            var body = new OrderEvent
            {
                Orders = new List<Order>(1){ order }
            };
            var response = client.PostAsJsonAsync("https://localhost:7219/order/add-update", body).GetAwaiter().GetResult();

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                await context.SaveAsync(order);

                var itemToUpdate = items.Where(x => x.Name == item).First();
                itemToUpdate.PalletsCount++;
                await context.SaveAsync(itemToUpdate);
            }
        }
    }

    internal class RemovePalletCommand : BaseCommand
    {
        private static readonly HttpClient client = new HttpClient();
        private readonly string _orderNumber;
        public RemovePalletCommand(string orderNumber)
        {
            _orderNumber = orderNumber;
        }

        public override void Execute()
        {
            var conditions = new List<ScanCondition>();
            using var context = AwsClient.GetContext();

            conditions.Add(new ScanCondition("Number", ScanOperator.Equal, _orderNumber));
            var order = context.ScanAsync<Order>(conditions).GetRemainingAsync().GetAwaiter().GetResult().First();
            var choices = order.Pallets.Select(x => x.Lpn).ToList();
            choices.Add("Back");
            var lpn = AnsiConsole
                .Prompt(new SelectionPrompt<string>()
                .Title("[bold lightseagreen]Pallet[/]")
                .AddChoices(choices));
            if (lpn != "Back")
            {
                var item = order.Pallets.Where(x => x.Lpn == lpn).First().Item;
                AnsiConsole.Status().Spinner(Spinner.Known.Dots).StartAsync("[bold sandybrown]Editing...[/]", _ => RemovePallet(_orderNumber, lpn, item, order)).Wait();
            }
        }

        private async Task RemovePallet(string orderNumber, string lpn, string item, Order order)
        {
            var palletsToKeep = order.Pallets.Where(x => x.Lpn != lpn).ToList();
            order.Pallets.RemoveAll(x => x.Lpn != lpn);

            var body = new OrderEvent
            {
                Orders = new List<Order>(1) { order }
            };
            var response = client.PostAsJsonAsync("https://localhost:7219/order/remove-pallets", body).GetAwaiter().GetResult();

            using var context = AwsClient.GetContext();
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                order.Pallets.RemoveAll(x => x.Lpn == lpn);
                foreach (var pallet in palletsToKeep)
                {
                    order.Pallets.Add(pallet);
                };
                await context.SaveAsync(order);
                var conditions = new List<ScanCondition>(1) { new ScanCondition("Name", ScanOperator.Equal, item) };
                var itemToUpdate = await context.ScanAsync<Item>(conditions).GetRemainingAsync();

                itemToUpdate.First().PalletsCount--;
                await context.SaveAsync(itemToUpdate.First());
            }
        }
    }
}
