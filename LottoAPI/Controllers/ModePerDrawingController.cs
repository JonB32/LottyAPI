using LottoAPI.Models;
using LottoGather;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace LottoAPI.Controllers
{
    public class ModePerDrawingsController : BaseController
    {
        // GET api/ModePerDrawings
        public SixNumberModel GetModeByDrawings(int lottoType)
        {
            SixNumberModel model = new SixNumberModel();
            IEnumerable<SixNumberModel> context;

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
            else if(lottoType == (int)LottoTypes.MegaMillions)
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

            // Get mode of n1
            var groupN1 = context.GroupBy(g => g.n1);
            int maxCountN1 = groupN1.Max(g => g.Count());
            int modeN1 = groupN1.First(g => g.Count() == maxCountN1).Key;
            model.n1 = modeN1;

            // Get mode of n2
            var groupN2 = context.GroupBy(g => g.n2);
            int maxCountN2 = groupN2.Max(g => g.Count());
            int modeN2 = groupN2.First(g => g.Count() == maxCountN2).Key;
            model.n2 = modeN2;

            // Get mode of n3
            var groupN3 = context.GroupBy(g => g.n3);
            int maxCountN3 = groupN3.Max(g => g.Count());
            int modeN3 = groupN3.First(g => g.Count() == maxCountN3).Key;
            model.n3 = modeN3;

            // Get mode of N4
            var groupN4 = context.GroupBy(g => g.n4);
            int maxCountN4 = groupN4.Max(g => g.Count());
            int modeN4 = groupN4.First(g => g.Count() == maxCountN4).Key;
            model.n4 = modeN4;

            // Get mode of n5
            var groupN5 = context.GroupBy(g => g.n5);
            int maxCountN5 = groupN5.Max(g => g.Count());
            int modeN5 = groupN5.First(g => g.Count() == maxCountN5).Key;
            model.n5 = modeN5;

            // Get mode of pb
            var groupPB = context.GroupBy(g => g.n6);
            int maxCountPB = groupPB.Max(g => g.Count());
            int modePB = groupPB.First(g => g.Count() == maxCountPB).Key;
            model.n6 = modePB;

            return model;
        }
    }
}
