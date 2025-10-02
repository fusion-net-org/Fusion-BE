using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Service.Commons.Helpers
{
    public static class MailUtils
    {
        public static string InviteMemberToCompany(string inviterName, string inviteeName, string companyName, string actionUrl, int expire)
        {
            return $@"
        <div style=""background-color:#f9f9f9;font-family:Arial,Helvetica,sans-serif;padding:20px"">
            <div style=""max-width:600px;margin:auto;background:#fff;border:1px solid #e1e4e8;border-radius:6px"">
                <div style=""padding:20px;text-align:center;border-bottom:1px solid #e1e4e8"">
                    <img src=""https://cdn-icons-png.flaticon.com/512/1828/1828461.png"" 
                         alt=""logo"" style=""width:48px;height:48px;margin-bottom:10px""/>
                    <h2 style=""margin:0;font-size:18px;color:#24292e"">
                        {inviterName} has invited you to join the <b>{companyName}</b> organization
                    </h2>
                </div>

                <div style=""padding:20px;color:#24292e;font-size:14px"">
                    <p>Hi {inviteeName},</p>
                    <p>
                        {inviterName} has invited you to join the <b>{companyName}</b> organization.<br/>
                        Click the button below to accept this invitation.
                    </p>

                    <div style=""text-align:center;margin:30px 0"">
                        <a href=""{actionUrl}"" 
                           style=""background:#2da44e;color:#fff;text-decoration:none;
                                  padding:12px 20px;border-radius:6px;font-weight:bold;
                                  display:inline-block"">
                            Join {companyName}
                        </a>
                    </div>

                    <p style=""font-size:12px;color:#57606a"">
                        This invitation will expire in {expire} minute.
                    </p>
                </div>
            </div>
            <div style=""text-align:center;margin-top:15px;font-size:12px;color:#57606a"">
                <p>© {DateTime.UtcNow.Year} {companyName}. All rights reserved.</p>
            </div>
        </div>";
        }
    }
}
