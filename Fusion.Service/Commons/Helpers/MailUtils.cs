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

        public static string CreateCompanyThankYouEmail(string userName, string companyName, string landingUrl, string companyUrl)
        {
            return $@"
<div style=""background-color:#f9f9f9;font-family:Arial,Helvetica,sans-serif;padding:20px"">
    <div style=""max-width:600px;margin:auto;background:#fff;border:1px solid #e1e4e8;border-radius:8px"">
        <div style=""padding:20px;text-align:center;border-bottom:1px solid #e1e4e8"">
            <img src=""https://cdn-icons-png.flaticon.com/512/190/190411.png"" 
                 alt=""Fusion Logo"" style=""width:56px;height:56px;margin-bottom:10px""/>
            <h2 style=""margin:0;font-size:20px;color:#24292e"">
                Welcome to <b>Fusion</b> Platform!
            </h2>
        </div>

        <div style=""padding:25px;color:#24292e;font-size:14px;line-height:1.6"">
            <p>Hi {userName},</p>
            <p>
                Thank you for choosing <b>Fusion</b> — our intelligent multi-enterprise IT project management and collaboration platform.
                Your company <b>{companyName}</b> has been successfully created on our system.
            </p>

            <p>
                You can now start inviting members, creating projects, and exploring all the features Fusion provides to simplify maintenance and development across your teams.
            </p>

            <div style=""text-align:center;margin:30px 0"">
                <a href=""{landingUrl}"" 
                   style=""background:#2da44e;color:#fff;text-decoration:none;
                          padding:12px 24px;border-radius:6px;font-weight:bold;
                          display:inline-block;margin-bottom:12px"">
                    Go to Landing Page
                </a>
                <br/>
                <a href=""{companyUrl}""
                   style=""background:#0969da;color:#fff;text-decoration:none;
                          padding:10px 22px;border-radius:6px;font-weight:bold;
                          display:inline-block"">
                    View Company Info
                </a>
            </div>

            <div style=""text-align:center;margin-top:20px"">
                <a href=""http://localhost:5173/subscriptions"" 
                   style=""background:#bf8700;color:#fff;text-decoration:none;
                          padding:10px 22px;border-radius:6px;font-weight:bold;
                          display:inline-block;"">
                    Buy Subscription Plan
                </a>
                <p style=""font-size:12px;color:#57606a;margin-top:8px"">
                    Unlock more projects, members, and premium AI features with Fusion Pro.
                </p>
            </div>

            <p style=""font-size:13px;color:#57606a"">
                Need help? Visit our <a href=""http://localhost:5173/helper"" style=""color:#0969da;text-decoration:none;"">Support Center</a> 
                or contact our team anytime.
            </p>
        </div>
    </div>

    <div style=""text-align:center;margin-top:20px;font-size:12px;color:#57606a"">
        <p>© {DateTime.UtcNow.Year} Fusion Platform. All rights reserved.</p>
    </div>
</div>";
        }

    }
}
