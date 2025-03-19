using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelLayer.Model
{
    public class ResetPasswordModel
    {
        public string Email { get; set; }
        public string NewPassword { get; set; }
    }
}
