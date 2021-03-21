using Microsoft.AspNetCore.Components;
using System;

namespace BlazorAlerts
{
    public class BlazorAlertsBase : ComponentBase, IDisposable
    {

        [Inject]
        public BlazorAlertsService BlazorAlertsService { get; set; }

        public BlazorAlertsBase() 
        {
            
        }

        protected override void OnInitialized()
        {
            this.BlazorAlertsService.OnAlert += OnAlertExecute;
        }

        public async void OnAlertExecute(string message, string heading, PositionType position, bool confirmDialog, string id)
        {
            await InvokeAsync(() =>
            {
                BlazorAlertsModel alert = new BlazorAlertsModel()
                {
                    Id = !string.IsNullOrEmpty(id) ? id : Guid.NewGuid().ToString(),
                    Message = message,
                    Headline = heading,
                    Position = position,
                    ConfirmDialog = confirmDialog,
                    CreatedOn = DateTime.Now
                };

                this.BlazorAlertsService.AddAlert(alert);
                StateHasChanged();
            });
        }

        public void CloseAlert_OnClicked(string id)
        {
            this.BlazorAlertsService.RemoveAlert(id);
        }

        public void ConfirmAlert_OnClicked(BlazorAlertsModel model)
        {
            this.BlazorAlertsService.RemoveAlert(model.Id);
            if(model.ConfirmDialog)
            {
                this.BlazorAlertsService.AlertConfirmed(model, true);
            }
        }

        public void DenyAlert_OnClicked(BlazorAlertsModel model)
        {
            this.BlazorAlertsService.RemoveAlert(model.Id);
            if (model.ConfirmDialog)
            {
                this.BlazorAlertsService.AlertConfirmed(model, false);
            }
        }

        public void Dispose()
        {
            this.BlazorAlertsService.OnAlert -= OnAlertExecute;
        }

    }
}
