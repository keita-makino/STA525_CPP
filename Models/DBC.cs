using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace BTC.Models
{
    public class DBC : System.Data.Entity.DbContext
    {
        public DbSet<TradeRecord> TradeRecords { get; set; }
        public DbSet<Config> Configs { get; set; }
    }
}