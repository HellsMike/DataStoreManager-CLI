using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Runtime;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Erp.Items
{
    internal class LoadItemsCommand : BaseCommand
    {
        private static readonly HttpClient client = new HttpClient();
        public override void Execute()
        {
            var path = @"C:\Users\bianco.m\Downloads\dati.txt"; // AnsiConsole.Ask<string>("File Path?");
            using (StreamReader str = new StreamReader(path))
            {
                var items = str.ReadToEnd();
                var d = JsonDocument.Parse(items);
                var data = JsonSerializer.Deserialize<IEnumerable<Item>>(d.RootElement.GetProperty("Data"));

                var body = new ItemEvent
                {
                    Data = data,
                    Versioning = DateTime.Now
                };
                var response = client.PostAsJsonAsync("https://localhost:7219/item", body).Result;
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    using var context = AwsClient.GetContext();
                    foreach (var item in data)
                        context.SaveAsync(item).Wait();
                }
            }
        }
    }
}