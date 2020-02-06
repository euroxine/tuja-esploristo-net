using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TujaEsploristoNet.Models;
using TujaEsploristoNet.Services;
using TujaEsploristoNet.ViewModels;

namespace TujaEsploristoNet.Controllers
{
	public class HomeController : Controller
	{
		private readonly ILogger<HomeController> _logger;
		private readonly ITujaServices tujaServices;
		private int HomeStat=0;
		public HomeController(ILogger<HomeController> logger, ITujaServices tujaServices)
		{
			_logger = logger;
			this.tujaServices = tujaServices;

		}

		public IActionResult Index()
		{
			TujaViewModels model = new TujaViewModels();
			string papath = HttpContext.Session.Get<string>("espdicPath");
			if (!string.IsNullOrWhiteSpace(papath))
				model.path = papath;
			else
				model.path = @"E:\tuja-vortaro";
			var momod = View(model);
			return momod;
		}
		[HttpPost]
		public IActionResult espdic(TujaViewModels model)
		{
			if (ModelState.IsValid)
			{
				HttpContext.Session.Set<string>("espdicPath", model.path);
				tujaServices.Krei(model.path, "espdic");
				ModelState.Clear();
			}
			return Created("", new { espdic = "espdic kreinta" });
		}
		public IActionResult etimologio(TujaViewModels model)
		{
			if (ModelState.IsValid)
			{
				HttpContext.Session.Set<string>("espdicPath", model.path);
				tujaServices.Krei(model.path, "etimologio");
				ModelState.Clear();
			}
			return RedirectToAction("Index", "Home");
		}
		public IActionResult difinoj(TujaViewModels model)
		{
			if (ModelState.IsValid)
			{
				HttpContext.Session.Set<string>("espdicPath", model.path);
				tujaServices.Krei(model.path, "difinoj");
				ModelState.Clear();
			}
			return RedirectToAction("Index", "Home");
		}
		public async Task<IActionResult> legu(TujaViewModels model)
		{
			if (ModelState.IsValid)
			{
				await tujaServices.KreiAsync(model.path, "legu");
				ModelState.Clear();
			}
			return RedirectToAction("Index", "Home");
		}
		public async Task<IActionResult> leguTest(TujaViewModels model)
		{
			if (ModelState.IsValid)
			{
				await tujaServices.KreiAsync(model.path, "leguTest");
				ModelState.Clear();
			}
			return RedirectToAction("Index", "Home");
		}
		[HttpGet("statutoJson")]
		public async Task<JsonResult> statutoJson(TujaViewModels model)
		{
			int stat = HttpContext.Session.Get<int>("utf8xml");
			//await Task<JsonResult>.Run(() => { });
			await utf8xml(model);
			return Json(new { path= model.path, stat=HomeStat } );			
		}
		[HttpGet("statutoHtml")]
		public async Task<IActionResult> statutoHtml(TujaViewModels model)
		{
			await utf8xml(model);
			return PartialView(@"statutoHtml", model);
		}
		[HttpPost("Pstatuto")]
		public IActionResult menfinjMarchePAs([FromBody]TujaViewModels model)
		{
			return Json(new { statuto = "espdic kreinta kun var =" + model.path });
		}

		[HttpPost("utf8xml")]
		public async Task<IActionResult> utf8xml(TujaViewModels model)
		{
			if (ModelState.IsValid)
			{
				HttpContext.Session.Set<string>("espdicPath", model.path);
				HttpContext.Session.Set<int>("utf8xml", 1);
				HomeStat = 1;
				//tujaServices.Krei(model.path, "utf8xml");
				await tujaServices.KreiAsync(model.path, "utf8xml");
				HttpContext.Session.Set<int>("utf8xml", 2);
				HomeStat = 2;
				ModelState.Clear();
			}
			return PartialView(@"statutoHtml", model);
			//return View();
		}

		[HttpPost("vortaroj")]
		public async Task<IActionResult> vortaroj(TujaViewModels model)
		{
			if (ModelState.IsValid)
			{
				HomeStat = 1;
				await tujaServices.KreiAsync(model.path, "vortaroj");
				HomeStat = 2;
				ModelState.Clear();
			}
			return PartialView(@"statutoHtml", model);
			//return View();
		}
		public IActionResult Privacy()
		{
			return View();
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}
