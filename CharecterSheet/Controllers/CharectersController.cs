using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CharecterEntities;
using CharecterExtensions;
using System.Net;

namespace CharecterSheet.Controllers
{
    public class CharactersController : Controller
    {
        // GET: Characters
        public ActionResult Index()
        {
            return View(new List<Character>().LoadAll());
        }

        // GET: Characters/Details/5
        public ActionResult Details(int? id)
        {
            ActionResult response;
            if (id == null)
            {
                response = new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            else
            {
                Character character = new Character().Load(id.Value);
                if (character == null)
                {
                    response = HttpNotFound();
                }
                else
                {
                    string sheetLocation = CharecterExtensions.Common.folderBase 
                        + "Character Sheet - Form Fillable.pdf";
                    character.GetPdf(false);
                    sheetLocation = CharecterExtensions.Common.folderBase
                        + "Character Sheet - Alternative - Form Fillable.pdf";
                    character.GetPdf(true);
                    response = View(character);
                }
            }
            return response;
        }

        public ActionResult CharecterSheet(int? id, bool isAlt = false)
        {
            ActionResult response;
            if (id == null)
            {
                response = new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            else
            {
                Character character = new Character().Load(id.Value);
                if (character == null)
                {
                    response = HttpNotFound();
                }
                else
                {
                    string charecterSheet = character.GetPdf(isAlt);
                    FileStreamResult result = new FileStreamResult(new System.IO.FileStream(charecterSheet, System.IO.FileMode.Open), "application/pdf");
                    result.FileDownloadName = charecterSheet.Split('\\').Last();
                    response = result;
                }
            }
            return response;
        }

        // GET: Characters/Create
        public ActionResult Create()
        {
            return View(new Character());
        }

        // POST: Characters/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Character character)
        {
            if (ModelState.IsValid)
            {
                character.Save();
                return RedirectToAction("Index");
            }
            return View(character);
        }

        // GET: Characters/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Character character = new Character().Load(id.Value);
            if (character == null)
            {
                return HttpNotFound();
            }
            return View(character);
        }

        // POST: Characters/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Character character)
        {
            ActionResult returnView;
            var invalidKeys = character.ClassLevels.Keys.Where(x => string.IsNullOrWhiteSpace(x)).ToList();
            var duplicateKeys = character.ClassLevels.Keys.GroupBy(x => x).Where(x => x.Count() > 1).Select(x => x.Key).ToList();
            character.Languages = character.Languages.First().Split(new string[] { ",", Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToList();
            character.OtherProfiencies = character.OtherProfiencies.First().Split(new string[] { ",", Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToList();
            character.Items.RemoveAll(x => x.Count == 0);
            if (invalidKeys.Any())
            {
                foreach (string invalidKey in invalidKeys)
                {
                    character.ClassLevels.Remove(invalidKey);
                }
                character.Save();
                returnView = View(character);
            }
            else
            {
                if (ModelState.IsValid)
                {
                    character.Save();
                    returnView = View(character);
                }
                else
                {
                    returnView = View(character);
                }
            }
            return returnView;
        }

        // GET: Characters/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Character character = new Character().Load(id.Value);
            if (character == null)
            {
                return HttpNotFound();
            }
            return View(character);
        }

        // POST: Characters/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Character character = new Character().Load(id);
            character.Delete();
            return RedirectToAction("Index");
        }
    }
}