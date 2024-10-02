using MIPSDK_WebApp1.Models.Entities;

namespace MIPSDK_WebApp1.Models
{
    public class PolicyViewModel
    {
        public int Id { get; set; }
        public string PolicyName { get; set; }
        public string LabelName { get; set; }

        public List<MipLabel> Labels { get; set; }
        public string SelectedLabelId { get; set; }
    }
}
