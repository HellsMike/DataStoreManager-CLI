using Amazon.DynamoDBv2.DataModel;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Erp.Orders
{
    internal class ViewOrdersCommand : BaseCommand
    {
        private static readonly HttpClient client = new HttpClient();

        public override void Execute()
        {
            const string baseuUrl = "https://localhost:7219/order";
            string input = "start";
            var page = 1;
            var type = AnsiConsole
                .Prompt(new SelectionPrompt<string>()
                .Title("[red]Type[/]")
                .AddChoices("Inbound", "Outbound", "Back"))[0];
            if (type != 'B')
            {
                var elemNumber = AnsiConsole.Ask<int>("How many orders per page? [springgreen1][[Recommended <5]][/]");
                AnsiConsole.Clear();

                while (input != "")
                {
                    NameValueCollection queryString = System.Web.HttpUtility.ParseQueryString(string.Empty);
                    queryString.Add("type", type.ToString());
                    queryString.Add("page", page.ToString());
                    queryString.Add("elem", elemNumber.ToString());

                    var response = client.GetAsync(string.Concat(string.Concat(baseuUrl, "?"), queryString)).GetAwaiter().GetResult();
                    var ordersString = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    var orders = JsonSerializer.Deserialize<IEnumerable<Order>>(ordersString);

                    var table = new Table()
                        .Border(TableBorder.Rounded)
                        .Centered()
                        .BorderColor(Color.Grey42);

                    // Add some columns
                    table.AddColumns(new TableColumn("[bold sandybrown]Number[/]").Centered(),
                        new TableColumn("[bold sandybrown]Status[/]").Centered(),
                        new TableColumn("[bold sandybrown]Pallets\nLpn | Item[/]").Centered());

                    foreach (var ord in orders)
                    {
                        table.AddRow(ord.Number ?? "[red]---[/]",
                            ord.Status.ToString() ?? "[red]---[/]",
                            ord.Pallets.Count.ToString() + ":");

                        var innerTable = new Table()
                            .Centered()
                            .Border(TableBorder.Ascii2)
                            .BorderColor(Color.LightSalmon1)
                            .AddColumns(new TableColumn("Lpn"), new TableColumn("Item"))
                            .HideHeaders();
                        foreach (var pallet in ord.Pallets)
                        {
                            innerTable.AddRow(pallet.Lpn ?? "[red]---[/]", pallet.Item ?? "[red]---[/]");
                        }

                        table.AddRow(new Markup(""), new Markup(""), innerTable);
                        table.AddEmptyRow();
                    }

                    // Render the table to the console
                    AnsiConsole.Write(table);
                    AnsiConsole.WriteLine(System.Environment.NewLine);
                    input = AnsiConsole
                        .Prompt(new TextPrompt<string>("Press A(<-) or D(->) to change page\nLeave it blank and press Enter to exit")
                        .AllowEmpty()
                        .Validate(input =>
                        {
                            return input switch
                            {
                                "" or "A" or "a" or "D" or "d" => ValidationResult.Success(),
                                _ => ValidationResult.Error("[red]Invalid input[/]")
                            };
                        }));
                    if (page > 1 && (input == "a" || input == "A"))
                        page--;
                    else if (input == "d" || input == "D")
                        page++;

                    AnsiConsole.Clear();
                }
            }
        }
    }
}
