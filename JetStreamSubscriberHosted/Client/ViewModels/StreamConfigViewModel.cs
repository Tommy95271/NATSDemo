using System.ComponentModel.DataAnnotations;

namespace JetStreamSubscriberHosted.Client.ViewModels
{
    public class StreamConfigViewModel
    {
        [Display(Name = "Stream 名稱")]
        public string StreamName { get; set; }
        [Display(Name = "儲存類型")]
        public Enums.StorageType StorageType { get; set; }
        [Display(Name = "要訂閱的主題")]
        public List<SubjectViewModel> Subjects { get; set; }
    }
}
