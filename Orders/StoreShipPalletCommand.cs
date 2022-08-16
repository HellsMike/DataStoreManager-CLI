using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Erp.Orders
{
    internal class StoreShipPalletCommand : BaseCommand
    {
        private static readonly HttpClient client = new HttpClient();
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
                conditions.Add(new ScanCondition("Number", ScanOperator.Equal, orderNumber));
                var order = context.ScanAsync<Order>(conditions).GetRemainingAsync().GetAwaiter().GetResult().First();
                var choices = order.Pallets.Select(x => x.Lpn).ToList();
                choices.Add("Back");
                var lpn = AnsiConsole
                    .Prompt(new SelectionPrompt<string>()
                    .Title("[bold lightseagreen]Pallet[/]")
                    .AddChoices(choices));
                if (lpn != "Back")
                {
                    AnsiConsole.Status().Spinner(Spinner.Known.Dots).StartAsync("[bold sandybrown]Editing...[/]", _ => StoreShipPallet(orderNumber, lpn)).Wait();
                }
            }
        }

        private async Task StoreShipPallet(string orderNumber, string lpn)
        {
            var response = await client.GetAsync($"https://localhost:7219/order/{orderNumber}/{lpn}/store-ship-pallet");
        }
    }
}
