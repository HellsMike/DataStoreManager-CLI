using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erp
{
    internal static class AwsClient
    {
        public static DynamoDBContext GetContext()
        {
            var dynamoDbConfig = new AmazonDynamoDBConfig
            {
                RegionEndpoint = RegionEndpoint.GetBySystemName("us-east-2")
            };

            var awsCredentials = new Cred(Environment.GetEnvironmentVariable("AWS_KEY") ?? string.Empty, Environment.GetEnvironmentVariable("AWS_SECRET") ?? string.Empty);
            var client = new AmazonDynamoDBClient(awsCredentials, dynamoDbConfig);
            return new DynamoDBContext(client);
        }
    }

    internal class Cred : AWSCredentials
    {
        public Cred(string key, string secret)
        {
            Key = key;
            Secret = secret;
        }

        private string Key { get; }
        private string Secret { get; }

        public override ImmutableCredentials GetCredentials()
        {
            return new ImmutableCredentials(Key,
                            Secret, null);
        }
    }
}