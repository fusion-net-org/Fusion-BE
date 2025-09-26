using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Service.ViewModels.Users.Requests;

public record RegisterRequest
(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string ConfirmPassword
);