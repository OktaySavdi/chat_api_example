using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Chat.Api.Models
{
    public class ElasticLogItem
    {
         public string Level { get; set; }
        
        [Text(Name = "log_message")]
        public string Log_Message { get; set; }
        
        [Text(Name = "timestamp")]
        public string TimeStamp { get; set; }
              

    }
}
