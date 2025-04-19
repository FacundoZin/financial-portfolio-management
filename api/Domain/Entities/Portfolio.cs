using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace api.Domain.Entities
{
    [Table("Portfolios")]
    public class Portfolio
    {
        public int ID { get; set; }
        public string Name { get; set; } = string.Empty;

        public string AppUserID { get; set; }
        public AppUser AppUser { get; set; }

        public List<Holding> Holdings { get; set; } = new List<Holding>();
    }
}