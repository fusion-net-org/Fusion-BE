using Fusion.Repository.ViewModels.Project;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Service.Commons.Helpers
{
    public static class MailUtils
    {
        public static string InviteMemberToCompany(string inviterName,string inviteeName,string companyName,string acceptUrl,string rejectUrl,int expire)
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
                                Click one of the buttons below to accept or reject this invitation.
                            </p>

                            <div style=""text-align:center;margin:30px 0;"">
                                <a href=""{acceptUrl}""
                                   style=""background:#2da44e;color:#fff;text-decoration:none;
                                          padding:12px 20px;border-radius:6px;font-weight:bold;
                                          display:inline-block;margin-right:10px;"">
                                    Join {companyName}
                                </a>

                                <a href=""{rejectUrl}""
                                   style=""background:#d73a49;color:#fff;text-decoration:none;
                                          padding:12px 20px;border-radius:6px;font-weight:bold;
                                          display:inline-block;"">
                                    Reject
                                </a>
                            </div>

                            <p style=""font-size:12px;color:#57606a"">
                                This invitation will expire in {expire} minute{(expire > 1 ? "s" : "")}.
                            </p>
                        </div>
                    </div>
                    <div style=""text-align:center;margin-top:15px;font-size:12px;color:#57606a"">
                        <p>© {DateTime.UtcNow.Year} {companyName}. All rights reserved.</p>
                    </div>
                </div>";
        }

        public static string FiredMemberFromCompany(string removerName,string removedMemberName,string companyName,string reason)
        {
            return $@"
<div style=""background-color:#f9f9f9;font-family:Arial,Helvetica,sans-serif;padding:20px"">
    <div style=""max-width:600px;margin:auto;background:#fff;border:1px solid #e1e4e8;border-radius:6px"">
        <div style=""padding:20px;text-align:center;border-bottom:1px solid #e1e4e8"">
            <img src=""https://cdn-icons-png.flaticon.com/512/1828/1828843.png"" 
                 alt=""logo"" style=""width:48px;height:48px;margin-bottom:10px""/>
            <h2 style=""margin:0;font-size:18px;color:#d73a49"">
                You have been removed from <b>{companyName}</b>
            </h2>
        </div>

        <div style=""padding:20px;color:#24292e;font-size:14px"">
            <p>Hi {removedMemberName},</p>
            <p>
                We would like to inform you that <b>{removerName}</b> has removed you from the 
                <b>{companyName}</b> organization.
            </p>

            <p style=""margin-top:20px;"">
                <b>Reason provided:</b><br/>
                <span style=""display:inline-block;background:#f6f8fa;
                               padding:10px 15px;border-radius:6px;
                               border:1px solid #e1e4e8;color:#24292e;"">
                    {reason}
                </span>
            </p>

            <p style=""margin-top:25px;color:#57606a;font-size:13px;"">
                If you believe this was a mistake or would like to discuss it, 
                you can contact the company admin using the link below:
            </p>

            <div style=""text-align:center;margin:25px 0;"">
                <a href=""http://localhost:5173/"" 
                   style=""background:#0969da;color:#fff;text-decoration:none;
                          padding:12px 20px;border-radius:6px;font-weight:bold;
                          display:inline-block"">
                    Contact Support
                </a>
            </div>

            <p style=""font-size:12px;color:#57606a"">
                This action was performed on {DateTime.UtcNow.AddHours(7):MMMM dd, yyyy HH:mm} (GMT+7).
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

        public static string CreateCloseProjectRequestEmail( string requesterName, CloseProjectSummaryDto summary, string projectName, string projectDetailUrl, string actionUrl)
        {
            var componentSection = BuildComponentTable(summary);

                        return $@"
            <div style=""background-color:#f6f8fa;font-family:Arial,Helvetica,sans-serif;padding:24px"">
                <div style=""max-width:640px;margin:auto;background:#ffffff;
                            border:1px solid #d0d7de;border-radius:10px"">

                    <!-- Header -->
                    <div style=""padding:20px;text-align:center;border-bottom:1px solid #d0d7de"">
                        <img src=""https://cdn-icons-png.flaticon.com/512/190/190411.png"" 
                             style=""width:52px;height:52px;margin-bottom:10px"" />
                        <h2 style=""margin:0;color:#24292f"">Project Close Request</h2>
                    </div>

                    <!-- Body -->
                    <div style=""padding:24px;color:#24292f;font-size:14px;line-height:1.6"">
                        <p>Hi <b>{requesterName}</b>,</p>

                        <p>
                            The project <b>{projectName}</b> has been requested to close.
                            Below is the current completion status:
                        </p>

                        <!-- Summary -->
                        <table style=""width:100%;border-collapse:collapse;margin:20px 0"">
                            <tr>
                                <td><b>Project Completion</b></td>
                                <td style=""color:#2da44e;font-weight:bold"">
                                    {summary.ProjectPercent}%
                                </td>
                            </tr>

                            <tr>
                                <td><b>Ticket Completion</b></td>
                                <td style=""color:#2da44e;font-weight:bold"">
                                    {summary.TicketPercent}%
                                </td>
                            </tr>

                            <tr>
                                <td><b>Total Tickets</b></td>
                                <td>{summary.TotalTickets}</td>
                            </tr>
                        </table>

                        {componentSection}

                        <p style=""margin-top:20px;color:#57606a"">
                            ⚠️ Some tasks or tickets may still be unfinished.
                            Please review carefully before accepting.
                        </p>

                        <!-- Actions -->
                        <div style=""text-align:center;margin:30px 0"">
                            <a href=""{projectDetailUrl}""
                               style=""background:#0969da;color:#fff;text-decoration:none;
                                      padding:12px 24px;border-radius:6px;font-weight:bold"">
                                View Project
                            </a>
                            <br/><br/>
                            <a href=""{actionUrl}""
                               style=""background:#2da44e;color:#fff;text-decoration:none;
                                      padding:12px 24px;border-radius:6px;font-weight:bold"">
                                Review Close Request
                            </a>
                        </div>
                    </div>
                </div>

                <div style=""text-align:center;margin-top:20px;font-size:12px;color:#57606a"">
                    © {DateTime.UtcNow.Year} Fusion Platform. All rights reserved.
                </div>
            </div>";
        }

        public static string CreateCloseProjectApprovedEmail(string projectName, string requesterName, string executorName, string projectUrl)
        {
            return $@"
<div style=""background-color:#f6f8fa;font-family:Arial,Helvetica,sans-serif;padding:24px"">
    <div style=""max-width:620px;margin:auto;background:#ffffff;
                border:1px solid #d0d7de;border-radius:10px"">

        <!-- Header -->
        <div style=""padding:20px;text-align:center;border-bottom:1px solid #d0d7de"">
            <img src=""https://cdn-icons-png.flaticon.com/512/190/190411.png""
                 style=""width:52px;height:52px;margin-bottom:10px"" />
            <h2 style=""margin:0;color:#2da44e"">
                Project Closed Successfully
            </h2>
        </div>

        <!-- Body -->
        <div style=""padding:24px;color:#24292f;font-size:14px;line-height:1.6"">
            <p>Hi <b>{executorName}</b>,</p>

            <p>
                Your request to close the project
                <b>""{projectName}""</b> has been
                <span style=""color:#2da44e;font-weight:bold"">approved</span>
                by <b>{requesterName}</b>.
            </p>

            <p>
                The project is now officially marked as
                <b style=""color:#cf222e"">Closed</b>.
                All related tasks and tickets have been finalized.
            </p>

            <div style=""text-align:center;margin:30px 0"">
                <a href=""{projectUrl}""
                   style=""background:#0969da;color:#fff;text-decoration:none;
                          padding:12px 26px;border-radius:6px;
                          font-weight:bold;display:inline-block"">
                    View Project Detail
                </a>
            </div>

            <p style=""font-size:13px;color:#57606a"">
                Thank you for using <b>Fusion Platform</b> to manage your projects efficiently.
            </p>
        </div>
    </div>

    <div style=""text-align:center;margin-top:20px;font-size:12px;color:#57606a"">
        <p>© {DateTime.UtcNow.Year} Fusion Platform. All rights reserved.</p>
    </div>
</div>";
        }

        public static string CreateCloseProjectRejectedEmail(
    string projectName,
    string requesterName,
    string executorName,
    string reasonReject,
    string projectUrl)
        {
            return $@"
<div style=""background-color:#f6f8fa;font-family:Arial,Helvetica,sans-serif;padding:24px"">
    <div style=""max-width:620px;margin:auto;background:#ffffff;
                border:1px solid #d0d7de;border-radius:10px"">

        <!-- Header -->
        <div style=""padding:20px;text-align:center;border-bottom:1px solid #d0d7de"">
            <img src=""https://cdn-icons-png.flaticon.com/512/190/190411.png""
                 style=""width:52px;height:52px;margin-bottom:10px"" />
            <h2 style=""margin:0;color:#cf222e"">
                Close Project Request Rejected
            </h2>
        </div>

        <!-- Body -->
        <div style=""padding:24px;color:#24292f;font-size:14px;line-height:1.6"">
            <p>Hi <b>{executorName}</b>,</p>

            <p>
                Your request to close the project
                <b>""{projectName}""</b> has been
                <span style=""color:#cf222e;font-weight:bold"">rejected</span>
                by <b>{requesterName}</b>.
            </p>

            <div style=""background:#fff8c5;border:1px solid #e3b341;
                        border-radius:6px;padding:14px;margin:20px 0"">
                <b>Reason provided:</b>
                <p style=""margin:8px 0 0;color:#24292f"">
                    {reasonReject}
                </p>
            </div>

            <p>
                The project remains in
                <b style=""color:#0969da"">Ongoing</b> status.
                You may update the project and submit a new close request if needed.
            </p>

            <div style=""text-align:center;margin:30px 0"">
                <a href=""{projectUrl}""
                   style=""background:#0969da;color:#fff;text-decoration:none;
                          padding:12px 26px;border-radius:6px;
                          font-weight:bold;display:inline-block"">
                    View Project Detail
                </a>
            </div>

            <p style=""font-size:13px;color:#57606a"">
                If you have questions, please contact the requester for clarification.
            </p>
        </div>
    </div>

    <div style=""text-align:center;margin-top:20px;font-size:12px;color:#57606a"">
        <p>© {DateTime.UtcNow.Year} Fusion Platform. All rights reserved.</p>
    </div>
</div>";
        }


        private static string BuildComponentTable(CloseProjectSummaryDto summary)
        {
            if (!summary.IsMaintenance || summary.Components == null || !summary.Components.Any())
                return "";

            var rows = string.Join("", summary.Components.Select(c => $@"
        <tr>
            <td style=""padding:6px 0"">{c.ComponentName}</td>
            <td style=""padding:6px 0"">{c.ClosedTasks}/{c.TotalTasks}</td>
            <td style=""padding:6px 0;color:#0969da;font-weight:bold"">
                {c.Percent}%
            </td>
        </tr>
    "));

            return $@"
        <h4 style=""margin-top:30px"">Component Progress</h4>
        <table style=""width:100%;border-collapse:collapse;margin-top:10px"">
            <tr style=""border-bottom:1px solid #d0d7de"">
                <th align=""left"" style=""padding-bottom:6px"">Component</th>
                <th align=""left"" style=""padding-bottom:6px"">Tasks</th>
                <th align=""left"" style=""padding-bottom:6px"">Completion</th>
            </tr>
            {rows}
        </table>";
        }

    }
}
