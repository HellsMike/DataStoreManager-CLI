using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Orders
{
    internal class ChangeOrderStatusCommand : BaseCommand
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
                var statuses = Enum.GetValues(typeof(OrderStatus)).Cast<OrderStatus>();
                statuses = statuses.Where(x => x != OrderStatus.Created);
                var status = AnsiConsole
                    .Prompt(new SelectionPrompt<OrderStatus>()
                    .Title("[bold lightseagreen]Order[/]")
                    .AddChoices(statuses));
                AnsiConsole.Status().Spinner(Spinner.Known.Dots).StartAsync("[bold sandybrown]Editing...[/]", _ => ChangeStatus(orderNumber, status)).Wait();
            }
        }

        private async Task ChangeStatus(string orderNumber, OrderStatus status)
        {
            using var context = AwsClient.GetContext();
            var conditions = new List<ScanCondition>();
            conditions.Add(new ScanCondition("Number", ScanOperator.Equal, orderNumber));
            var orders = await context.ScanAsync<Order>(conditions).GetRemainingAsync();
            var order = orders.First();
            order.Status = status;

            var response = new HttpResponseMessage();
            switch (order.Status)
            {
                case OrderStatus.Active:
                    response = await client.GetAsync($"https://localhost:7219/order/{order.Number}/active");
                    break;
                case OrderStatus.Suspended:
                    response = await client.GetAsync($"https://localhost:7219/order/{order.Number}/suspend");
                    break;
                case OrderStatus.Closed:
                    response = await client.GetAsync($"https://localhost:7219/order/{order.Number}/close");
                    break;
                case OrderStatus.Deleted:
                    response = await client.GetAsync($"https://localhost:7219/order/{order.Number}/delete");
                    break;
            }
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
                await context.SaveAsync(order);
        }
    }
}
