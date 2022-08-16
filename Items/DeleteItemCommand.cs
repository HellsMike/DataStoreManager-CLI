using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Items
{
    internal class DeleteItemCommand : BaseCommand
    {
        private static readonly HttpClient client = new HttpClient();
        public override void Execute()
        {
            var conditions = new List<ScanCondition>();
            using var context = AwsClient.GetContext();
            var Taskresult = context.ScanAsync<Item>(conditions).GetRemainingAsync().Result;

            var names = Taskresult.Select(x => x.Name).ToList();
            names.Add("Back");
            var name = AnsiConsole
                .Prompt(new SelectionPrompt<string>()
                .Title("[red]Item To Edit[/]")
                .AddChoices(names));

            if (name != "Back")
            {
                AnsiConsole.Status().Spinner(Spinner.Known.Dots).StartAsync("[bold sandybrown]Deleting...[/]", _ => DeleteItem(name)).Wait();
            }
        }

        private async Task DeleteItem (string name)
        {
            var conditions = new List<ScanCondition>();
            using var context = AwsClient.GetContext();
            conditions.Add(new ScanCondition("Name", ScanOperator.Equal, name));
            var item = context.ScanAsync<Item>(conditions).GetRemainingAsync().Result.First();
            
            var response = client.GetAsync($"https://localhost:7219/item/{name}/delete").Result;
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
                await context.DeleteAsync(item);
        }
    }
}
