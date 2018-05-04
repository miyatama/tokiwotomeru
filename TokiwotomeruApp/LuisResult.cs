using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TokiwotomeruApp
{
    class LuisResult
    {

      public string query{get; set;}
      public TopScoringIntent topScoringIntent{get; set;}
      public Entities[] entities {get; set;}
    }
}
