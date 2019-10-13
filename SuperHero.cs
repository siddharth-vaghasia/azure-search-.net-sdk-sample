using Microsoft.Azure.Search;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AZSD
{
    class SuperHero
    {

        [System.ComponentModel.DataAnnotations.Key]
        [IsFilterable]
        public string ID { get; set; }

        [IsSearchable, IsFilterable, IsSortable, IsFacetable]
        public string NAME { get; set; }

        [IsSearchable, IsFilterable, IsSortable, IsFacetable]
        public string ScreenName { get; set; }

        [IsSearchable, IsFilterable, IsSortable, IsFacetable]
        public string Power { get; set; }
    }
}
