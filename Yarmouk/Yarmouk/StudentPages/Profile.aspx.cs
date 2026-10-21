using System;
using System.Web.UI;
using Yarmouk.Models;
namespace Yarmouk.StudentPages
{
    public partial class Profile : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Simple MODEL example (sample data only; no Firebase request).
                Student student = new Student();
                student.StudentId = "20260001";
                student.MajorId = "CIS";
                lblStudentId.Text = student.StudentId;
                lblMajor.Text = student.MajorId;
                // Later: call StudentService with verified signed-in user and ID token.
            }
        }
    }
}
