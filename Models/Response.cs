using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Chat.Api.Models
{
    ///<summary>
    ///App wide JSON Response model
    /// </summary>
    public class Response
    {
        public bool Status { get; set; }
        public Object Data { get; set; }

        public Response()
        {
            Status = true; //successful resultset
        }

    }
}
