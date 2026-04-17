using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CFMS_WPF.Data
{
	public class CaseMetaData 
	{
		public int documentid { get; set; }
		public int caseId { get; set; }
		public string CaseFileName { get; set; }
		public string CaseFilePath { get; set; }
		public string CaseUploadedBy { get; set; }
		public DateTime CaseUploadAt { get; set; }

		public CaseFile Case {  get; set; }

	}
}
