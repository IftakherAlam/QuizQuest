using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace QuizFormsApp.Services
{
    public class SalesforceService
    {
        private readonly IConfiguration _config;
        private readonly HttpClient _client;

        public SalesforceService(IConfiguration config)
        {
            _config = config;
            _client = new HttpClient();
        }

       public async Task<(string accessToken, string instanceUrl)> AuthenticateAsync()
{
    var values = new Dictionary<string, string>
    {
        { "grant_type", "password" },
        { "client_id", _config["Salesforce:ClientId"] },
        { "client_secret", _config["Salesforce:ClientSecret"] },
        { "username", _config["Salesforce:Username"] },
        { "password", _config["Salesforce:Password"] + _config["Salesforce:SecurityToken"] }
    };

    var response = await _client.PostAsync($"{_config["Salesforce:LoginUrl"]}/services/oauth2/token", new FormUrlEncodedContent(values));
    var content = await response.Content.ReadAsStringAsync();

    if (!response.IsSuccessStatusCode)
    {
        // Log the full error from Salesforce
        throw new Exception($"Salesforce authentication failed. Status Code: {response.StatusCode}. Response: {content}");
    }

    var result = JsonSerializer.Deserialize<Dictionary<string, object>>(content);

    return (result["access_token"].ToString(), result["instance_url"].ToString());
}
        public async Task<string> CreateAccountAsync(string accessToken, string instanceUrl, string name)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var data = new { Name = name };
            var response = await _client.PostAsJsonAsync($"{instanceUrl}/services/data/v57.0/sobjects/Account/", data);
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<Dictionary<string, object>>(content);
            return result["id"].ToString();
        }

        public async Task CreateContactAsync(string accessToken, string instanceUrl, string lastName, string email, string accountId)
        {
            var data = new
            {
                LastName = lastName,
                Email = email,
                AccountId = accountId
            };
            await _client.PostAsJsonAsync($"{instanceUrl}/services/data/v57.0/sobjects/Contact/", data);
        }

        public async Task<Dictionary<string, object>> GetContactByEmailAsync(string accessToken, string instanceUrl, string email)
{
    _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

    var query = $"SELECT Id, FirstName, LastName, Email, AccountId FROM Contact WHERE Email = '{email}'";
    var response = await _client.GetAsync($"{instanceUrl}/services/data/v57.0/query/?q={Uri.EscapeDataString(query)}");
    var content = await response.Content.ReadAsStringAsync();
    var result = JsonSerializer.Deserialize<Dictionary<string, object>>(content);

    return result;
}

    }
}
