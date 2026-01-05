using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ClinicManagement_API.Contracts
{
    public record ApiResponse<T>(bool IsSuccess, string Message, T? Data);

}