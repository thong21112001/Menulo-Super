using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Newtonsoft.Json;

namespace Menulo.Extensions
{
    public static class TempDataExtensions
    {
        // Sử dụng một class nhỏ để đóng gói thông tin alert
        public class Alert
        {
            public string Message { get; set; } = string.Empty;
            public string Icon { get; set; } = "info"; // Mặc định là info
        }

        private const string AlertKey = "AppAlert"; // Khóa chung để lưu alert

        public static void SetAlert(this ITempDataDictionary tempData, string message, string icon)
        {
            var alert = new Alert { Message = message, Icon = icon };
            tempData[AlertKey] = JsonConvert.SerializeObject(alert);
        }

        public static Alert? GetAlert(this ITempDataDictionary tempData)
        {
            if (tempData.TryGetValue(AlertKey, out object? value))
            {
                if (value is string alertJson)
                {
                    // Lỗi ở đây nếu alertJson là null
                    return JsonConvert.DeserializeObject<Alert>(alertJson);
                }
            }
            return null;
        }

        // Các phương thức tiện ích cho từng loại icon cụ thể
        public static void SetSuccess(this ITempDataDictionary tempData, string message)
        {
            tempData.SetAlert(message, "success");
        }

        public static void SetError(this ITempDataDictionary tempData, string message)
        {
            tempData.SetAlert(message, "error");
        }

        public static void SetWarning(this ITempDataDictionary tempData, string message)
        {
            tempData.SetAlert(message, "warning");
        }

        public static void SetInfo(this ITempDataDictionary tempData, string message)
        {
            tempData.SetAlert(message, "info");
        }
    }
}
