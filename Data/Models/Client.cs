using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Models
{
    public class Client
    {
        public int Id { get; set; }
        public string ClientId { get; set; }
        public string ClientSecretHash { get; set; }
        public string AllowedScopes { get; set; } // CSV list
    }
}
