using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Items
{
    internal class InsertAdvancedItem : BaseCommand
    {
        private static readonly HttpClient client = new HttpClient();
        public override void Execute()
        {
            var itemsCount = AnsiConsole.Ask<int>("How Many Items? ");
            var values = new List<IDictionary<string,string>>(itemsCount);
            AnsiConsole.Clear();

            for (int i = 0; i < itemsCount; i++)
            {
                values.Add(new Dictionary<string, string>());
                AnsiConsole.Markup($"[darkseagreen]Item #{i + 1}[/]\n");
                values[i].Add("name", AnsiConsole.Ask<string>("Item Name: "));
                values[i].Add("height", AnsiConsole.Ask<decimal>("Height: ").ToString());
                values[i].Add("length", AnsiConsole.Ask<decimal>("Length: ").ToString());
                values[i].Add("width", AnsiConsole.Ask<decimal>("Width: ").ToString());
                values[i].Add("netWeight", AnsiConsole.Ask<decimal>("Net Weight: ").ToString());
                values[i].Add("description", AnsiConsole.Ask<string>("Description: "));
                values[i].Add("extDescription", AnsiConsole.Ask<string>("Extended Description: "));
                values[i].Add("customer", AnsiConsole.Ask<string>("Customer Code: "));
                values[i].Add("daysToExpiry", AnsiConsole.Ask<int>("Days To Expiry: ").ToString());
                values[i].Add("defaultShelfLife", AnsiConsole.Ask<int>("Default Shelf Life: ").ToString());
                AnsiConsole.Clear();
            }
            AnsiConsole.Status().Spinner(Spinner.Known.Dots).StartAsync("[bold sandybrown]Generating...[/]", _ => GenerateItems(values)).Wait();
            AnsiConsole.Clear();
        }

        private async Task GenerateItems(List<IDictionary<string, string>> values)
        {
            var items = new List<Item>(); 
            foreach (var dict in values)
            {
                items.Add(new Item
                {
                    Name = dict["name"],
                    Height = Convert.ToDecimal(dict["height"]),
                    Length = Convert.ToDecimal(dict["length"]),
                    Width = Convert.ToDecimal(dict["width"]),
                    NetWeight = Convert.ToDecimal(dict["netWeight"]),
                    Description = dict["description"],
                    ExtDescription = dict["extDescription"],
                    CustomerCode = dict["customer"],
                    DaysToExpiry = Int32.Parse(dict["daysToExpiry"]),
                    DefaultShelfLife = Int32.Parse(dict["defaultShelfLife"]),
                    PalletsCount = 0,
                    Quantity = 0,
                    Active = true,
                    Version = DateTime.Now,
                });
            }

            var body = new ItemEvent
            {
                Data = items,
                Versioning = DateTime.Now,
            };
            var response = client.PostAsJsonAsync("https://localhost:7219/item", body).Result;
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                using var context = AwsClient.GetContext();
                foreach (var item in items)
                    await context.SaveAsync(item);
            }
        }
    }
}
