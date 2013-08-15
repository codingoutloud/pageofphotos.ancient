﻿using MediaRepository;
using PoP.WebTier.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PoP.WebTier.Controllers
{
    public class PageController : Controller
    {
        //
        // GET: /Page/

        public ActionResult Index(string slug)
        {
            if (String.IsNullOrEmpty(slug))
                return RedirectToAction("Index", "Home");

            var pageModel = GetFakePageModel(slug);

            return View(pageModel);
        }

#if false
        public ActionResult Index()
        {
            return View();
        }
#endif

      private PageModel GetFakePageModel(string slug)
      {
         var m1 = new ImageRepresentation()
         {
            DisplaySize = new Size(14, 20),
            MimeType = "image/jpeg",
            Url = "https://pop.blob.core.windows.net/photos/perfect_beer.jpg",
            Title = "Mmmmm... Beer"
         };
         var m2 = new ImageRepresentation()
         {
            MimeType = "image/jpeg",
            Url = "https://pop.blob.core.windows.net/photos/perfect_beer.jpg",
            Title = "Dublin Beer"
         };
         var m3 = new ImageRepresentation()
         {
            DisplaySize = new Size(34, 30),
            MimeType = "image/jpeg",
            Url = "https://pop.blob.core.windows.net/photos/perfect_beer.jpg",
            Title = "Da Beer"
         };
         var mediaRepresentationList = new List<MediaRepresentation> { m1, m2, m3 };
         var fakePageModel = new PageModel()
         {
            Name = slug,
            Description = "Pages for " + slug,
            Media = mediaRepresentationList
         };

         return fakePageModel;
      }

// TODO: re-enable      [Authorize]
      [HttpGet]
      public ActionResult Upload()
      {
         return View();
      }

  // TODO: re-enable    [Authorize]
      [HttpPost]
      public ActionResult Upload(IEnumerable<HttpPostedFileBase> files)
      {
         foreach (var file in files.Where(file => file != null && file.ContentLength > 0))
         {
            Contract.Assert(file.FileName == Path.GetFileName(file.FileName)); // browsers should not send path info - but synthetic test could
               
#if false
   //var path = Path.Combine(Server.MapPath("~/App_Data/uploads"), file.FileName);
               var path = Path.Combine("e:/temp/uploads", file.FileName);
               file.SaveAs(path);
#else
            AzureStorageHelper.CaptureUploadedMedia(file.InputStream, file.FileName, file.ContentType, file.ContentLength);
#endif
         }
         return RedirectToAction("Upload");
      }


      //
      // GET: /Page/Details/5

      public ActionResult Details(int id)
      {
         return View();
      }

      //
      // GET: /Page/Create

      public ActionResult Create()
      {
         return View();
      }

      //
      // POST: /Page/Create

      [HttpPost]
      public ActionResult Create(FormCollection collection)
      {
         try
         {
            // TODO: Add insert logic here

            return RedirectToAction("Index");
         }
         catch
         {
            return View();
         }
      }

      //
      // GET: /Page/Edit/5

      public ActionResult Edit(int id)
      {
         return View();
      }

      //
      // POST: /Page/Edit/5

      [HttpPost]
      public ActionResult Edit(int id, FormCollection collection)
      {
         try
         {
            // TODO: Add update logic here

            return RedirectToAction("Index");
         }
         catch
         {
            return View();
         }
      }

      //
      // GET: /Page/Delete/5

      public ActionResult Delete(int id)
      {
         return View();
      }

      //
      // POST: /Page/Delete/5

      [HttpPost]
      public ActionResult Delete(int id, FormCollection collection)
      {
         try
         {
            // TODO: Add delete logic here

            return RedirectToAction("Index");
         }
         catch
         {
            return View();
         }
      }
   }
}