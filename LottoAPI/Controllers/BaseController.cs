using LottoGather;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;

namespace LottoAPI.Controllers
{
    public class BaseController : ApiController
    {
        protected readonly DbSet<Powerball_Numbers> pbContext = new LottyDBEntities().Powerball_Numbers;
        protected readonly DbSet<MegaMillions_Numbers> mmContext = new LottyDBEntities().MegaMillions_Numbers;
    }

    public enum LottoTypes
    {
        Powerball = 0,
        MegaMillions = 1,
    }
}