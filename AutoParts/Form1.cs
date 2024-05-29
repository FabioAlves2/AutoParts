using System.Data;
using System.Data.SqlClient;
using System.Drawing;

namespace AutoParts
{
    public partial class Form1 : Form
    {
        DataTable dt = new DataTable();
        DataTable dt2 = new DataTable();
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

        //AVALIAÇÃO CODE

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            labelAvalicao.Text = trackBar1.Value.ToString();
        }
        private void AvaliacaoLoad(object sender, EventArgs e)
        {
            trackBar1.Value = 5;
            labelAvalicao.Text = "5";
            //SqlConnection CN = GetDbConnection();
            //CustomerLoad(CN);
        }
        private void CustomerLoad(SqlConnection CN)
        {
            string query = "SELECT * FROM Customer";
            //
            //
            //Vamos buscar aos clientes ou a pessoa?
            //
            //


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
                        string customerName = reader["Nome"].ToString();
                        string customerCC = reader["CC"].ToString();

                        Cdrop.Items.Add(customerName + " - " + customerCC);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred: " + ex.Message);
                }
            }
        }

        // ADD ENGINE CODE
        private void AddEngine(SqlConnection CN)
        {
            // Get the values from the textboxes
            string engineID = Mid.Text;
            string engineMake = Mmarca.Text;
            string engineFuel = Mcomb.Text;
            string engineFuelSystem = Malimentacao.Text;
            string engineAspiration = Msobrealimentacao.Text;
            string enginePower = Mhp.Text;
            string engineCC = Mcilindrada.Text;
            string engineTorque = Mbin.Text;
            string engineCylinders = MNcil.Text;
            string engineValves = MNval.Text;
            string engineType = Mtipo.Text;

            // Query to insert the engine
            string query = "INSERT INTO AP_Engine (Engine_id, Make, Fuel_type, Fuel_system, Aspiration, Horsepower, Cubic_cpt, Torque, Cylinder, Valves, Type) VALUES (@ID, @Make, @Fuel, @FuelSys, @Aspiration, @HP, @CC, @Torque, @Cylinder, @Valves, @Type)";

            using (SqlCommand cmd = new SqlCommand(query, CN))
            {

                cmd.Parameters.AddWithValue("@ID", engineID);
                cmd.Parameters.AddWithValue("@Make", engineMake);
                cmd.Parameters.AddWithValue("@Fuel", engineFuel);
                cmd.Parameters.AddWithValue("@FuelSys", engineFuelSystem);
                cmd.Parameters.AddWithValue("@Aspiration", engineAspiration);
                cmd.Parameters.AddWithValue("@HP", enginePower);
                cmd.Parameters.AddWithValue("@CC", engineCC);
                cmd.Parameters.AddWithValue("@Torque", engineTorque);
                cmd.Parameters.AddWithValue("@Cylinder", engineCylinders);
                cmd.Parameters.AddWithValue("@Valves", engineValves);
                cmd.Parameters.AddWithValue("@Type", engineType);

                try
                {
                    if (CN.State == System.Data.ConnectionState.Closed)
                    {
                        CN.Open();
                    }

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Engine added successfully.");
                    }
                    else
                    {
                        MessageBox.Show("An error occurred while adding the engine.");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred: " + ex.Message);
                }
            }
        }

        private void Madd_Click(object sender, EventArgs e)
        {
            AddEngine(CN);
        }

        // ENGINE LIST
        private void EngineList(SqlConnection CN)
        {
            string query = "SELECT * FROM AP_Engine";

            using (SqlDataAdapter adapter = new SqlDataAdapter(query, CN))
            {
                DataTable engineDt = new DataTable();
                adapter.Fill(engineDt);
                Mlista.DataSource = engineDt;
            }
        }
        private void MotorLista_Enter(object sender, EventArgs e)
        {
            EngineList(CN);
        }

        // ENGINE FILTERS
        private void MlistaID_TextChanged(object sender, EventArgs e)
        {
            MlistaFilters();
        }

        private void MlistaMarca_TextChanged(object sender, EventArgs e)
        {
            MlistaFilters();
        }

        private void MlistaCil_ValueChanged(object sender, EventArgs e)
        {
            MlistaFilters();
        }

        private void MlistaVal_ValueChanged(object sender, EventArgs e)
        {
            MlistaFilters();
        }

        private void MlistaHP_TextChanged(object sender, EventArgs e)
        {
            MlistaFilters();
        }

        private void MlistaCC_TextChanged(object sender, EventArgs e)
        {
            MlistaFilters();
        }

        private void MlistaTipo_SelectedIndexChanged(object sender, EventArgs e)
        {
            MlistaFilters();
        }

        private void MlistaComb_SelectedIndexChanged(object sender, EventArgs e)
        {
            MlistaFilters();
        }

        private void Mlista_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (Mlista.IsCurrentCellDirty)
            {
                Mlista.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        private void MlistaFilters()
        {
            string filterID = MlistaID.Text;
            string filterMake = MlistaMarca.Text;
            string filterCil = MlistaCil.Value.ToString();
            string filterVal = MlistaVal.Value.ToString();
            string filterPower = MlistaHP.Text;
            string filterCC = MlistaCC.Text;
            string filterType = MlistaTipo.SelectedItem?.ToString();
            string filterFuel = MlistaComb.SelectedItem?.ToString();

            string filterExpression = string.Empty;

            if (!string.IsNullOrEmpty(filterID))
            {
                filterExpression += string.Format("Engine_Id LIKE '%{0}%'", filterID);
            }

            if (!string.IsNullOrEmpty(filterMake))
            {
                if (!string.IsNullOrEmpty(filterExpression))
                {
                    filterExpression += " AND ";
                }
                filterExpression += string.Format("Make LIKE '{0}%'", filterMake);
            }

            if (MlistaCil.Value != 0 )
            {
                if (!string.IsNullOrEmpty(filterExpression))
                {
                    filterExpression += " AND ";
                }
                filterExpression += string.Format("Cylinder = {0}", filterCil);
            }

            if (MlistaVal.Value != 0)
            {
                if (!string.IsNullOrEmpty(filterExpression))
                {
                    filterExpression += " AND ";
                }
                filterExpression += string.Format("Valves = {0}", filterVal);
            }

            if (!string.IsNullOrEmpty(filterPower))
            {
                if (!string.IsNullOrEmpty(filterExpression))
                {
                    filterExpression += " AND ";
                }
                filterExpression += string.Format("Horsepower = {0}", filterPower);
            }

            if (!string.IsNullOrEmpty(filterCC))
            {
                if (!string.IsNullOrEmpty(filterExpression))
                {
                    filterExpression += " AND ";
                }
                filterExpression += string.Format("Cubic_cpt = {0}", filterCC);
            }

            if (!string.IsNullOrEmpty(filterType))
            {
                if (!string.IsNullOrEmpty(filterExpression))
                {
                    filterExpression += " AND ";
                }
                filterExpression += string.Format("Type LIKE '{0}%'", filterType);
            }

            if (!string.IsNullOrEmpty(filterFuel))
            {
                if (!string.IsNullOrEmpty(filterExpression))
                {
                    filterExpression += " AND ";
                }
                filterExpression += string.Format("Fuel_type LIKE '{0}%'", filterFuel);
            }

            (Mlista.DataSource as DataTable).DefaultView.RowFilter = filterExpression;


        }

        // ENGINE REMOVE
        private void Mremover_Click(object sender, EventArgs e)
        {
            if(Mlista.SelectedRows.Count > 0)
            {
                var confirmResult = MessageBox.Show("Tem a certeza que deseja remover o motor selecionado?", "Confirmar", MessageBoxButtons.YesNo);
                if (confirmResult == DialogResult.Yes)
                {
                    string engineID = Mlista.SelectedRows[0].Cells["Engine_id"].Value.ToString();
                    string query = "DELETE FROM AP_Engine WHERE Engine_id = @ID";

                    using (SqlCommand cmd = new SqlCommand(query, CN))
                    {
                        cmd.Parameters.AddWithValue("@ID", engineID);

                        try
                        {
                            if (CN.State == System.Data.ConnectionState.Closed)
                            {
                                CN.Open();
                            }

                            int rowsAffected = cmd.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("Engine removed successfully.");
                                EngineList(CN);
                            }
                            else
                            {
                                MessageBox.Show("An error occurred while removing the engine.");
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("An error occurred: " + ex.Message);
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Por favor selecione um motor para remover.");
            }
        }
    }
}
