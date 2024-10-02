using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MIPSDK_WebApp1.Models.Entities
{
    public class DataPolicy
    {
        public enum PolicyDirection
        {
            Upload,
            Download
        }
        public int ID { get; set; }

        [Display(Name = "Policy Name")]
        public string PolicyName { get; set; } = string.Empty;

        public PolicyDirection Direction { get; set; }

        [Display(Name = "Label Id for Action")]
        public string MinLabelIdForAction { get; set; } = string.Empty;

    }
}
