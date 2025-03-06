using RestSharp;
using System.Text;
using Newtonsoft.Json;
using QuizFormsApp.Models;

namespace QuizFormsApp.Services
{
    public class JiraService
    {
        private readonly JiraSettings _settings;
        private readonly RestClient _client;

        public JiraService(IConfiguration configuration)
        {
            _settings = configuration.GetSection("JiraSettings").Get<JiraSettings>();
            _client = new RestClient(_settings.BaseUrl);
        }

        private RestRequest CreateRequest(string resource, Method method)
        {
            var request = new RestRequest(resource, method);
            var authToken = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_settings.Email}:{_settings.ApiToken}"));
            request.AddHeader("Authorization", $"Basic {authToken}");
            request.AddHeader("Accept", "application/json");
            request.AddHeader("Content-Type", "application/json");
            return request;
        }

        public async Task<string> CreateTicketAsync(string summary, string priority, string template, string pageLink, string reporterEmail)
        {
            var request = CreateRequest("/rest/api/3/issue", Method.Post);

            var body = new
            {
                fields = new
                {
                    project = new { key = _settings.ProjectKey },
                    summary = summary,
                    issuetype = new { name = "Ticket" },
                    priority = new { name = priority },
                    // Replace with your actual custom field IDs
                    customfield_10054 = template,   // ✅ Template field
                    customfield_10055 = pageLink,   // ✅ Page Link (URL) field
                    customfield_10056 = reporterEmail                }
            };

            request.AddJsonBody(body);

            var response = await _client.ExecuteAsync(request);

            if (!response.IsSuccessful)
            {
                throw new Exception($"Failed to create ticket: {response.Content}");
            }

            var responseData = JsonConvert.DeserializeObject<dynamic>(response.Content);
            return $"{_settings.BaseUrl}/browse/{responseData.key}";
        }

   public async Task<List<JiraTicket>> GetUserTicketsAsync(string reporterEmail, int startAt = 0, int maxResults = 10)
{
    Console.WriteLine($"🔍 Starting Jira ticket search for email: {reporterEmail}");

    var request = CreateRequest("/rest/api/3/search", Method.Get);

    var jql = $"cf[10056] ~ \"{reporterEmail}\" ORDER BY created DESC";
    Console.WriteLine($"🔍 Using JQL: {jql}");

    request.AddQueryParameter("jql", jql);
    request.AddQueryParameter("startAt", startAt.ToString());
    request.AddQueryParameter("maxResults", "50");

    var response = await _client.ExecuteAsync(request);

    if (!response.IsSuccessful)
    {
        Console.WriteLine($"❌ Jira API failed: {response.StatusCode} - {response.Content}");
        throw new Exception($"Jira error: {response.Content}");
    }

    Console.WriteLine($"✅ Jira API Success: {response.Content}");

    var responseData = JsonConvert.DeserializeObject<JiraSearchResponse>(response.Content);

    var ticketCount = responseData.Issues?.Count ?? 0;
    Console.WriteLine($"✅ Total tickets found: {ticketCount}");

    return responseData.Issues.Select(issue => new JiraTicket
    {
        Key = issue.Key,
        Summary = issue.Fields.Summary,
        Status = issue.Fields.Status.Name,
        Link = $"{_settings.BaseUrl}/browse/{issue.Key}"
    }).ToList();
}

}

    }
