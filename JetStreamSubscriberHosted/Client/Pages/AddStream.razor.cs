using JetStreamSubscriberHosted.Client.Services;
using JetStreamSubscriberHosted.Client.ViewModels;
using JetStreamSubscriberHosted.Shared.Enums;
using Microsoft.AspNetCore.Components;

namespace JetStreamSubscriberHosted.Client.Pages
{
    public partial class AddStream
    {
        private StreamConfigViewModel stream { get; set; }
        private StorageType? storageType { get; set; }
        private List<StorageTypeViewModel>? storageTypes { get; set; } = new List<StorageTypeViewModel>();


        [Inject]
        private IStreamService streamService { get; set; }

        protected override async Task OnInitializedAsync()
        {
            stream = new StreamConfigViewModel()
            {
                StreamName = "teststeam1",
                StorageType = Enums.StorageType.File,
                Subjects = new List<SubjectViewModel> { new SubjectViewModel() { Text = "subject1", Value = "subject1" }, new SubjectViewModel() { Text = "subject5", Value = "subject5" } }
            };
            foreach (StorageType item in Enum.GetValues(typeof(StorageType)))
            {
                storageTypes.Add(new StorageTypeViewModel { Text = item.ToString(), Value = item });
            }
        }

        private async Task HandleValidSubmit()
        {
            var result = await streamService.AddStream(stream);


        }

        private async Task HandleInvalidSubmit()
        {
        }
    }
}
