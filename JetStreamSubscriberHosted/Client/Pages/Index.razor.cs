using JetStreamSubscriberHosted.Client.Models;
using JetStreamSubscriberHosted.Client.Services;
using Microsoft.AspNetCore.Components;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace JetStreamSubscriberHosted.Client.Pages
{
    public partial class Index
    {
        #region Services
        [Inject]
        private IStreamService streamService { get; set; }
        #endregion

        #region Properties
        private List<string>? streamNames { get; set; }
        private ObservableCollection<Dropdown>? subjectNames { get; set; }
        private ObservableCollection<Dropdown>? consumerNames { get; set; }
        private string? selectedStreamName { get; set; }
        private string? selectedSubjectName { get; set; }
        private string? selectedConsumerName { get; set; }
        #endregion

        #region Functions

        protected override async Task OnInitializedAsync()
        {
            await loadData();
        }

        private async Task loadData()
        {
            streamNames = (await streamService.GetStreamNames()).ToList();
            subjectNames = new ObservableCollection<Dropdown>();
            consumerNames = new ObservableCollection<Dropdown>();
        }
        private async Task getSubjects()
        {
            subjectNames.Clear();
            consumerNames.Clear();
            (await streamService.GetSubjectNames(selectedStreamName)).ToList().ForEach(steram =>
            {
                subjectNames.Add(new Dropdown { Text = steram, Value = steram });
            });
        }
        private async Task getConsumers()
        {
            subjectNames.Clear();
            consumerNames.Clear();
            (await streamService.GetConsumerNames(selectedStreamName)).ToList().ForEach(consumer =>
            {
                consumerNames.Add(new Dropdown { Text = consumer, Value = consumer });
            });
        }
        #endregion

    }
}
