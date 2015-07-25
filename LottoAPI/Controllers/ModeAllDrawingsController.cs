using LottoAPI.Models;
using LottoGather;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace LottoAPI.Controllers
{
    public class ModeAllDrawingsController : BaseController
    {
        // GET api/ModeAllDrawings/0
        public SixNumberModel GetModeByAll(int lottoType)
        {
            IEnumerable<SixNumberModel> context;
            List<int> allDrawings = new List<int>();
            SixNumberModel model = new SixNumberModel();

            if (lottoType == (int)LottoTypes.Powerball)
            {
                context = pbContext.Select(s => new SixNumberModel()
                {
                    n1 = s.N1,
                    n2 = s.N2,
                    n3 = s.N3,
                    n4 = s.N4,
                    n5 = s.N5,
                    n6 = s.PB,
                });
            }
            else if (lottoType == (int)LottoTypes.MegaMillions)
            {
                context = mmContext.Select(s => new SixNumberModel()
                {
                    n1 = s.N1,
                    n2 = s.N2,
                    n3 = s.N3,
                    n4 = s.N4,
                    n5 = s.N5,
                    n6 = s.MB,
                });
            }
            else
            {
                //shouldn't get here
                return null;
            }

            foreach(var record in context)
            {
                allDrawings.Add(record.n1);
                allDrawings.Add(record.n2);
                allDrawings.Add(record.n3);
                allDrawings.Add(record.n4);
                allDrawings.Add(record.n5);
            }

            //get top 5 drawings
            var groupDrawing = allDrawings.GroupBy(g => g);
            var groupMode = groupDrawing.GroupBy(g => g.Count()).OrderByDescending(o => o.Key);
            model.n1 = groupMode.ElementAt(0).First().Key;
            model.n2 = groupMode.ElementAt(1).First().Key;
            model.n3 = groupMode.ElementAt(2).First().Key;
            model.n4 = groupMode.ElementAt(3).First().Key;
            model.n5 = groupMode.ElementAt(4).First().Key;

            //get powerball separately
            // Get mode of pb
            var groupPB = context.GroupBy(g => g.n6);
            int maxCountPB = groupPB.Max(g => g.Count());
            int modePB = groupPB.First(g => g.Count() == maxCountPB).Key;
            model.n6 = modePB;

            return model;
        }
    }
}
