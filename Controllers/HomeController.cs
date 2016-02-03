using bible.Models;
using System;
using System.Data.Entity;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;
using bible.Classes;

namespace bible.Controllers
{
	public class HomeController : Controller
	{
		private readonly int OLD_TEST = 1;
		private readonly int NEW_TEST = 2;
		private readonly int APOCRYPHA = 3;
		private readonly Regex trimmer = new Regex(@"\s\s+");

		public ActionResult DownloadBible()
		{
			using(var db = new Entities())
			{
				TruncateAllTables(db);
				FillSectionTable(db);
				
				List<string> old_testament = new List<string>(){"1Ms", "2Ms", "3Ms", "4Ms", "5Ms",  "Jos", "Km", "Rt", "1Sm", "2Sm", "1Kn", "2Kn", "1Aj", "2Aj", "Esr", "Ne", "Est", "Ii", "Ps", "Õp", "Kg", "Ül", "Js", "Jr", "Nl", "Hs", "Tn", "Ho", "Jl", "Am", "Ob", "Jn", "Mi", "Na", "Ha", "Sf", "Hg", "Sk", "Ml"};
				List<string> new_testament = new List<string>(){"Mt", "Mk", "Lk", "Jh", "Ap", "Rm", "1Kr", "2Kr", "Gl", "Ef", "Fl", "Kl", "1Ts", "2Ts", "1Tm", "2Tm", "Tt", "Fm", "Hb", "Jk", "1Pt", "2Pt", "1Jh", "2Jh", "3Jh", "Jd", "Ilm"};
				List<string> apocrypha = new List<string>(){"Ju", "Trk", "Tb", "Srk", "Brk", "1Mak", "2Mak", "Erl", "Trl"};

				SaveSectionToDb(old_testament, OLD_TEST, Response);
				SaveSectionToDb(new_testament, NEW_TEST, Response);				
				SaveSectionToDb(apocrypha, APOCRYPHA, Response);				
				return Content("Done");
			}
		}

		private static void SaveSectionToDb(List<string> section, int section_id, HttpResponseBase response)
		{
			string url = String.Empty;
			int ch_count = 1;
			int current_ch = 1;
			XDocument xml = null;
			int chapter_id = 0;
			int book_id = 0;
			foreach (string abbrev in section)
			{
				response.Write("<br /><hr /><br />");
				while(current_ch <= ch_count)
				{
					using(var db = new Entities())
					{
						url = "http://piibel.net/.xml?q=" + abbrev + "%20" + current_ch;
						xml = XDocument.Load(url);
						if(current_ch == 1)
						{
							book_id = AddBook(section_id, xml);
							ch_count = Int32.Parse(xml.Descendants("bible").First().Attribute("pages").Value);
						}
						chapter_id = AddChapter(xml, book_id);
						AddVerses(xml, chapter_id);
						response.Write("Saved: " + abbrev + " " + current_ch + "<br />");
						response.Flush();
						current_ch++;
					}
				}
				current_ch = 1;
				ch_count = 1;
			}
		}

		private static void AddVerses(XDocument xml, int chapter_id)
		{
			using(var db = new Entities())
				{
				IEnumerable<XElement> verses = xml.Descendants("verse");
				foreach (XElement v in verses)
				{
					db.verse.Add(new verse()
					{
						chapter_id = chapter_id,
						number = Int32.Parse(v.Attribute("id").Value),
						heading = v.Attribute("heading").Value.Replace("<br />", "; ").Replace("„", "\"").Replace("”", "\""),
						content = StringTools.StripTagsCharArray(NormalizeWhiteSpace(v.Value.Replace("<br />", " ").Replace("„", "\"").Replace("”", "\"")))
					});
				}
				db.SaveChanges();
			}
		}

		private static int AddChapter(XDocument xml, int book_id)
		{
			using(var db = new Entities())
			{
				chapter chapter = new chapter();
				chapter.book_id = book_id;
				chapter.number = Int32.Parse(xml.Descendants("chapter").First().Attribute("id").Value);
				db.chapter.Add(chapter);
				db.SaveChanges();
				return chapter.id;
			}
		}

		private static int AddBook(int section_id, XDocument xml)
		{
			using(var db = new Entities())
			{
				book book = new book();
				book.name = xml.Descendants("book").First().Attribute("title").Value;
				book.name_short = xml.Descendants("book").First().Attribute("abbrev").Value;
				book.section_id = section_id;
				db.book.Add(book);
				db.SaveChanges();
				return book.id;
			}
		}

		private void FillSectionTable(Entities db)
		{
			db.section.Add(new section() { id = OLD_TEST, name = "Vana Testament" });
			db.section.Add(new section() { id = NEW_TEST, name = "Uus Testament" });
			db.section.Add(new section() { id = APOCRYPHA, name = "Apokrüüfid" });
			db.SaveChanges();
		}

		private static void TruncateAllTables(Entities db)
		{
			string sqlFile = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "truncate_all_tables.sql").First();
			string script = System.IO.File.ReadAllText(sqlFile).Replace(System.Environment.NewLine, " ");
			db.Database.ExecuteSqlCommand(script);
		}

		private static string NormalizeWhiteSpace(string input, char normalizeTo = ' ')
		{
			if (string.IsNullOrEmpty(input))
				return string.Empty;

			int current = 0;
			char[] output = new char[input.Length];
			bool skipped = false;

			foreach (char c in input.ToCharArray())
			{
				if (char.IsWhiteSpace(c))
				{
					if (!skipped)
					{
						if (current > 0)
							output[current++] = normalizeTo;

						skipped = true;
					}
				}
				else
				{
					skipped = false;
					output[current++] = c;
				}
			}

			return new string(output, 0, skipped ? current - 1 : current);
		}

		public ActionResult Bible()
		{
			using(var db = new Entities())
			{
				BibleModel model = new BibleModel();
				model.sections = db.section.ToList();
				model.books = db.book.ToList();
				model.chapters = db.chapter.ToList();
				model.verses = db.verse.ToList();
				return View(model);
			}
		}

		public ActionResult Index()
		{
			ViewBag.Message = "Modify this template to jump-start your ASP.NET MVC application.";

			return View();
		}

		public ActionResult About()
		{
			ViewBag.Message = "Your app description page.";

			return View();
		}

		public ActionResult Contact()
		{
			ViewBag.Message = "Your contact page.";

			return View();
		}
	}
}
