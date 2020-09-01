using IdentityModel.Client;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;

namespace Client
{
    class Program
    {
        static async System.Threading.Tasks.Task Main(string[] args)
        {
            string re= Console.ReadLine();
            if (re == "123")
            {
                var client = new HttpClient();
                var disco = await client.GetDiscoveryDocumentAsync("https://localhost:5001");
                if (disco.IsError)
                {
                    Console.WriteLine(disco.Error);
                   
                    return;
                   
                }
                var tokenResponse = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
                {
                    Address = disco.TokenEndpoint,

                    ClientId = "client",
                    ClientSecret = "secret",
                    Scope = "api1"
                });

                if (tokenResponse.IsError)
                {
                    Console.WriteLine(tokenResponse.Error);
                    return;
                }

                Console.WriteLine(tokenResponse.Json);
             
                var apiClient = new HttpClient();
                apiClient.SetBearerToken(tokenResponse.AccessToken);

                var response = await apiClient.GetAsync("https://localhost:6001/identity");
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine(response.StatusCode);
                    Console.ReadKey();
                }
                else
                {
                    var content = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(JArray.Parse(content));
                    Console.ReadKey();
                }
            }
           
            
        }
    }
}
