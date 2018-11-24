using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace BTC.Models
{
    public class TradeRecord
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long id { get; set; }
        public long timestamp { get; set; }
        public double last { get; set; }
        public double ShortTrend_CC { get; set; }
        public double LongTrend_CC { get; set; }
        public double last_BT { get; set; }
        public double Before_BT { get; set; }
        public double After_BT { get; set; }
        public double ShortSlope_BT { get; set; }
        public int BchStatus { get; set; }
    }

    public class PositionList
    {
        public List<Position> data { get; set; }
    }

    public class Position
    {
        public string side { get; set; }
        public string status { get; set; }
        public int id { get; set; }
        public double amount { get; set; }
    }
    public class Config
    {
        public int Id { get; set; }
        public bool Switch { get; set; }
        public double Threshold_High { get; set; }
        public double Threshold_Low { get; set; }
        public double Multiplier { get; set; }

    }
}