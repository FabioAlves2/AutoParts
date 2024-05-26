using System.Data.SqlClient;

namespace AutoParts
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        //Connect to the database
        private SqlConnection GetDbConnection()
        {
            string connectionString = "FALTA LIGAR";
            SqlConnection CN = new SqlConnection(connectionString);
            return CN;
        }


        //ADMIN CODE
        private void button1_Click(object sender, EventArgs e, SqlConnection CN)
        {
            // Get the values from the textboxes
            string adminName = textBox1.Text;
            string adminCC = textBox2.Text;
            string adminBirth = textBox3.Text;
            string adminAddr = textBox4.Text;
            string adminCp = textBox5.Text;
            string adminSalary = textBox6.Text;
            string adminStart = monthCalendar1.SelectionStart.ToShortDateString();

            // Query to insert the admin
            string query = "INSERT INTO Admins (Name, CC, Birth, Address, PostalCode, Salary, StartDate) VALUES (@Name, @CC, @Birth, @Address, @PostalCode, @Salary, @StartDate)";

            using (SqlCommand cmd = new SqlCommand(query, CN))
            {
                cmd.Parameters.AddWithValue("@Name", adminName);
                cmd.Parameters.AddWithValue("@CC", adminCC);
                cmd.Parameters.AddWithValue("@Birth", adminBirth);
                cmd.Parameters.AddWithValue("@Address", adminAddr);
                cmd.Parameters.AddWithValue("@PostalCode", adminCp);
                cmd.Parameters.AddWithValue("@Salary", adminSalary);
                cmd.Parameters.AddWithValue("@StartDate", adminStart);

                try
                {
                    if (CN.State == System.Data.ConnectionState.Closed)
                    {
                        CN.Open();
                    }

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Admin added successfully.");
                    }
                    else
                    {
                        MessageBox.Show("An error occurred while adding the admin.");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred: " + ex.Message);
                }
            }
        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        //CUSTOMER CODE
    }
}
