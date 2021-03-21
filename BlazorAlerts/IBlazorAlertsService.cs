using System;

namespace BlazorAlerts
{
    public interface IBlazorAlertsService
    {

        void NewBlazorAlert(string message, string heading, PositionType position = PositionType.Fixed, bool confirmDialog = false, string id = null);

        void AddAlert(BlazorAlertsModel model);

        void RemoveAlert(string id);

    }
}
