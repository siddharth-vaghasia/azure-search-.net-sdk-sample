using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace AZSD
{
    class Program
    {

       static string searchServiceName = "myazsearch2"; // ADD your Azure Search Service name
        static string adminApiKey = "***************"; // ADD your admin API key here
       static string queryApiKey = "*******************";  // ADD you query APU key here
        static string indexName = "superheroes";


        static void Main(string[] args)
        {
            SearchServiceClient serviceClient = CreateSearchServiceClient(searchServiceName, adminApiKey);

            Console.WriteLine("{0}", "Deleting index...\n");
            DeleteIndexIfExists(indexName, serviceClient);


            Console.WriteLine("{0}", "Creating index...\n");
            CreateIndex(indexName, serviceClient);

            Console.WriteLine("{0}", "Uploading documents...\n");
            ISearchIndexClient indexClient = serviceClient.Indexes.GetClient(indexName);
            UploadDocuments(indexClient);


            ISearchIndexClient indexClientForQueries = CreateSearchIndexClient(indexName, searchServiceName, queryApiKey);
            RunQueries(indexClientForQueries);
            Console.WriteLine("{0}", "Complete.  Press any key to end application...\n");
            Console.ReadKey();
        }

        private static SearchServiceClient CreateSearchServiceClient(string searchServiceName, string adminApiKey)
        {
            SearchCredentials creds = new SearchCredentials(adminApiKey);
            SearchServiceClient serviceClient = new SearchServiceClient(searchServiceName, creds);
            return serviceClient;
        }


        private static SearchIndexClient CreateSearchIndexClient(string indexName, string searchServiceName, string queryApiKey)
        {
            SearchIndexClient indexClient = new SearchIndexClient(searchServiceName, indexName, new SearchCredentials(queryApiKey));
            return indexClient;
        }
       
        // Delete an existing index to reuse its name  
        private static void DeleteIndexIfExists(string indexName, SearchServiceClient serviceClient)
        {
            
            if (serviceClient.Indexes.Exists(indexName))
            {
                serviceClient.Indexes.Delete(indexName);
            }
        }

        // Create an index whose fields correspond to the properties of the SuperHero class.  
        // The fields of the index are defined by calling the FieldBuilder.BuildForType() method.  
        private static void CreateIndex(string indexName, SearchServiceClient serviceClient)
        {
            var definition = new Index()
            {
                Name = indexName,
                Fields = FieldBuilder.BuildForType<SuperHero>()
            };
            serviceClient.Indexes.Create(definition);
            
        }

        // Upload documents as a batch  
        private static void UploadDocuments(ISearchIndexClient indexClient)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(@"D:\Projects\AZSD\SuperHeroes.xml");
            XmlNodeList nodeList = xmlDoc.DocumentElement.SelectNodes("/root/SuperHeros/SuperHero");
            List<SuperHero> SuperHeroCollections = new List<SuperHero>();
            var actions = new IndexAction<SuperHero>[nodeList.Count];
            int i = 0;
            foreach (XmlNode node in nodeList)
            {
                SuperHero SuperHero = new SuperHero();
                SuperHero.ID = node.SelectSingleNode("ID").InnerText;
                SuperHero.NAME = node.SelectSingleNode("NAME").InnerText;
                SuperHero.ScreenName = node.SelectSingleNode("ScreenName").InnerText;
                SuperHero.Power = node.SelectSingleNode("Power").InnerText;
                SuperHeroCollections.Add(SuperHero);
                actions[i] = IndexAction.Upload(SuperHero);
                
                i++;
            }
            IndexBatch<SuperHero> batch = IndexBatch.New(actions);
            try
            {
                indexClient.Documents.Index(batch);
            }
            catch (Exception e)
            {
                // When a service is under load, indexing might fail for some documents in the batch.   
                // Depending on your application, you can compensate by delaying and retrying.   
                // For this simple demo, we just log the failed document keys and continue.  
                Console.WriteLine("Failed to index some of the documents: {0}",
                    String.Join(", ", e.Message));
            }
            // Wait 2 seconds before starting queries  
            Console.WriteLine("Waiting for indexing...\n");
            Thread.Sleep(2000);
        }

        // Add query logic and handle results  
        private static void RunQueries(ISearchIndexClient indexClient)
        {
            SearchParameters parameters;
            DocumentSearchResult<SuperHero> results;

            // Query 1   
            Console.WriteLine("Query 1: Search for term 'batman', returning the full document");
            parameters = new SearchParameters();
            results = indexClient.Documents.Search<SuperHero>("batman", parameters);
            WriteDocuments(results);

            // Query 2  
            Console.WriteLine("Query 2: Search on the term 'fly', returning selected fields");
            Console.WriteLine("Returning only these fields: NAME, ScreenName, Power:\n");
            parameters =
                new SearchParameters()
                {
                    Select = new[] { "NAME", "ScreenName", "Power" }
                    
                };
            results = indexClient.Documents.Search<SuperHero>("fly", parameters);
            WriteDocuments(results);

            // Query 3  
            Console.WriteLine("Query 3: Search for the terms 'acrobat' and 'metahuman'");
            Console.WriteLine("Returning only these fields: NAME, ScreenName, Power:\n");
            parameters =
                new SearchParameters()
                {
                    Select = new[] { "NAME", "ScreenName", "Power" },
                };
            results = indexClient.Documents.Search<SuperHero>("acrobat, metahuman", parameters);
            WriteDocuments(results);

            // Query 4 
            Console.WriteLine("Query 4: Search based on filter experession");
            
            parameters =
                new SearchParameters()
                {
                    Filter = "ScreenName eq 'Aquaman' or NAME eq 'Victor Stone'"
                    
                };
            results = indexClient.Documents.Search<SuperHero>("", parameters);
            WriteDocuments(results);


        }

        // Handle search results, writing output to the console  
        private static void WriteDocuments(DocumentSearchResult<SuperHero> searchResults)
        {
            foreach (SearchResult<SuperHero> result in searchResults.Results)
            {
                Console.WriteLine("ID:--" + result.Document.ID);
                Console.WriteLine("Name:--" + result.Document.NAME);
                Console.WriteLine("ScreenName:--" + result.Document.ScreenName);
                Console.WriteLine("Power:--" + result.Document.Power);
            }
            Console.WriteLine();
        }
    }
}
