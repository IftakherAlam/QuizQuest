using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuizFormsApp.Models
{

    public class SalesforceSyncViewModel
{
    public string CompanyName { get; set; }
    public string JobTitle { get; set; }
    public string Phone { get; set; }
    public string Website { get; set; }
    public string Description { get; set; }
}



}