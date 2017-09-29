using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Configuration;
using System.Net.Http.Headers;
using System.Collections.Generic;

namespace WebApiClientConsoleApp
{
    class Program
    {
        // This is the url of the api to call.
        private static string serviceUrl = ConfigurationManager.AppSettings["api:ValuesApiUrl"];
        private static bool[] EXPECTED_App1 = new bool[] { true, false, false, true, true };
        private static bool[] EXPECTED_App2 = new bool[] { false, true, false, true, true };
        private static bool[] EXPECTED_App3 = new bool[] { true, true, true, true, true };
        private static Dictionary<int, bool[]> EXPECTED_RESULTS = new Dictionary<int, bool[]>();
        private static Dictionary<int, string> SERVICE_URLS = new Dictionary<int, string>();

        static void Main(string[] args)
        {
     
            EXPECTED_RESULTS.Add(1, EXPECTED_App1);
            EXPECTED_RESULTS.Add(2, EXPECTED_App2);
            EXPECTED_RESULTS.Add(3, EXPECTED_App3);

            SERVICE_URLS.Add(1, ConfigurationManager.AppSettings["api:ProductsApiUrl"]);
            SERVICE_URLS.Add(2, ConfigurationManager.AppSettings["api:CustomersApiUrl"]);
            SERVICE_URLS.Add(3, ConfigurationManager.AppSettings["api:StockApiUrl"]);
            SERVICE_URLS.Add(4, ConfigurationManager.AppSettings["api:CompanyApiUrl"]);
            SERVICE_URLS.Add(5, ConfigurationManager.AppSettings["api:TextApiUrl"]);

            DoJobAsync().Wait();
        }

        private static string GetClientSecret(int appId)
        {
            string key = string.Format("client{0}", appId);
            string appSetting = ConfigurationManager.AppSettings[key];
            string[] appsettings = appSetting.Split(',');
            return appsettings[1];
        }

        private static string GetClientId(int appId)
        {
            string key = string.Format("client{0}", appId);
            string appSetting = ConfigurationManager.AppSettings[key];
            string[] appsettings = appSetting.Split(',');
            return appsettings[0];
        }

        private async static Task<AuthenticationResult> AuthenticateAsync()
        {
            
            // The client id is used to identify this console applications 'application' in Azure AD
            string clientID = ConfigurationManager.AppSettings["ida:ClientId"];
            Console.WriteLine("clientID: {0}", clientID);

            // The client secret is the secret code to prove that this is indeed the right console application (client generally)
            string clientSecret = ConfigurationManager.AppSettings["ida:ClientSecret"];
            Console.WriteLine("First part of clientSecret: {0}", clientSecret.Substring(0,10));

            // The authority is effectively just our tenant in AAD
            string authority = ConfigurationManager.AppSettings["ida:Authority"];
            Console.WriteLine("authority: {0}", authority);

            // The resource is the Web API we want to access (can be either the App Id URL or the Application Id of the API).
            // That is referred to as the ida:ClientID in the webapi web.config
            string resource = ConfigurationManager.AppSettings["api:AppIdUri"];
            Console.WriteLine("resource: {0}", resource);

            // Create a client credential to access the AAD Application that represents the console application
            var clientCredential = new ClientCredential(clientID, clientSecret);
            Console.WriteLine("ClientCredential created.");

            // Create a context that represents the authority
            var context = new AuthenticationContext(authority);
            Console.WriteLine("AuthenticationContext created for resource: {0}", resource);
            
            // Get an OAuth Bearer token wrapped in an AuthenticationResult
            var authenticationResult = await context.AcquireTokenAsync(
                    resource, clientCredential);
            
            return authenticationResult;
        }

