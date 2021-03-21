using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorAlerts
{
    public class BlazorAlertsService : IBlazorAlertsService
    {

        public event Action<string, string, PositionType, bool, string> OnAlert;

        /// <summary>
        ///    Returns a dynamic object with the values 'model' as a BlazorAlertsModel and 'confirmed' as a boolean.
        /// </summary>
        public event EventHandler<dynamic> OnAlertConfirmed;

        public List<BlazorAlertsModel> BlazorAlerts { get; set; } = new List<BlazorAlertsModel>();

        public BlazorAlertsService()
        {

        }

        public void NewBlazorAlert(string message, string heading = "[Javascript Application]", PositionType position = PositionType.Absolute, bool confirmDialog = false, string id = null)
        {
            this.OnAlert?.Invoke(message, heading, position, confirmDialog, id);
        }

        public void AddAlert(BlazorAlertsModel model)
        {
            if(!this.BlazorAlerts.Any(item => item.Id == model.Id))
            {
                this.BlazorAlerts.Add(model);
            }
        }

        public void RemoveAlert(string id)
        {
            BlazorAlertsModel item = this.BlazorAlerts.FirstOrDefault(item => item.Id == id);
            if(item != null)
            {
                this.BlazorAlerts.Remove(item);
            }
        }

        public void AlertConfirmed(BlazorAlertsModel model, bool confirmed)
        {
            dynamic obj = new ExpandoObject();
            obj.confirmed = confirmed;
            obj.model = model;

            this.OnAlertConfirmed.Invoke(this, obj);
        }

    }
}
