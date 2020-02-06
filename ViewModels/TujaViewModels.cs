using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TujaEsploristoNet.ViewModels
{
	public class TujaViewModels
	{
		[Required]
		[MinLength(3)]
		public string path { get; set; }

	}
}
