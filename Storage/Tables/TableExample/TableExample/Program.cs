using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TableExample
{
    class Program
    {
        static void Main(string[] args)
        {
            try {
                // Retrieve the storage account from the connection string.
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                    CloudConfigurationManager.GetSetting("StorageConnectionString"));

                // Create the table client.
                CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

                // Retrieve a reference to the table.
                CloudTable table = tableClient.GetTableReference("people");

                // Create the table if it doesn't exist.
                table.CreateIfNotExists();

                //CustomerEntity customer1 = new CustomerEntity("Harp", "Walter");
                //customer1.Email = "Walter@contoso.com";
                //customer1.PhoneNumber = "425-555-0101";
                //TableOperation insertOperation = TableOperation.Insert(customer1);
                //table.Execute(insertOperation);

                // Create the table query.
                TableQuery<CustomerEntity> rangeQuery = new TableQuery<CustomerEntity>().Where(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "Harp"));

                // Loop through the results, displaying information about the entity.
                foreach (CustomerEntity entity in table.ExecuteQuery(rangeQuery))
                {
                    Console.WriteLine("{0}, {1}\t{2}\t{3}", entity.PartitionKey, entity.RowKey,
                        entity.Email, entity.PhoneNumber);
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
            Console.WriteLine("Done.");
            Console.ReadLine();
        }
    }

    public class CustomerEntity : TableEntity
    {
        public CustomerEntity(string lastName, string firstName)
        {
            this.PartitionKey = lastName;
            this.RowKey = firstName;
        }

        public CustomerEntity() { }

        public string Email { get; set; }

        public string PhoneNumber { get; set; }
    }
}
