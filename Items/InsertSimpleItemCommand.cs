using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Net.Http.Json;

namespace Erp.Items
{
    internal class InsertSimpleItemCommand : BaseCommand
    {
        private static readonly HttpClient client = new HttpClient();
        public override void Execute()
        {
            var itemsCount = AnsiConsole.Ask<int>("How Many Items? ");
            var names = new List<string>();
            for (int i = 0; i < itemsCount; i++)
            {
                AnsiConsole.Markup($"[darkseagreen]Item #{i + 1}[/]\n");
                var itemName = AnsiConsole.Ask<string>("Name: ");
                names.Add(itemName);
            }
            AnsiConsole.Status().Spinner(Spinner.Known.Dots).StartAsync("[bold sandybrown]Generating...[/]", _ => GenerateItem(names)).Wait();
            AnsiConsole.Clear();
        }

        private async Task GenerateItem(List<string> names)
        {
            var items = new List<Item>();
            foreach (var name in names)
            {
                items.Add(new Item
                {
                    Name = name,
                    Height = 0,
                    Length = 0,
                    Width = 0,
                    NetWeight = 0,
                    Quantity = 0,
                    Active = true,
                    Version = DateTime.Now,
                    PalletsCount = 0,
                });
            }

            var body = new ItemEvent
            {
                Data = items,
                Versioning = DateTime.Now
            };
            var response = client.PostAsJsonAsync("https://localhost:7219/item", body).GetAwaiter().GetResult();
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                using var context = AwsClient.GetContext();
                foreach (var item in items)
                    await context.SaveAsync(item);
            }
        }
    }
}
