using CFMS_WPF.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CFMS_WPF
{
	public class CaseFile
	{
		public int caseId { get; set; }
		public string caseYear { get; set; }
		public string caseType { get; set; }
		public string caseNo { get; set; }
		public string caseSubject { get; set; }
		public string caseNature {  get; set; }
		public string caseAgent { get; set; }
		public string caseComplainant { get; set; }
		public string caseStatus { get; set; }

		public CaseMetaData Document { get; set; }
	}
}
