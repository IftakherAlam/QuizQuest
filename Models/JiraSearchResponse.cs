using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace QuizFormsApp.Models
{
    public class JiraSearchResponse
    {
        [JsonProperty("issues")]
        public List<JiraIssue> Issues { get; set; }
    }

    public class JiraIssue
    {
        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("fields")]
        public JiraIssueFields Fields { get; set; }
    }

    public class JiraIssueFields
    {
        [JsonProperty("summary")]
        public string Summary { get; set; }

        [JsonProperty("status")]
        public JiraIssueStatus Status { get; set; }
    }

    public class JiraIssueStatus
    {
        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
