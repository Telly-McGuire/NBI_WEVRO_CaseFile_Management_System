using CFMS_WPF.Data;

namespace CFMS_WPF
{
	public class CaseFile
	{
		public int caseId { get; set; }
		public string caseYear { get; set; }
		public string caseType { get; set; }
		public string caseNo { get; set; }
		public string caseSubject { get; set; }
		public string caseNature { get; set; }
		public string caseAgent { get; set; }
		public string caseComplainant { get; set; }
		public string caseStatus { get; set; }

		public CaseMetaData Document { get; set; }
	}
}
