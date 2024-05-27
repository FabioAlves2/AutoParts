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
            if (dt.Columns.Count == 0)
            {
                dt.Columns.Add("Tipo", typeof(string));
                dt.Columns.Add("Contacto", typeof(string));
                ClientContactData.DataSource = dt;
            }
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
            else if (Cdrop.Text == "Telemóvel" && (Ccontact.Text.Length != 9 || !Ccontact.Text.StartsWith("9")))
            {
                MessageBox.Show("Por favor insira um número de telemóvel válido.");
                return;
            }
            else
            {
                if (dt.Rows.Count == 0)
                {
                    dt.Rows.Add(Cdrop.Text, Ccontact.Text);
                }
                else if (dt.Rows.Count == 1)
                {
                    if (dt.Rows[0]["Tipo"].ToString() == Cdrop.Text)
                    {
                        MessageBox.Show("Já inseriu um contacto do mesmo tipo.");
                        return;
                    }
                    else
                    {
                        dt.Rows.Add(Cdrop.Text, Ccontact.Text);
                    }
                }
                else
                {
                    MessageBox.Show("Só pode inserir dois contactos.");
                    return;
                }
            }
        }
        private void AddClient(SqlConnection CN)
        {
            // Get the values from the textboxes
            string customerName = Cnome.Text;
            string customerCC = Ccc.Text;
            string customerBirth = Cbirth.Text;
            string customerAddr = Caddr.Text;
            string customerCp = Ccp.Text;
            string customerContacts = "";
            string contact0 = string.Empty;
            string contact1 = string.Empty;
            string? customerEmail = null;
            string? customerTelm = null;
            foreach (DataRow row in dt.Rows)
            {
                customerContacts += row["Tipo"].ToString() + ": " + row["Contacto"].ToString() + "|";
            }
            if (customerContacts.Contains("Email") && customerContacts.Contains("Telemóvel"))
            {
                contact0 = customerContacts.Split("|")[0];
                contact1 = customerContacts.Split("|")[1];
            }
            else if (customerContacts == "")
            {
                MessageBox.Show("O cliente deve ter pelo menos um contacto!");
                return;
            }
            else
            {
                contact1 = customerContacts.Split("|")[0];
            }

            if (contact0.StartsWith("Email"))
            {
                customerEmail = contact0.Split(":")[1];
            }
            else if (contact0.StartsWith("Telemóvel"))
            {
                customerTelm = contact0.Split(":")[1];
            }
            if (contact1.StartsWith("Email"))
            {
                customerEmail = contact1.Split(":")[1];
            }
            else if (contact1.StartsWith("Telemóvel"))
            {
                customerTelm = contact1.Split(":")[1];
            }

            // Query to insert the customer
            string query = "INSERT INTO Customers (Name, CC, Birth, Address, PostalCode, Email, Telm) VALUES (@Name, @CC, @Birth, @Address, @PostalCode, @Contacts)";

            using (SqlCommand cmd = new SqlCommand(query, CN))
            {
                cmd.Parameters.AddWithValue("@Name", customerName);
                cmd.Parameters.AddWithValue("@CC", customerCC);
                cmd.Parameters.AddWithValue("@Birth", customerBirth);
                cmd.Parameters.AddWithValue("@Address", customerAddr);
                cmd.Parameters.AddWithValue("@PostalCode", customerCp);
                cmd.Parameters.AddWithValue("@Email", customerEmail);
                cmd.Parameters.AddWithValue("@Telm", customerTelm);
                cmd.Parameters.AddWithValue("@Contacts", customerContacts);

                try
                {
                    if (CN.State == System.Data.ConnectionState.Closed)
                    {
                        CN.Open();
                    }

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Customer added successfully.");
                        //ClientAdd_Clear(sender, e);
                    }
                    else
                    {
                        MessageBox.Show("An error occurred while adding the customer.");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred: " + ex.Message);
                }
            }

        }
        private void ClientAdd_Clear(object sender, EventArgs e)
        {
            Cnome.Text = "";
            Ccc.Text = "";
            Cbirth.Text = "";
            Caddr.Text = "";
            Ccp.Text = "";
            Cdrop.Text = "";
            Ccontact.Text = "";
            dt.Clear();
        }

        private void Cbutton_Click(object sender, EventArgs e)
        {
            //SqlConnection CN = GetDbConnection();
            //AddClient(CN);
        }

    }
}
