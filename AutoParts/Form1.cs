using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace AutoParts
{
    public partial class Form1 : Form
    {
        DataTable dt = new DataTable();
        DataTable dt2 = new DataTable();
        DataTable dt3 = new DataTable();
        SqlConnection CN = GetDbConnection();

        public Form1()
        {
            InitializeComponent();

        }

        //Connect to the database
        private static SqlConnection GetDbConnection()
        {
            string connectionString = "Data Source = tcp:mednat.ieeta.pt\\SQLSERVER,8101; uid = REDACTED_UID; password = REDACTED_PASSWORD";
            SqlConnection CN = new SqlConnection(connectionString);
            return CN;
        }

        //ADMIN CODE
        private void AddAdmin(SqlConnection CN)
        {
            // Get the values from the textboxes
            string adminName = Aname.Text;
            string adminCC = Acc.Text;
            DateTime.TryParse(Abirth.Text, out DateTime adminBirth);
            string adminAddr = Aaddr.Text;
            string adminCp = Acp.Text;
            double.TryParse(Asal.Text, out double adminSalary);
            DateTime adminStart = monthCalendar1.SelectionStart.Date;

            //Query to insert the Person
            string queryPerson = "INSERT INTO AP_Person (Name, CC, Birth, Address, Postal) VALUES (@Name, @CC, @Birth, @Address, @Postal)";

            // Query to insert the admin
            string queryAdmin = "INSERT INTO AP_Administrator (CC,Contract_Start,Contract_End,Work_id, Salary) VALUES (@CC, @Contract_Start, @Contract_End, @Work_id, @Salary)";

            using (SqlCommand cmd = new SqlCommand(queryPerson, CN))
            {
                cmd.Parameters.AddWithValue("@Name", adminName);
                cmd.Parameters.AddWithValue("@CC", adminCC);
                cmd.Parameters.AddWithValue("@Birth", adminBirth);
                cmd.Parameters.AddWithValue("@Address", adminAddr);
                cmd.Parameters.AddWithValue("@Postal", adminCp);

                try
                {
                    if (CN.State == System.Data.ConnectionState.Closed)
                    {
                        CN.Open();
                    }

                    int rowsAffected = cmd.ExecuteNonQuery();

                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred: " + ex.Message);
                }
            }

            using (SqlCommand cmd = new SqlCommand(queryAdmin, CN))
            {
                cmd.Parameters.AddWithValue("@CC", adminCC);
                cmd.Parameters.AddWithValue("@Contract_Start", adminStart);
                cmd.Parameters.AddWithValue("@Contract_End", DBNull.Value);
                cmd.Parameters.AddWithValue("@Work_id", 0);
                cmd.Parameters.AddWithValue("@Salary", adminSalary);

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
            AddAdmin(CN);
        }

        //CUSTOMER CODE
        private void Customer_Load(object sender, EventArgs e)
        {
            if (dt.Columns.Count == 0)
            {
                dt.Columns.Add("Tipo", typeof(string));
                dt.Columns.Add("Contacto", typeof(string));
                ClientContactData.DataSource = dt;
            }
            //read the last id from the database
            string query = "SELECT MAX(Id) FROM AP_Customer";

            using (SqlCommand cmd = new SqlCommand(query, CN))
            {
                try
                {
                    if (CN.State == System.Data.ConnectionState.Closed)
                    {
                        CN.Open();
                    }

                    object result = cmd.ExecuteScalar();

                    if (result != DBNull.Value && result != null)
                    {
                        int maxId = Convert.ToInt32(result);
                        Cid.Text = (maxId + 1).ToString();
                    }
                    else
                    {
                        // This handles the case where there are no rows in the table
                        Cid.Text = "1";
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred: " + ex.Message);
                }
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
            DateTime.TryParse(Cbirth.Text, out DateTime customerBirth);
            string customerAddr = Caddr.Text;
            string customerCp = Ccp.Text;
            int customerId = int.Parse(Cid.Text);
            string customerContacts = "";
            string contact0 = string.Empty;
            string contact1 = string.Empty;
            string? customerEmail = null;
            string? customerTelm = null;
            foreach (DataRow row in dt.Rows)
            {
                customerContacts += row["Tipo"].ToString() + ":" + row["Contacto"].ToString() + "|";
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

            // Query to insert the Person
            string queryPerson = "INSERT INTO AP_Person (Name, CC, Birth, Address, Postal) VALUES (@Name, @CC, @Birth, @Address, @Postal)";

            using (SqlCommand cmd = new SqlCommand(queryPerson, CN))
            {
                cmd.Parameters.AddWithValue("@Name", customerName);
                cmd.Parameters.AddWithValue("@CC", customerCC);
                cmd.Parameters.AddWithValue("@Birth", customerBirth);
                cmd.Parameters.AddWithValue("@Address", customerAddr);
                cmd.Parameters.AddWithValue("@Postal", customerCp);

                try
                {
                    if (CN.State == System.Data.ConnectionState.Closed)
                    {
                        CN.Open();
                    }

                    int rowsAffected = cmd.ExecuteNonQuery();

                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred: " + ex.Message);
                }
            }

            // Query to insert the customer
            string query = "INSERT INTO AP_Customer (CC, Id, Email, Phone) VALUES (@CC, @Id, @Email, @Phone)";

            using (SqlCommand cmd = new SqlCommand(query, CN))
            {
                cmd.Parameters.AddWithValue("@CC", customerCC);
                cmd.Parameters.AddWithValue("@Id", customerId);
                if (customerEmail == null)
                {
                    cmd.Parameters.AddWithValue("@Email", DBNull.Value);
                }
                else
                {
                    cmd.Parameters.AddWithValue("@Email", customerEmail);
                }
                if (customerTelm == null)
                {
                    cmd.Parameters.AddWithValue("@Phone", DBNull.Value);
                }
                else
                {
                    cmd.Parameters.AddWithValue("@Phone", customerTelm);
                }

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
            AddClient(CN);
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
            if (dt3.Columns.Count == 0)
            {
                dt3.Columns.Add("Id", typeof(string));
                dt3.Columns.Add("Marca", typeof(string));
                dt3.Columns.Add("Modelo", typeof(string));
                dt3.Columns.Add("Versão", typeof(string));
                dt3.Columns.Add("Ano", typeof(DateTime));
                dt3.Columns.Add("Combustível", typeof(string));
                dt3.Columns.Add("Hp", typeof(int));
                dt3.Columns.Add("Binário", typeof(int));
                PVehiclePesquisa.DataSource = dt3;
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
        private void PVehiclePesquisaBtn_Click(object sender, EventArgs e)
        {
            Load_Vehicles(CN);
        }
        private void Load_Vehicles(SqlConnection CN)
        {
            //read the values from the textboxes
            string vehicleId = PVehicleIDInput.Text;
            string vehicleMarca = PVehicleMarcaInput.Text;
            string vehicleModel = PVehicleModeloInput.Text;
            string vehicleVersion = PVehicleVersaoInput.Text;
            //Query to get the vehicles and engines based on the filters or one of them or all
            string query = "SELECT AP_Vehicle.Vehicle_id, AP_Vehicle.Make, AP_Vehicle.Model, AP_Vehicle.Sub_model, AP_Vehicle.Manuf_stat, AP_Engine.Fuel_type, AP_Engine.Horsepower, AP_Engine.Torque " +
               "FROM AP_Vehicle JOIN AP_Engine ON AP_Vehicle.Vengine_id = AP_Engine.Engine_id " +
               "WHERE AP_Vehicle.Vehicle_id LIKE %@Id% AND AP_Vehicle.Make LIKE %@Make% " +
               "AND AP_Vehicle.Model LIKE %@Model% AND AP_Vehicle.Sub_model LIKE %@Sub_model%";

            using (SqlCommand cmd = new SqlCommand(query, CN))
            {
                cmd.Parameters.AddWithValue("@Id", vehicleId);
                cmd.Parameters.AddWithValue("@Make", vehicleMarca);
                cmd.Parameters.AddWithValue("@Model", vehicleModel);
                cmd.Parameters.AddWithValue("@Sub_model", vehicleVersion);

                try
                {
                    if (CN.State == System.Data.ConnectionState.Closed)
                    {
                        CN.Open();
                    }

                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        string vehicleIdValue = reader["Vehicle_id"].ToString();
                        string vehicleMarcaValue = reader["Make"].ToString();
                        string vehicleModelValue = reader["Model"].ToString();
                        string vehicleVersionValue = reader["Sub_model"].ToString();
                        string vehicleAnoValue = reader["Manuf_stat"].ToString();
                        string vehicleCombustivelValue = reader["Fuel_type"].ToString();
                        string vehicleHpValue = reader["Horsepower"].ToString();
                        string vehicleBinarioValue = reader["Torque"].ToString();

                        dt3.Rows.Add(vehicleIdValue, vehicleMarcaValue, vehicleModelValue, vehicleVersionValue, vehicleAnoValue, vehicleCombustivelValue, vehicleHpValue, vehicleBinarioValue);
                    }
                    else
                    {
                        MessageBox.Show("Não foram encontrados veículos com os filtros inseridos.");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred: " + ex.Message);
                }
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
            string Compatibility = Pcompatibilidade.Text;
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
            string query = "INSERT INTO AP_Part (Name, Price, Description, Manufacturer, Part_id) VALUES (@Name, @Price, @Description, @Brand, @Part_id)";
            using (SqlCommand cmd = new SqlCommand(query, CN))
            {
                cmd.Parameters.AddWithValue("@Name", partName);
                cmd.Parameters.AddWithValue("@Price", partPrice);
                cmd.Parameters.AddWithValue("@Description", partdesc);
                cmd.Parameters.AddWithValue("@Brand", partMarca);
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

            // Query to insert the specs
            string query2 = "INSERT INTO AP_Specs (Part_id, Weight, Height, Width, Length, Diameter) VALUES (@Part_id, @Weight, @Height, @Width, @Length, @Diameter)";
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

            // query to insert the category
            string query3 = "INSERT INTO AP_Category (Part_id, Category) VALUES (@Part_id, @Category)";
            using (SqlCommand cmd = new SqlCommand(query3, CN))
            {
                cmd.Parameters.AddWithValue("@Part_id", partId);
                cmd.Parameters.AddWithValue("@Category", partCategotia);

                try
                {
                    if (CN.State == System.Data.ConnectionState.Closed)
                    {
                        CN.Open();
                    }

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Category added successfully.");
                    }
                    else
                    {
                        MessageBox.Show("An error occurred while adding the category.");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred: " + ex.Message);
                }
            }

            // Query to insert the compatibility with the vehicle
            string query4 = "INSERT INTO AP_Compatibility (CPart_id, CVehicle_id, Type) VALUES (@CPart_id, @Cvehicle_id, @Type)";
            using (SqlCommand cmd = new SqlCommand(query4, CN))
            {
                cmd.Parameters.AddWithValue("@CPart_id", partId);
                cmd.Parameters.AddWithValue("@Cvehicle_id", DBNull.Value);
                cmd.Parameters.AddWithValue("@Type", Compatibility);

                try
                {
                    if (CN.State == System.Data.ConnectionState.Closed)
                    {
                        CN.Open();
                    }

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Compatibility added successfully.");
                    }
                    else
                    {
                        MessageBox.Show("An error occurred while adding the compatibility.");
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

        //AVALIAÇÃO CODE

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            labelAvalicao.Text = trackBar1.Value.ToString();
        }
        private void AvaliacaoLoad(object sender, EventArgs e)
        {
            trackBar1.Value = 5;
            labelAvalicao.Text = "5";
            CustomerLoad(CN);
        }
        private void CustomerLoad(SqlConnection CN)
        {
            // Query to get the customers join the Person table
            string query = "SELECT AP_Person.Name, AP_Customer.CC, AP_Customer.Id FROM AP_Person JOIN AP_Customer ON AP_Person.CC = AP_Customer.CC";

            using (SqlCommand cmd = new SqlCommand(query, CN))
            {
                try
                {
                    if (CN.State == System.Data.ConnectionState.Closed)
                    {
                        CN.Open();
                    }

                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        string customerName = reader["Name"].ToString();
                        string customerCC = reader["CC"].ToString();
                        string customerId = reader["Id"].ToString();

                        AvalicaoClient.Items.Add(customerId + " - " + customerName + " - " + customerCC);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred: " + ex.Message);
                }
            }
        }
    }
}
