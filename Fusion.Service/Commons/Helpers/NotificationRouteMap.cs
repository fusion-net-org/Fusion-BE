using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Service.Commons.Helpers
{
    public static class NotificationRouteMap
    {
        // Mỗi key có thể có Web hoặc Mobile hoặc cả hai
        public static readonly Dictionary<string, (string? Web, string? Mobile)> Routes = new()
        {
            ["HOME_PAGE"] = ("/company", "/(tabs)/home"),
            ["COMPANY_DETAIL_PAGE"] = ("/company/{id}", null),
            ["PROJECT_DETAIL"] = ("/company/project/{id}", null),
            ["PARTNER_PAGE"] = ("/company/{id}/partners", null),
            ["MEMBER_PAGE"] = ("/company/{id}/members", null),
            ["PROJECT_REQUEST_PAGE"] = ("/company/{id}/project-request", null),
            ["TASK_DETAIL_PAGE"] = ("/companies/:companyId/project/:projectId/task/:taskId", null),
        };

        /// <summary>
        /// Resolve route key thành đường dẫn cụ thể.
        /// Nếu có id → thay {id}, nếu không có thì giữ nguyên route.
        /// </summary>
        public static (string? Web, string? Mobile) Resolve(
           string key,
           Guid? id = null)
        {
            if (!Routes.TryGetValue(key, out var route))
                return (null, null);

            string ReplaceId(string? text) =>
                string.IsNullOrEmpty(text) ? null :
                (id.HasValue ? text.Replace("{id}", id.ToString()) : text.Replace("/{id}", ""));

            return (ReplaceId(route.Web), ReplaceId(route.Mobile));
        }

    }
}
