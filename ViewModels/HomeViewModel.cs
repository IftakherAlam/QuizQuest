using System.Collections.Generic;
using QuizFormsApp.Models;

namespace QuizFormsApp.ViewModels
{
    public class HomeViewModel
    {
        public List<Template> LatestTemplates { get; set; } = new List<Template>();
        public List<Template> TopTemplates { get; set; } = new List<Template>();
        public List<Tag> PopularTags { get; set; } = new List<Tag>();
    }
}
