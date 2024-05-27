using System.Data;
using System.Data.SqlClient;

namespace AutoParts
{
    public partial class Form1 : Form
    {
        DataTable dt = new DataTable();
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
        private void AddAdmin(SqlConnection CN)
        {
            // Get the values from the textboxes
            string adminName = Aname.Text;
            string adminCC = Acc.Text;
            string adminBirth = Abirth.Text;
            string adminAddr = Aaddr.Text;
            string adminCp = Acp.Text;
            string adminSalary = Asal.Text;
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

        private void Abutton_Click(object sender, EventArgs e)
        {
            //SqlConnection CN = GetDbConnection();
            //AddAdmin(CN);
        }

        //CUSTOMER CODE
        private void Contact_Load(object sender, EventArgs e)
        {
            dt.Columns.Add("Tipo", typeof(string));
            dt.Columns.Add("Contacto", typeof(string));
            ClientContactData.DataSource = dt;
        }

        private void Caddc_Click(object sender, EventArgs e)
        {
            if (Cdrop.Text == "" || Ccontact.Text == "")
            {
                MessageBox.Show("Por favor preencha todos os campos.");
                return;
            }
            else if (Cdrop.Text == "Email" && !Ccontact.Text.Contains("@"))
            {
                MessageBox.Show("Por favor insira um email válido.");
                return;
            }
            else if (Cdrop.Text == "Telemóvel" && Ccontact.Text.Length != 9 && !Ccontact.Text.StartsWith("9"))
            {
                MessageBox.Show("Por favor insira um número de telemóvel válido.");
                return;
            }
            else
            {
                dt.Rows.Add(Cdrop.Text, Ccontact.Text);
            }
        }
        private void AddCustomer(SqlConnection CN)
        {
            // Get the values from the textboxes
            string customerName = Cnome.Text;
            string customerCC = Ccc.Text;
            string customerBirth = Cbirth.Text;
            string customerAddr = Caddr.Text;
            string customerCp = Ccp.Text;
            string customerContacts = "";
            foreach (DataRow row in dt.Rows)
            {
                customerContacts += row["Tipo"].ToString() + ": " + row["Contacto"].ToString() + "|";
            }

        }

        private void Cbutton_Click(object sender, EventArgs e)
        {
            //SqlConnection CN = GetDbConnection();
            //AddCustomer(CN);
        }
    }
}
