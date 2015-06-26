using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace LottoGather
{
    public class MegaMillionsModel
    {
        [JsonProperty(PropertyName="draw_date")]
        public DateTime Date { get; set; }

        [JsonProperty(PropertyName = "winning_numbers")]
        public String Numbers { get; set; }

        [JsonProperty(PropertyName = "mega_ball")]
        public int MB { get; set; }

        [JsonProperty(PropertyName = "multiplier")]
        public int MN { get; set; }
    }
}