        private async static Task<AuthenticationResult> AuthenticateAsync(int appId)
        {
            Console.WriteLine("Authenticating for App Id={0}", appId);
            // The client id is used to identify this console applications 'application' in Azure AD
            string clientID = GetClientId(appId);
            Console.WriteLine("clientID: {0}", clientID);

            // The client secret is the secret code to prove that this is indeed the right console application (client generally)
            string clientSecret = GetClientSecret(appId);
            Console.WriteLine("First part of clientSecret: {0}", clientSecret.Substring(0, 10));

            // The authority is effectively just our tenant in AAD
            string authority = ConfigurationManager.AppSettings["ida:Authority"];
            Console.WriteLine("authority: {0}", authority);

            // The resource is the Web API we want to access (can be either the App Id URL or the Application Id of the API).
            // That is referred to as the ida:ClientID in the webapi web.config
            string resource = ConfigurationManager.AppSettings["api:AppIdUri"];
            Console.WriteLine("resource: {0}", resource);

            // Create a client credential to access the AAD Application that represents the console application
            var clientCredential = new ClientCredential(clientID, clientSecret);
            Console.WriteLine("ClientCredential created.");

            // Create a context that represents the authority
            var context = new AuthenticationContext(authority);
            Console.WriteLine("AuthenticationContext created for resource: {0}", resource);

            // Get an OAuth Bearer token wrapped in an AuthenticationResult
            var authenticationResult = await context.AcquireTokenAsync(
                    resource, clientCredential);

            return authenticationResult;
        }

        private static async Task DoApplicationJobAsync(int appId)
        {
            string responseData = null;
            try
            {
                Console.WriteLine("Running under the context of Application #{0}", appId);
                await PrintExpectedResults(appId);
                // Get an OAuth Bearer token wrapped in an AuthenticationResult
                Console.WriteLine("Authenticating...");
                var authenticationResult = await AuthenticateAsync(appId);

                // Create an HttpClient and add the Bearer token to the request headers

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                        "Bearer", authenticationResult.AccessToken);
                    //Console.WriteLine("Adding Bearer: {0} to request header", authenticationResult.AccessToken);
                    for (int i = 1; i < 6; i++)
                    {
                        string uri = SERVICE_URLS[i];
                        Console.WriteLine("Calling {0} API {1}", (Api) appId, uri);
                        string expectation = "Unauthorized";
                        if (EXPECTED_RESULTS[appId][i-1]) expectation = "Success";
                        Console.WriteLine("Expectation is {0}", expectation);
                        var result = await client.GetAsync(uri);

                        if (result.IsSuccessStatusCode)
                        {
                            responseData = await result.Content.ReadAsStringAsync();
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("Api {0} was called sucessfully", SERVICE_URLS[i]);
                            Console.WriteLine("response from {0} was {1}", serviceUrl, responseData);
                            Console.ResetColor();
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("Error calling Api {0}. Status code was {1}", SERVICE_URLS[i], result.StatusCode);
                            Console.WriteLine("Message {0}", result.ReasonPhrase);
                            Console.ResetColor();
                        }
                    }
                    

                    
                    

                   
                }
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("An Exception occurred.  Message: {0}", e.Message);
                Console.ResetColor();
            }
        }

        private async static Task PrintExpectedResults(int appId)
        {
            
            Console.WriteLine("Application {0} clientId:{1} Expected Results", appId, GetClientId(appId));
            bool[] results = EXPECTED_RESULTS[appId];
            Console.WriteLine("Products Api: {0}", results[0]);
            Console.WriteLine("Customers Api: {0}", results[1]);
            Console.WriteLine("Stock Api: {0}", results[2]);
            Console.WriteLine("Company Api: {0}", results[3]);
            Console.WriteLine("Text Api: {0}", results[4]);
        }

        private async static Task DoJobAsync()
        {

            for (int i = 1; i < 4; i++)
            {
                await DoApplicationJobAsync(i);
            }
            Console.WriteLine("Press Enter to exit this console application.");
            Console.ReadLine();
        }

     private enum Api { products, customers, stock, company, text}

    }
}
