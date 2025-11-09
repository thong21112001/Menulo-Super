using Microsoft.AspNetCore.SignalR;

namespace Menulo.Infrastructure.RealTime
{
    public class TableHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            if (httpContext != null)
            {
                var connectionId = Context.ConnectionId;

                // 1. Logic cho nhóm RestaurantId
                if (httpContext.Request.Query.TryGetValue("restaurantId", out var restaurantVals)
                    && int.TryParse(restaurantVals.FirstOrDefault(), out var restId))
                {
                    // Tên group khớp với hubclient.js
                    await Groups.AddToGroupAsync(connectionId, $"restaurant_{restId}");
                }

                // 2. Logic cho nhóm TableId
                if (httpContext.Request.Query.TryGetValue("tableId", out var tableVals)
                    && int.TryParse(tableVals.FirstOrDefault(), out var tableId))
                {
                    // Tên group khớp với hubclient.js
                    await Groups.AddToGroupAsync(connectionId, $"table_{tableId}");
                }
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
        }
    }
}
