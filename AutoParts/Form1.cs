using System.Data;
using System.Data.SqlClient;

namespace AutoParts
{
    public partial class Form1 : Form
    {
        DataTable dt = new DataTable();
        DataTable dt2 = new DataTable();
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
            Cdrop.SelectedIndex = -1;
            Ccontact.Text = "";
            dt.Clear();
        }

        private void Cbutton_Click(object sender, EventArgs e)
        {
            //SqlConnection CN = GetDbConnection();
            //AddClient(CN);
        }


        //PEÇAS CODE
        private void Specs_Load(object sender, EventArgs e)
        {
            if (dt2.Columns.Count == 0)
            {
                dt2.Columns.Add("Tipo", typeof(string));
                dt2.Columns.Add("Medida", typeof(string));
                SpecsGrid.DataSource = dt2;
            }
        }

        private void SpecBtn_Click(object sender, EventArgs e)
        {
            if (Pdrop.Text == "" || Psize.Text == "")
            {
                MessageBox.Show("Por favor preencha todos os campos.");
                return;
            }
            if (!double.TryParse(Psize.Text, out double psizeValue) || psizeValue <= 0)
            {
                MessageBox.Show("O valor da medida deve ser númerico e positivo.");
                return;
            }

            if (dt2.Rows.Count == 0)
            {
                dt2.Rows.Add(Pdrop.Text, Psize.Text);
            }
            else if (dt2.Rows.Count > 0 && dt2.Rows.Count < 5)
            {
                //verificar se já existe um tipo igual em qualquer linha
                foreach (DataRow row in dt2.Rows)
                {
                    if (row["Tipo"].ToString() == Pdrop.Text)
                    {
                        MessageBox.Show("Já inseriu uma especificação do mesmo tipo.");
                        return;
                    }
                }
                dt2.Rows.Add(Pdrop.Text, Psize.Text);

            }
            else
            {
                MessageBox.Show("Só pode inserir até 5 especificações.");
                return;
            }

        }
        private void AddPart(SqlConnection CN)
        {
            // Get the values from the textboxes
            string partName = Pname.Text;
            string partPrice = Ppreco.Text;
            string partdesc = Pdescri.Text;
            string partMarca = Pmarca.Text;
            string partCategotia = Pcategoria.Text;
            string partId = Pid.Text;
            string? specWeight = null;
            string? specHeight = null;
            string? specWidth = null;
            string? specLength = null;
            string? specDiameter = null;
            foreach (DataRow row in dt2.Rows)
            {
                if (row["Tipo"].ToString() == "Peso")
                {
                    specWeight = row["Medida"].ToString();
                }
                else if (row["Tipo"].ToString() == "Altura")
                {
                    specHeight = row["Medida"].ToString();
                }
                else if (row["Tipo"].ToString() == "Largura")
                {
                    specWidth = row["Medida"].ToString();
                }
                else if (row["Tipo"].ToString() == "Comprimento")
                {
                    specLength = row["Medida"].ToString();
                }
                else if (row["Tipo"].ToString() == "Diâmetro")
                {
                    specDiameter = row["Medida"].ToString();
                }   
            }

            // Query to insert the part
            string query = "INSERT INTO Parts (Name, Price, Description, Manufacturer, Category, Part_id) VALUES (@Name, @Price, @Description, @Brand, @Category, @Part_id)";

            // Query to insert the specs
            string query2 = "INSERT INTO Specs (Part_id, Weight, Height, Width, Length, Diameter) VALUES (@Part_id, @Weight, @Height, @Width, @Length, @Diameter)";

            // Execute the query to the parts table

            using (SqlCommand cmd = new SqlCommand(query, CN))
            {
                cmd.Parameters.AddWithValue("@Name", partName);
                cmd.Parameters.AddWithValue("@Price", partPrice);
                cmd.Parameters.AddWithValue("@Description", partdesc);
                cmd.Parameters.AddWithValue("@Brand", partMarca);
                cmd.Parameters.AddWithValue("@Category", partCategotia);
                cmd.Parameters.AddWithValue("@Part_id", partId);

                try
                {
                    if (CN.State == System.Data.ConnectionState.Closed)
                    {
                        CN.Open();
                    }

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Part added successfully.");
                    }
                    else
                    {
                        MessageBox.Show("An error occurred while adding the part.");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred: " + ex.Message);
                }
            }

            // Execute the query to the specs table

            using (SqlCommand cmd = new SqlCommand(query2, CN))
            {
                cmd.Parameters.AddWithValue("@Part_id", partId);
                cmd.Parameters.AddWithValue("@Weight", specWeight);
                cmd.Parameters.AddWithValue("@Height", specHeight);
                cmd.Parameters.AddWithValue("@Width", specWidth);
                cmd.Parameters.AddWithValue("@Length", specLength);
                cmd.Parameters.AddWithValue("@Diameter", specDiameter);

                try
                {
                    if (CN.State == System.Data.ConnectionState.Closed)
                    {
                        CN.Open();
                    }

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Specs added successfully.");
                    }
                    else
                    {
                        MessageBox.Show("An error occurred while adding the specs.");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred: " + ex.Message);
                }
            }


        }

        private void Pbutton_Click(object sender, EventArgs e)
        {
            //SqlConnection CN = GetDbConnection();
            //AddPart(CN);
        }
        private void PartAdd_Clear(object sender, EventArgs e)
        {
            Pname.Text = "";
            Ppreco.Text = "";
            Pdescri.Text = "";
            Pmarca.Text = "";
            Pid.Text = "";
            Pcategoria.SelectedIndex = -1;
            Pdrop.SelectedIndex = -1;
            Psize.Text = "";
            dt2.Clear();
        }
    }
}
