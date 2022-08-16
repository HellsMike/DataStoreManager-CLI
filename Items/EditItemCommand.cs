using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Items
{
    public enum ParameterCommands
    {
        Name,
        CustomerCode,
        DaysToExpiry,
        DefaultShelfLife,
        Description,
        ExtDescription,
        Height,
        Length,
        Width,
        NetWeight,
        Quantity,
        Active,
        Back
    }

    internal class EditItemCommand : BaseCommand
    {
        public override void Execute()
        {
            BaseCommand command = null;
            var conditions = new List<ScanCondition>();
            using var context = AwsClient.GetContext();
            var items = context.ScanAsync<Item>(conditions).GetRemainingAsync().Result;

            var names = items.Select(x => x.Name).ToList();
            names.Add("Back");
            var name = AnsiConsole
                .Prompt(new SelectionPrompt<string>()
                .Title("[bold lightseagreen]Item To Edit[/]")
                .AddChoices(names));

            if (name != "Back")
            {
                var item = items.Where(x => x.Name == name).First();

                var result = AnsiConsole
                    .Prompt(new SelectionPrompt<ParameterCommands>()
                    .Title("[bold lightseagreen]Field To Edit[/]")
                    .AddChoices(Enum.GetValues(typeof(ParameterCommands)).Cast<ParameterCommands>()));

                switch (result)
                {
                    case ParameterCommands.Name:
                    case ParameterCommands.CustomerCode:
                    case ParameterCommands.Description:
                    case ParameterCommands.ExtDescription:
                        command = new FieldEditCommand<string>(item, result.ToString());
                        break;

                    case ParameterCommands.Length:
                    case ParameterCommands.Height:
                    case ParameterCommands.Width:
                    case ParameterCommands.NetWeight:
                        command = new FieldEditCommand<decimal>(item, result.ToString());
                        break;

                    case ParameterCommands.DaysToExpiry:
                    case ParameterCommands.DefaultShelfLife:
                    case ParameterCommands.Quantity:
                        command = new FieldEditCommand<int>(item, result.ToString());
                        break;

                    case ParameterCommands.Active:
                        command = new FieldEditCommand<bool>(item, result.ToString());
                        break;

                    case ParameterCommands.Back:
                        break;
                }

                if (command is not null)
                    command.Execute();
            }
        }
    }
   
    internal class FieldEditCommand<T> : BaseCommand
    {
        private static readonly HttpClient client = new HttpClient();
        private  Item _item;
        private readonly string _field;
        public FieldEditCommand (Item item, string field)
        {
            _item = item;
            _field = field;
        }

        public override void Execute()
        {
            var value = AnsiConsole.Ask<T>("New Value: ");

            if (_field == "Active" && (bool)(object)value == true)
            {
                AnsiConsole.WriteLine("You cannot re-active a dismiss object");
                AnsiConsole.Prompt(new TextPrompt<string>("Press enter to exit").AllowEmpty());
                AnsiConsole.Clear();
            }
            else
            {
                AnsiConsole.Status().Spinner(Spinner.Known.Dots).StartAsync("[bold sandybrown]Editing...[/]", _ => EditItem(_item, _field, value)).Wait();
            }
        }

        private async Task EditItem(Item item, string field, T value)
        {
            using var context = AwsClient.GetContext();
            switch (field)
            {
                case "Name":
                    context.DeleteAsync<Item>(item).Wait();
                    item.Name = (string)(object)value;
                    break;
                case "CustomerCode":
                    item.CustomerCode = (string)(object)value;
                    break;
                case "Description":
                    item.Description = (string)(object)value;
                    break;
                case "ExtDescription":
                    item.Description = (string)(object)value;
                    break;
                case "Height":
                    item.Height = (decimal)(object)value;
                    break;
                case "Length":
                    item.Length = (decimal)(object)value;
                    break;
                case "Width":
                    item.Width = (decimal)(object)value;
                    break;
                case "Weight":
                    item.NetWeight = (decimal)(object)value;
                    break;
                case "DaysToExpiry":
                    item.DaysToExpiry = (int)(object)value;
                    break;
                case "DefaultShelfLife":
                    item.DefaultShelfLife = (int)(object)value;
                    break;
                case "Quantity":
                    item.Quantity = (int)(object)value;
                    break;
                case "Active":
                    item.Active = (bool)(object)value;
                    break;
            }

            var response = new HttpResponseMessage();
            if (field == "Active")
            {
                response = await client.GetAsync($"https://localhost:7219/item/{item.Name}/dismiss");
            }
            else
            {
                var body = new ItemEvent
                {
                    Data = new List<Item>(1) { item },
                    Versioning = DateTime.Now,
                };
                response = await client.PostAsJsonAsync("https://localhost:7219/item", body);
            }
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
                await context.SaveAsync(item);
        }
    }
}
 