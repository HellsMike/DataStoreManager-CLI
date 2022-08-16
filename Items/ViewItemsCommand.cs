using Amazon.DynamoDBv2.DataModel;
using Amazon.Runtime;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Erp.Items
{
    internal class ViewItemsCommand : BaseCommand
    {
        private static readonly HttpClient client = new HttpClient();
        
        public override void Execute()
        {
            const string baseuUrl = "https://localhost:7219/item";
            string input = "start";
            var page = 1;
            var extend = false;
            var elemNumber = AnsiConsole.Ask<int>("How many items per page? [springgreen1][[Recommended <10]][/]");
            AnsiConsole.Clear();

            while (input != "")
            {
                NameValueCollection queryString = System.Web.HttpUtility.ParseQueryString(string.Empty);
                queryString.Add("page", page.ToString());
                queryString.Add("elem", elemNumber.ToString());

                var response = client.GetAsync(string.Concat(string.Concat(baseuUrl, "?"), queryString)).GetAwaiter().GetResult();
                var itemsString = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                var items = JsonSerializer.Deserialize<IEnumerable<Item>>(itemsString);
                
                var context = AwsClient.GetContext();
                var dynamoItems = context.ScanAsync<Item>(new List<ScanCondition>()).GetRemainingAsync().GetAwaiter().GetResult();

                var table = new Table()
                    .Border(TableBorder.Rounded)
                    .Centered()
                    .BorderColor(Color.Grey42);

                // Add some columns
                table.AddColumns(new TableColumn("[bold sandybrown]Name[/]").Centered(),
                    new TableColumn("[bold sandybrown]Description[/]").Centered(),
                    new TableColumn("[bold sandybrown]Pallets Count[/]").Centered());

                if (extend)
                {
                    table.AddColumns(new TableColumn("[bold sandybrown]Extended Description[/]").Centered(),
                        new TableColumn("[bold sandybrown]Dimensions (l*w*h)[/]").Centered(),
                        new TableColumn("[bold sandybrown]Weight[/]").Centered(),
                        new TableColumn("[bold sandybrown]Quantity[/]").Centered(),
                        new TableColumn("[bold sandybrown]Days To Expiry[/]").Centered(),
                        new TableColumn("[bold sandybrown]Default Shelf Life[/]").Centered(),
                        new TableColumn("[bold sandybrown]Customer Code[/]").Centered(),
                        new TableColumn("[bold sandybrown]Is Active?[/]").Centered());
                }

                foreach (var itm in items)
                {
                    if (extend)
                    {
                        table.AddRow(itm.Name ?? "[red]---[/]",
                         itm.Description ?? "[grey]---[/]",
                         dynamoItems.Where(x => x.Name == itm.Name).Select(x => x.PalletsCount).FirstOrDefault().ToString() ?? "[red]---[/]",
                         itm.ExtDescription ?? "[grey]---[/]",
                         $"{((float)itm.Length).ToString() ?? "[red]---[/]"} * {((float)itm.Width).ToString() ?? "[red]---[/]"} * {((float)itm.Height).ToString() ?? "[red]---[/]"}",
                         ((float)itm.NetWeight).ToString() ?? "[red]---[/]",
                         ((float)itm.Quantity).ToString() ?? "[grey]---[/]",
                         itm.DaysToExpiry.ToString() ?? "[grey]---[/]",
                         itm.DefaultShelfLife.ToString() ?? "[grey]---[/]",
                         itm.CustomerCode ?? "[grey]---[/]",
                         dynamoItems.Where(x => x.Name == itm.Name).Select(x => x.Active).FirstOrDefault().ToString() ?? "[red]---[/]");
                    }
                    else
                    {
                        table.AddRow(itm.Name ?? "[red]---[/]", 
                            itm.Description ?? "[grey]---[/]", 
                            dynamoItems.Where(x => x.Name == itm.Name).Select(x => x.PalletsCount).FirstOrDefault().ToString() ?? "[red]---[/]");
                    }
                    table.AddEmptyRow();
                }

                // Render the table to the console
                AnsiConsole.Write(table);
                AnsiConsole.WriteLine(System.Environment.NewLine);
                input = AnsiConsole
                    .Prompt(new TextPrompt<string>("Press A(<-) or D(->) to change page\nPress S to extend/reduce details\n" +
                    "Leave it blank and press Enter to exit")
                    .AllowEmpty()
                    .Validate(input =>
                    {
                        return input switch
                        {
                            "" or "S" or "s" or "A" or "a" or "D" or "d" => ValidationResult.Success(),
                            _ => ValidationResult.Error("[red]Invalid input[/]")
                        };
                    }));
                switch (input)
                {
                    case "A" or "a":
                        if (page > 1)
                            page--;
                        break;
                    case "D" or "d":
                        page++;
                        break;
                    case "S" or "s":
                        extend = !extend;
                        break;
                    default:
                        break;
                }

                AnsiConsole.Clear();
            }
        }
    }
}