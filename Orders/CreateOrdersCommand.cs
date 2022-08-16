using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Erp
{
    internal class CreateOrdersCommand : BaseCommand
    {
        private static readonly HttpClient client = new HttpClient();

        public override void Execute()
        {
            var orderTypes = AnsiConsole
                .Prompt(new MultiSelectionPrompt<string>()
                .Title("[bold lightseagreen]Orders Types[/]")
                .InstructionsText("[grey]Press Space to select and Enter to accept\n\nLegend:\nI = Inbound\nO = Outbound[/]")
                .AddChoices("I", "O"));

            var conditions = new List<ScanCondition>();
            using var context = AwsClient.GetContext();
            var items = context.ScanAsync<Item>(conditions).GetRemainingAsync().GetAwaiter().GetResult();

            var names = items.Select(x => x.Name).ToList();

            var typeOrders = new List<List<IDictionary<string, string>>>(2);
            var itemsInUpdate = new List<string>();
            var itemsOutUpdate = new List<string>();

            foreach (var orderType in orderTypes)
            {
                typeOrders.Add(new List<IDictionary<string, string>>());
                var index = orderType == "I" ? 0 : 1;
                var ordersCount = AnsiConsole.Ask<int>($"How Many {orderType} Orders? ");
                for (int i = 0; i < ordersCount; i++)
                {
                    AnsiConsole.Clear();
                    AnsiConsole.Markup($"[darkseagreen]Order #{i + 1}[/]\n");
                    typeOrders[index].Add(new Dictionary<string, string>());
                    typeOrders[index][i].Add("type", orderType);
                    typeOrders[index][i].Add("number", AnsiConsole.Prompt(
                        new TextPrompt<string>("Number: ")
                        .Validate(number =>
                        {
                            if (number.StartsWith(orderType))
                                return ValidationResult.Success();
                            else
                                return ValidationResult.Error("[red]Order number should start with order type letter[/]");
                        })));
                    var pallets = new List<OrderPallet>();
                    var orderPalletCount = AnsiConsole.Ask<int>("How Many Pallets? ");
                    for (int j = 0; j < orderPalletCount; j++)
                    {
                        AnsiConsole.Clear();
                        AnsiConsole.Markup($"[darkseagreen]Pallet #{j + 1}[/]\n");
                        var lpn = AnsiConsole.Ask<string>("Lpn: ");
                        var item = AnsiConsole
                            .Prompt(new SelectionPrompt<string>()
                            .Title("[bold lightseagreen]Item[/]")
                            .AddChoices(names));
                        pallets.Add(GeneratePallet(lpn, item).GetAwaiter().GetResult());
                        if (orderType == "I")
                            itemsInUpdate.Add(item);
                        else
                            itemsOutUpdate.Add(item);
                    }
                    typeOrders[index][i].Add("pallets", JsonSerializer.Serialize(pallets));
                    AnsiConsole.Clear();
                }
            }
            AnsiConsole.Status().Spinner(Spinner.Known.Dots).StartAsync("[bold sandybrown]Generating...[/]", _ => GenerateOrder(typeOrders, items, itemsInUpdate, itemsOutUpdate)).Wait();
        }

        private async Task GenerateOrder(List<List<IDictionary<string, string>>> typeValues, List<Item> items, List<string> itemsInUpdate, List<string> itemsOutUpdate)
        {
            var orders = new List<Order>();
            foreach (var ordersValues in typeValues)
            {
                foreach (var values in ordersValues)
                {
                    orders.Add(new Order
                    {
                        Number = values["number"],
                        Type = values["type"],
                        Status = OrderStatus.Created,
                        Pallets = JsonSerializer.Deserialize<List<OrderPallet>>(values["pallets"])
                    });
                }
            }
            var body = new OrderEvent
            {
                Orders = orders
            };
            var response = client.PostAsJsonAsync("https://localhost:7219/order/add-update", body).GetAwaiter().GetResult();
            
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                using var context = AwsClient.GetContext();
                foreach (var order in orders)
                    await context.SaveAsync(order);

                foreach (var itemName in itemsInUpdate)
                {
                    var itemToUpdate = items.Where(x => x.Name == itemName).First();
                    itemToUpdate.PalletsCount++;
                    await context.SaveAsync(itemToUpdate);
                }
                foreach (var itemName in itemsOutUpdate)
                {
                    var itemToUpdate = items.Where(x => x.Name == itemName).First();
                    itemToUpdate.PalletsCount--;
                    await context.SaveAsync(itemToUpdate);
                }
            }
        }

        private async Task<OrderPallet> GeneratePallet(string lpn, string item)
        {
            return await Task.FromResult (new OrderPallet
            {
                Lpn = lpn,
                Item = item,
            });
        }
    }
}