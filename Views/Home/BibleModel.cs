using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace bible.Models
{
	public class BibleModel
	{
		public List<section> sections {get; set;}
		public List<book> books {get; set;}
		public List<chapter> chapters {get; set;}
		public List<verse> verses {get; set;}
	}
}