using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace AutoParts
{
    public partial class Form1 : Form
    {
        DataTable dt = new DataTable();
        DataTable dt2 = new DataTable();
        DataTable OrderCart = new DataTable();
        SqlConnection CN = GetDbConnection();

        private string engineID;

        public Form1()
        {
            InitializeComponent();

        }

        //Connect to the database
        private static SqlConnection GetDbConnection()
        {
            string? connectionString = Environment.GetEnvironmentVariable("AUTOPARTS_CONNECTION_STRING");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "AUTOPARTS_CONNECTION_STRING is not set. Example: " +
                    "\"Data Source=<server>;uid=<user>;password=<password>\" " +
                    "(see README for setup instructions).");
            }
            SqlConnection CN = new SqlConnection(connectionString);
            return CN;
        }

        //ADMIN CODE
        private void Admin_Load(object sender, EventArgs e)
        {
            LoadAdmins(CN);
        }
        private void LoadAdmins(SqlConnection CN)
        {
            if (CN.State == System.Data.ConnectionState.Open)
            {
                CN.Close();
            }

            CN.Open();
            string query = "SELECT AP_Administrator.Work_id, AP_Person.Name, AP_Person.CC, AP_Person.Birth, AP_Person.Address, AP_Person.Postal, AP_Administrator.Contract_Start, AP_Administrator.Contract_End, AP_Administrator.Salary FROM AP_Person JOIN AP_Administrator ON AP_Person.CC = AP_Administrator.CC";

            using (SqlDataAdapter adapter = new SqlDataAdapter(query, CN))
            {
                DataTable adminDt = new DataTable();
                adapter.Fill(adminDt);
                PesquisaAdmin.DataSource = adminDt;
            }
            CN.Close();
        }
        private void AdminFilter()
        {
            string filterID = AdminIDfilter.Text;
            string filterName = AdminNamefilter.Text;


            string filterExpression = string.Empty;

            if (!string.IsNullOrEmpty(filterID))
            {
                filterExpression += string.Format("CONVERT(Work_id, 'System.String') LIKE '%{0}%'", filterID);
            }
            if (!string.IsNullOrEmpty(filterName))
            {
                if (!string.IsNullOrEmpty(filterExpression))
                {
                    filterExpression += " AND ";
                }
                filterExpression += string.Format("Name LIKE '{0}%'", filterName);
            }
            if (contratocheck.Checked)
            {
                if (!string.IsNullOrEmpty(filterExpression))
                {
                    filterExpression += " AND ";
                }
                filterExpression += string.Format("Contract_End IS NULL");
            }


            (PesquisaAdmin.DataSource as DataTable).DefaultView.RowFilter = filterExpression;
        }
        private void AdminIDfilter_TextChanged(object sender, EventArgs e)
        {
            AdminFilter();
        }
        private void AdminNamefilter_TextChanged(object sender, EventArgs e)
        {
            AdminFilter();
        }
        private void contratocheck_CheckedChanged(object sender, EventArgs e)
        {
            AdminFilter();
        }
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
            string queryAdmin = "INSERT INTO AP_Administrator (CC,Contract_Start,Contract_End, Salary) VALUES (@CC, @Contract_Start, @Contract_End, @Salary)";

            using (SqlCommand cmd = new SqlCommand(queryPerson, CN))
            {
                cmd.Parameters.AddWithValue("@Name", adminName);
                cmd.Parameters.AddWithValue("@CC", adminCC);
                cmd.Parameters.AddWithValue("@Birth", adminBirth);
                cmd.Parameters.AddWithValue("@Address", adminAddr);
                cmd.Parameters.AddWithValue("@Postal", adminCp);

                try
                {
                    if (CN.State == System.Data.ConnectionState.Open)
                    {
                        CN.Close();
                    }

                    CN.Open();

                    int rowsAffected = cmd.ExecuteNonQuery();
                    CN.Close();
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
                cmd.Parameters.AddWithValue("@Salary", adminSalary);

                try
                {
                    if (CN.State == System.Data.ConnectionState.Open)
                    {
                        CN.Close();
                    }

                    CN.Open();

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Admin added successfully.");
                    }
                    else
                    {
                        MessageBox.Show("An error occurred while adding the admin.");
                    }
                    CN.Close();
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
        //REMOVE ADMIN
        private void RemoveAdmin_Click(object sender, EventArgs e)
        {
            if (PesquisaAdmin.SelectedRows.Count > 0)
            {
                var confirmResult = MessageBox.Show("Tem a certeza que deseja remover o administrador selecionado?", "Confirmar", MessageBoxButtons.YesNo);
                if (confirmResult == DialogResult.Yes)
                {
                    string workID = PesquisaAdmin.SelectedRows[0].Cells["Work_id"].Value.ToString();
                    string query = "DELETE FROM AP_Administrator WHERE Work_id = @workID";

                    using (SqlCommand cmd = new SqlCommand(query, CN))
                    {
                        cmd.Parameters.AddWithValue("@workID", workID);

                        try
                        {
                            if (CN.State == System.Data.ConnectionState.Open)
                            {
                                CN.Close();
                            }

                            CN.Open();

                            int rowsAffected = cmd.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("Administrator removed successfully.");
                                LoadAdmins(CN);
                            }
                            else
                            {
                                MessageBox.Show("An error occurred while removing the administrator.");
                            }
                            CN.Close();
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
                MessageBox.Show("Por favor selecione um administrador para remover.");
            }
        }
        //
        //
        //STOCK CODE
        //
        //
        private void Stock_Load(object sender, EventArgs e)
        {
            StockAdminLoad(CN);
            StockPecasLoad(CN);
            StockFornecedorLoad(CN);
        }
        private void StockPecasLoad(SqlConnection CN)
        {
            string query = "SELECT AP_Part.Part_id, AP_Part.Name, AP_Part.Manufacturer, AP_Category.Category FROM AP_Part JOIN AP_Category ON AP_Part.Part_id = AP_Category.Part_id";
            if (CN.State == System.Data.ConnectionState.Open)
            {
                CN.Close();
            }
            CN.Open();

            using (SqlDataAdapter adapter = new SqlDataAdapter(query, CN))
            {
                DataTable spartsDt = new DataTable();
                adapter.Fill(spartsDt);
                StockPecasSearch.DataSource = spartsDt;
            }
            CN.Close();
        }
        private void StockPecasFilter(SqlConnection CN)
        {
            string filterID = StockPecasIdFilter.Text;
            string filterName = StockPecasNomeFilter.Text;
            string filterMarca = StockPecasMarcaFilter.Text;
            string filterCategoria = StockPecasCategoriaFilter.Text;

            string filterExpression = string.Empty;

            if (!string.IsNullOrEmpty(filterID))
            {
                filterExpression += string.Format("Part_id LIKE '%{0}%'", filterID);
            }
            if (!string.IsNullOrEmpty(filterName))
            {
                if (!string.IsNullOrEmpty(filterExpression))
                {
                    filterExpression += " AND ";
                }
                filterExpression += string.Format("Name LIKE '%{0}%'", filterName);
            }
            if (!string.IsNullOrEmpty(filterMarca))
            {
                if (!string.IsNullOrEmpty(filterExpression))
                {
                    filterExpression += " AND ";
                }
                filterExpression += string.Format("Manufacturer LIKE '{0}%'", filterMarca);
            }
            if (!string.IsNullOrEmpty(filterCategoria))
            {
                if (!string.IsNullOrEmpty(filterExpression))
                {
                    filterExpression += " AND ";
                }
                filterExpression += string.Format("Category LIKE '%{0}%'", filterCategoria);
            }

            (StockPecasSearch.DataSource as DataTable).DefaultView.RowFilter = filterExpression;
        }

        private void StockAdminLoad(SqlConnection CN)
        {
            //query to get the admins
            string query = "SELECT AP_Administrator.Work_id, AP_Person.Name, AP_Person.CC FROM AP_Person JOIN AP_Administrator ON AP_Person.CC = AP_Administrator.CC";

            using (SqlCommand cmd = new SqlCommand(query, CN))
            {
                try
                {
                    if (CN.State == System.Data.ConnectionState.Open)
                    {
                        CN.Close();
                    }
                    CN.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        string adminName = reader["Name"].ToString();
                        string adminCC = reader["CC"].ToString();
                        string adminId = reader["Work_id"].ToString();

                        StockAdmin.Items.Add(adminId + " - " + adminName + " - " + adminCC);
                    }
                    CN.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred: " + ex.Message);
                }
            }
        }
        private void StockFornecedorLoad(SqlConnection CN)
        {
            //query to get the suppliers
            string query = "SELECT Supplier_id, Name FROM AP_Supplier";

            using (SqlCommand cmd = new SqlCommand(query, CN))
            {
                try
                {
                    if (CN.State == System.Data.ConnectionState.Open)
                    {
                        CN.Close();
                    }

                    CN.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        string supplierName = reader["Name"].ToString();
                        string supplierId = reader["Supplier_id"].ToString();

                        StockFornecedor.Items.Add(supplierId + " - " + supplierName);
                    }
                    CN.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred: " + ex.Message);
                }
            }
        }
        private void AddStock(SqlConnection CN)
        {
            // Get the values from the textboxes
            string stockAdmin = StockAdmin.Text.Split('-')[0].Trim();
            string stockPart = StockPecasSearch.SelectedRows[0].Cells["Part_id"].Value.ToString();
            string stockFornecedor = StockFornecedor.Text.Split('-')[0].Trim();
            int.TryParse(StockQty.Text, out int stockQuantidade);

            // Query to insert the stock
            string query = "INSERT INTO AP_Stocks (Part_id, Supplier_id, Work_id, Qty) VALUES (@Part_id, @Supplier_id, @Work_id, @Qty)";

            using (SqlCommand cmd = new SqlCommand(query, CN))
            {
                cmd.Parameters.AddWithValue("@Part_id", stockPart);
                cmd.Parameters.AddWithValue("@Supplier_id", stockFornecedor);
                cmd.Parameters.AddWithValue("@Work_id", stockAdmin);
                cmd.Parameters.AddWithValue("@Qty", stockQuantidade);

                try
                {
                    if (CN.State == System.Data.ConnectionState.Open)
                    {
                        CN.Close();
                    }

                    CN.Open();

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Stock added successfully.");
                    }
                    else
                    {
                        MessageBox.Show("An error occurred while adding the stock.");
                    }
                    CN.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred: " + ex.Message);
                }
            }
        }
        private void StockButton_Click(object sender, EventArgs e)
        {
            AddStock(CN);
        }
        private void StockPecasIdFilter_TextChanged(object sender, EventArgs e)
        {
            StockPecasFilter(CN);
        }
        private void StockPecasNomeFilter_TextChanged(object sender, EventArgs e)
        {
            StockPecasFilter(CN);
        }
        private void StockPecasMarcaFilter_TextChanged(object sender, EventArgs e)
        {
            StockPecasFilter(CN);
        }
        private void StockPecasCategoriaFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            StockPecasFilter(CN);
        }
        private void StockClear(object sender, EventArgs e)
        {
            StockAdmin.SelectedIndex = -1;
            StockPecasSearch.ClearSelection();
            StockFornecedor.SelectedIndex = -1;
            StockQty.Text = "";
            StockPecasCategoriaFilter.SelectedIndex = -1;
            StockPecasIdFilter.Text = "";
            StockPecasMarcaFilter.Text = "";
            StockPecasNomeFilter.Text = "";
            StockAdmin.Items.Clear();
            StockFornecedor.Items.Clear();
        }

        //
        //
        //CUSTOMER CODE
        //
        //
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
                    if (CN.State == System.Data.ConnectionState.Open)
                    {
                        CN.Close();
                    }

                    CN.Open();

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
                    CN.Close();
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
                MessageBox.Show("Por favor insira um email v�lido.");
                return;
            }
            else if (Cdrop.Text == "Telem�vel" && (Ccontact.Text.Length != 9 || !Ccontact.Text.StartsWith("9")))
            {
                MessageBox.Show("Por favor insira um n�mero de telem�vel v�lido.");
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
                        MessageBox.Show("J� inseriu um contacto do mesmo tipo.");
                        return;
                    }
                    else
                    {
                        dt.Rows.Add(Cdrop.Text, Ccontact.Text);
                    }
                }
                else
                {
                    MessageBox.Show("S� pode inserir dois contactos.");
                    return;
                }
            }
        }
        private void ClientIDfilter_TextChanged(object sender, EventArgs e)
        {
            ClientFilter();
        }

        private void ClientNamefilter_TextChanged(object sender, EventArgs e)
        {
            ClientFilter();
        }

        private void ClientCCfilter_TextChanged(object sender, EventArgs e)
        {
            ClientFilter();
        }

        private void ClientTlmfilter_TextChanged(object sender, EventArgs e)
        {
            ClientFilter();
        }
        private void ClientEmailfilter_TextChanged(object sender, EventArgs e)
        {
            ClientFilter();
        }

        private void CLientEmailfilter_TextChanged(object sender, EventArgs e)
        {
            ClientFilter();
        }
        private void Client_Load(object sender, EventArgs e)
        {
            LoadClient(CN);
        }
        private void LoadClient(SqlConnection CN)
        {
            string query = "SELECT AP_Customer.Id, AP_Person.Name, AP_Person.CC, AP_Person.Birth, AP_Person.Address, AP_Person.Postal, AP_Customer.Email, AP_Customer.Phone FROM AP_Person JOIN AP_Customer ON AP_Person.CC = AP_Customer.CC";

            if (CN.State == System.Data.ConnectionState.Open)
            {
                CN.Close();
            }

            CN.Open();
            using (SqlDataAdapter adapter = new SqlDataAdapter(query, CN))
            {
                DataTable clientDt = new DataTable();
                adapter.Fill(clientDt);
                PesquisaClient.DataSource = clientDt;
            }
            CN.Close();
        }
        private void ClientFilter()
        {
            string filterID = ClientIDfilter.Text;
            string filterName = ClientNamefilter.Text;
            string filterCC = ClientCCfilter.Text;
            string filterTlm = ClientTlmfilter.Text;
            string filterEmail = ClientEmailfilter.Text;


            string filterExpression = string.Empty;

            if (!string.IsNullOrEmpty(filterID))
            {
                filterExpression += string.Format("CONVERT(Id, 'System.String') LIKE '%{0}%'", filterID);
            }
            if (!string.IsNullOrEmpty(filterName))
            {
                if (!string.IsNullOrEmpty(filterExpression))
                {
                    filterExpression += " AND ";
                }
                filterExpression += string.Format("Name LIKE '%{0}%'", filterName);
            }
            if (!string.IsNullOrEmpty(filterCC))
            {
                if (!string.IsNullOrEmpty(filterExpression))
                {
                    filterExpression += " AND ";
                }
                filterExpression += string.Format("CONVERT(CC, 'System.String') LIKE '{0}%'", filterCC);
            }
            if (!string.IsNullOrEmpty(filterTlm))
            {
                if (!string.IsNullOrEmpty(filterExpression))
                {
                    filterExpression += " AND ";
                }
                filterExpression += string.Format("Phone LIKE '{0}%'", filterTlm);
            }
            if (!string.IsNullOrEmpty(filterEmail))
            {
                if (!string.IsNullOrEmpty(filterExpression))
                {
                    filterExpression += " AND ";
                }
                filterExpression += string.Format("Email LIKE '%{0}%'", filterEmail);
            }


            (PesquisaClient.DataSource as DataTable).DefaultView.RowFilter = filterExpression;
        }
        private void ClientRemove_Click(object sender, EventArgs e)
        {
            if (PesquisaClient.SelectedRows.Count > 0)
            {
                var confirmResult = MessageBox.Show("Tem a certeza que deseja remover o cliente selecionado?", "Confirmar", MessageBoxButtons.YesNo);
                if (confirmResult == DialogResult.Yes)
                {
                    string ClientID = PesquisaClient.SelectedRows[0].Cells["Id"].Value.ToString();
                    string query = "DELETE FROM AP_Customer WHERE Id = @ID";

                    using (SqlCommand cmd = new SqlCommand(query, CN))
                    {
                        cmd.Parameters.AddWithValue("@ID", ClientID);

                        try
                        {
                            if (CN.State == System.Data.ConnectionState.Open)
                            {
                                CN.Close();
                            }

                            CN.Open();

                            int rowsAffected = cmd.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("Customer removed successfully.");
                                LoadClient(CN);
                            }
                            else
                            {
                                MessageBox.Show("An error occurred while removing the customer.");
                            }
                            CN.Close();
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
                MessageBox.Show("Por favor selecione um cliente para remover.");
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
            string customerContacts = "";
            string contact0 = string.Empty;
            string contact1 = string.Empty;
            string? customerEmail = null;
            string? customerTelm = null;
            foreach (DataRow row in dt.Rows)
            {
                customerContacts += row["Tipo"].ToString() + ":" + row["Contacto"].ToString() + "|";
            }
            if (customerContacts.Contains("Email") && customerContacts.Contains("Telem�vel"))
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
            else if (contact0.StartsWith("Telem�vel"))
            {
                customerTelm = contact0.Split(":")[1];
            }
            if (contact1.StartsWith("Email"))
            {
                customerEmail = contact1.Split(":")[1];
            }
            else if (contact1.StartsWith("Telem�vel"))
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
                    if (CN.State == System.Data.ConnectionState.Open)
                    {
                        CN.Close();
                    }

                    CN.Open();

                    int rowsAffected = cmd.ExecuteNonQuery();
                    CN.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred: " + ex.Message);
                }
            }

            // Query to insert the customer
            string query = "INSERT INTO AP_Customer (CC, Email, Phone) VALUES (@CC, @Email, @Phone)";

            using (SqlCommand cmd = new SqlCommand(query, CN))
            {
                cmd.Parameters.AddWithValue("@CC", customerCC);
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
                    if (CN.State == System.Data.ConnectionState.Open)
                    {
                        CN.Close();
                    }

                    CN.Open();

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Customer added successfully.");
                        LoadClient(CN);
                    }
                    else
                    {
                        MessageBox.Show("An error occurred while adding the customer.");
                    }
                    CN.Close();
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
        //
        //
        //PE�AS CODE
        //
        //
        private void Parts_Load(object sender, EventArgs e)
        {
            PartsLoad(CN);
        }
        private void PartsLoad(SqlConnection CN)
        {
            string query = "SELECT * FROM AP_PartDetailsView;";
            if (CN.State == System.Data.ConnectionState.Open)
            {
                CN.Close();
            }

            CN.Open();

            using (SqlDataAdapter adapter = new SqlDataAdapter(query, CN))
            {
                DataTable pecasDt = new DataTable();
                adapter.Fill(pecasDt);
                PesquisaPecas.DataSource = pecasDt;
            }
            CN.Close();
        }
        private void PartsFilter(SqlConnection CN)
        {
            string filterID = PecasIdFilter.Text;
            string filterName = PecasNomeFilter.Text;
            string filterMarca = PecasMarcaFilter.Text;
            string filterCategoria = PecasCategoriaFilter.Text;
            string filterVehicle = PecasVehicleFilter.Text;

            string filterExpression = string.Empty;

            if (!string.IsNullOrEmpty(filterID))
            {
                filterExpression += string.Format("ID LIKE '%{0}%'", filterID);
            }
            if (!string.IsNullOrEmpty(filterName))
            {
                if (!string.IsNullOrEmpty(filterExpression))
                {
                    filterExpression += " AND ";
                }
                filterExpression += string.Format("Nome LIKE '%{0}%'", filterName);
            }
            if (!string.IsNullOrEmpty(filterMarca))
            {
                if (!string.IsNullOrEmpty(filterExpression))
                {
                    filterExpression += " AND ";
                }
                filterExpression += string.Format("Marca LIKE '{0}%'", filterMarca);
            }
            if (!string.IsNullOrEmpty(filterCategoria))
            {
                if (!string.IsNullOrEmpty(filterExpression))
                {
                    filterExpression += " AND ";
                }
                filterExpression += string.Format("Categoria LIKE '%{0}%'", filterCategoria);
            }
            if (!string.IsNullOrEmpty(filterVehicle))
            {
                if (!string.IsNullOrEmpty(filterExpression))
                {
                    filterExpression += " AND ";
                }
                filterExpression += string.Format("ID_Ve�culo LIKE '%{0}%'", filterVehicle);
            }
            if (PecasStockFilter.Checked)
            {
                if (!string.IsNullOrEmpty(filterExpression))
                {
                    filterExpression += " AND ";
                }
                filterExpression += string.Format("Stock > 0");
            }

            (PesquisaPecas.DataSource as DataTable).DefaultView.RowFilter = filterExpression;

        }
        private void PecasIdFilter_TextChanged(object sender, EventArgs e)
        {
            PartsFilter(CN);
        }

        private void PecasNomeFilter_TextChanged(object sender, EventArgs e)
        {
            PartsFilter(CN);
        }

        private void PecasMarcaFilter_TextChanged(object sender, EventArgs e)
        {
            PartsFilter(CN);
        }

        private void PecasCategoriaFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            PartsFilter(CN);
        }

        private void PecasVehicleFilter_TextChanged(object sender, EventArgs e)
        {
            PartsFilter(CN);
        }

        private void PecasStockFilter_CheckedChanged(object sender, EventArgs e)
        {
            PartsFilter(CN);
        }
        private void Specs_Load(object sender, EventArgs e)
        {
            if (dt2.Columns.Count == 0)
            {
                dt2.Columns.Add("Tipo", typeof(string));
                dt2.Columns.Add("Medida", typeof(string));
                SpecsGrid.DataSource = dt2;
                PVehicleLoad(CN);
            }
        }
        private void PVehicleLoad(SqlConnection CN)
        {
            string query = "SELECT AP_Vehicle.Vehicle_id, AP_Vehicle.Make, AP_Vehicle.Model, AP_Vehicle.Sub_model, AP_Vehicle.Manuf_start, AP_Engine.Fuel_type, AP_Engine.Horsepower, AP_Engine.Torque FROM AP_Vehicle JOIN AP_Engine ON AP_Vehicle.Vengine_id = AP_Engine.Engine_id";
            if (CN.State == System.Data.ConnectionState.Open)
            {
                CN.Close();
            }

            CN.Open();
            using (SqlDataAdapter adapter = new SqlDataAdapter(query, CN))
            {
                DataTable PVehicleDt = new DataTable();
                adapter.Fill(PVehicleDt);
                PVehiclePesquisa.DataSource = PVehicleDt;
            }
            CN.Close();
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
                MessageBox.Show("O valor da medida deve ser n�merico e positivo.");
                return;
            }

            if (dt2.Rows.Count == 0)
            {
                dt2.Rows.Add(Pdrop.Text, Psize.Text);
            }
            else if (dt2.Rows.Count > 0 && dt2.Rows.Count < 5)
            {
                //verificar se j� existe um tipo igual em qualquer linha
                foreach (DataRow row in dt2.Rows)
                {
                    if (row["Tipo"].ToString() == Pdrop.Text)
                    {
                        MessageBox.Show("J� inseriu uma especifica��o do mesmo tipo.");
                        return;
                    }
                }
                dt2.Rows.Add(Pdrop.Text, Psize.Text);

            }
            else
            {
                MessageBox.Show("S� pode inserir at� 5 especifica��es.");
                return;
            }

        }
        private void Load_Vehicles(SqlConnection CN)
        {
            //read the values from the textboxes
            string vehicleId = PVehicleIDInput.Text;
            string vehicleMarca = PVehicleMarcaInput.Text;
            string vehicleModel = PVehicleModeloInput.Text;
            string vehicleVersion = PVehicleVersaoInput.Text;

            string filterExpression = string.Empty;
            if (!string.IsNullOrEmpty(vehicleId))
            {
                filterExpression += string.Format("CONVERT(Vehicle_id, 'System.String') LIKE '%{0}%'", vehicleId);
            }
            if (!string.IsNullOrEmpty(vehicleMarca))
            {
                if (!string.IsNullOrEmpty(filterExpression))
                {
                    filterExpression += " AND ";
                }
                filterExpression += string.Format("Make LIKE '%{0}%'", vehicleMarca);
            }
            if (!string.IsNullOrEmpty(vehicleModel))
            {
                if (!string.IsNullOrEmpty(filterExpression))
                {
                    filterExpression += " AND ";
                }
                filterExpression += string.Format("Model LIKE '%{0}%'", vehicleModel);
            }
            if (!string.IsNullOrEmpty(vehicleVersion))
            {
                if (!string.IsNullOrEmpty(filterExpression))
                {
                    filterExpression += " AND ";
                }
                filterExpression += string.Format("Sub_model LIKE '%{0}%'", vehicleVersion);
            }


            (PVehiclePesquisa.DataSource as DataTable).DefaultView.RowFilter = filterExpression;

        }
        private void PVehicleIDInput_TextChanged(object sender, EventArgs e)
        {
            Load_Vehicles(CN);
        }

        private void PVehicleMarcaInput_TextChanged(object sender, EventArgs e)
        {
            Load_Vehicles(CN);
        }

        private void PVehicleModeloInput_TextChanged(object sender, EventArgs e)
        {
            Load_Vehicles(CN);
        }

        private void PVehicleVersaoInput_TextChanged(object sender, EventArgs e)
        {
            Load_Vehicles(CN);
        }

        private void AddPart(SqlConnection CN)
        {
            // Get the values from the textboxes
            string partName = Pname.Text;
            double.TryParse(Ppreco.Text, out double partPrice);
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
                else if (row["Tipo"].ToString() == "Di�metro")
                {
                    specDiameter = row["Medida"].ToString();
                }
            }

            // Query to insert the part
            string query = "INSERT INTO AP_Part (Name, Price, Description, Manufacturer, Part_id) VALUES (@Name, @Price, @Description, @Brand, @Part_id)";
            using (SqlCommand cmd = new SqlCommand(query, CN))
            {
                cmd.Parameters.AddWithValue("@Part_id", partId);
                cmd.Parameters.AddWithValue("@Name", partName);
                cmd.Parameters.AddWithValue("@Price", partPrice);
                cmd.Parameters.AddWithValue("@Description", partdesc);
                cmd.Parameters.AddWithValue("@Brand", partMarca);

                try
                {
                    if (CN.State == System.Data.ConnectionState.Open)
                    {
                        CN.Close();
                    }

                    CN.Open();

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Part added successfully.");
                    }
                    else
                    {
                        MessageBox.Show("An error occurred while adding the part.");
                    }
                    CN.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred: " + ex.Message);
                }
            }

            // Query to insert the specs
            string query2 = "INSERT INTO AP_Specs (Spart_id, Weight, Height, Width, Length, Diameter) VALUES (@Spart_id, @Weight, @Height, @Width, @Length, @Diameter)";
            using (SqlCommand cmd = new SqlCommand(query2, CN))
            {
                cmd.Parameters.AddWithValue("@Spart_id", partId);
                if (specWeight == null)
                {
                    cmd.Parameters.AddWithValue("@Weight", DBNull.Value);
                }
                else
                {
                    double.TryParse(specWeight, out double weight);
                    cmd.Parameters.AddWithValue("@Weight", weight);
                }
                if (specHeight == null)
                {
                    cmd.Parameters.AddWithValue("@Height", DBNull.Value);
                }
                else
                {
                    double.TryParse(specHeight, out double height);
                    cmd.Parameters.AddWithValue("@Height", height);
                }
                if (specWidth == null)
                {
                    cmd.Parameters.AddWithValue("@Width", DBNull.Value);
                }
                else
                {
                    double.TryParse(specWidth, out double width);
                    cmd.Parameters.AddWithValue("@Width", width);
                }
                if (specLength == null)
                {
                    cmd.Parameters.AddWithValue("@Length", DBNull.Value);
                }
                else
                {
                    double.TryParse(specLength, out double length);
                    cmd.Parameters.AddWithValue("@Length", length);
                }
                if (specDiameter == null)
                {
                    cmd.Parameters.AddWithValue("@Diameter", DBNull.Value);
                }
                else
                {
                    double.TryParse(specDiameter, out double diameter);
                    cmd.Parameters.AddWithValue("@Diameter", diameter);
                }

                try
                {
                    if (CN.State == System.Data.ConnectionState.Open)
                    {
                        CN.Close();
                    }

                    CN.Open();

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Specs added successfully.");
                    }
                    else
                    {
                        MessageBox.Show("An error occurred while adding the specs.");
                    }
                    CN.Close();
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
                cmd.Parameters.AddWithValue("@Category", partCategotia);
                cmd.Parameters.AddWithValue("@Part_id", partId);

                try
                {
                    if (CN.State == System.Data.ConnectionState.Open)
                    {
                        CN.Close();
                    }

                    CN.Open();

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Category added successfully.");
                    }
                    else
                    {
                        MessageBox.Show("An error occurred while adding the category.");
                    }
                    CN.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred: " + ex.Message);
                }
            }
            string CvehicleID = PVehiclePesquisa.SelectedRows[0].Cells["Vehicle_id"].Value.ToString();

            // Query to insert the compatibility with the vehicle
            string query4 = "INSERT INTO AP_Compatibility (CPart_id, CVehicle_id, Type) VALUES (@CPart_id, @Cvehicle_id, @Type)";
            using (SqlCommand cmd = new SqlCommand(query4, CN))
            {
                cmd.Parameters.AddWithValue("@Cvehicle_id", CvehicleID);
                cmd.Parameters.AddWithValue("@CPart_id", partId);
                cmd.Parameters.AddWithValue("@Type", Compatibility);

                try
                {
                    if (CN.State == System.Data.ConnectionState.Open)
                    {
                        CN.Close();
                    }

                    CN.Open();

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Compatibility added successfully.");
                    }
                    else
                    {
                        MessageBox.Show("An error occurred while adding the compatibility.");
                    }
                    CN.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred: " + ex.Message);
                }
            }
        }

        private void Pbutton_Click(object sender, EventArgs e)
        {
            AddPart(CN);
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
        private void RemovePart_Click(object sender, EventArgs e)
        {
            PartRemove(CN);
        }
        private void PartRemove(SqlConnection CN)
        {
            if (PesquisaPecas.SelectedRows.Count > 0)
            {
                var confirmResult = MessageBox.Show("Tem a certeza que deseja remover a pe�a selecionada?", "Confirmar", MessageBoxButtons.YesNo);
                if (confirmResult == DialogResult.Yes)
                {
                    string PartID = PesquisaPecas.SelectedRows[0].Cells["ID"].Value.ToString();
                    string query = "DELETE FROM AP_Part WHERE Part_id = @ID";

                    using (SqlCommand cmd = new SqlCommand(query, CN))
                    {
                        cmd.Parameters.AddWithValue("@ID", PartID);

                        try
                        {
                            if (CN.State == System.Data.ConnectionState.Open)
                            {
                                CN.Close();
                            }

                            CN.Open();

                            int rowsAffected = cmd.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("Part removed successfully.");
                                PartsLoad(CN);
                            }
                            else
                            {
                                MessageBox.Show("An error occurred while removing the part.");
                            }
                            CN.Close();
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
                MessageBox.Show("Por favor selecione uma pe�a para remover.");
            }

        }
        //
        //
        //AVALIA��O CODE
        //
        //
        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            labelAvalicao.Text = trackBar1.Value.ToString();
        }
        private void AvaliacaoLoad(object sender, EventArgs e)
        {
            trackBar1.Value = 5;
            labelAvalicao.Text = "5";
            PecasLoad(CN);
            CustomerLoad(CN);
        }
        private void PecasLoad(SqlConnection CN)
        {
            string query = "SELECT AP_Part.Part_id, AP_Part.Name, AP_Part.Manufacturer, AP_Category.Category FROM AP_Part JOIN AP_Category ON AP_Part.Part_id = AP_Category.Part_id";
            if (CN.State == System.Data.ConnectionState.Open)
            {
                CN.Close();
            }

            CN.Open();
            using (SqlDataAdapter adapter = new SqlDataAdapter(query, CN))
            {
                DataTable partsDt = new DataTable();
                adapter.Fill(partsDt);
                AvaliacaoPecas.DataSource = partsDt;
            }
            CN.Close();
        }
        private void AvaliacaoFilter()
        {
            string filterID = AvaliacaoID.Text;
            string filterName = AvaliacaoNome.Text;
            string filterMarca = AvaliacaoMarca.Text;
            string filterCategoria = AvaliacaoCategoria.Text;

            string filterExpression = string.Empty;

            if (!string.IsNullOrEmpty(filterID))
            {
                filterExpression += string.Format("Part_id LIKE '%{0}%'", filterID);
            }
            if (!string.IsNullOrEmpty(filterName))
            {
                if (!string.IsNullOrEmpty(filterExpression))
                {
                    filterExpression += " AND ";
                }
                filterExpression += string.Format("Name LIKE '%{0}%'", filterName);
            }
            if (!string.IsNullOrEmpty(filterMarca))
            {
                if (!string.IsNullOrEmpty(filterExpression))
                {
                    filterExpression += " AND ";
                }
                filterExpression += string.Format("Manufacturer LIKE '{0}%'", filterMarca);
            }
            if (!string.IsNullOrEmpty(filterCategoria))
            {
                if (!string.IsNullOrEmpty(filterExpression))
                {
                    filterExpression += " AND ";
                }
                filterExpression += string.Format("Category LIKE '%{0}%'", filterCategoria);
            }

            (AvaliacaoPecas.DataSource as DataTable).DefaultView.RowFilter = filterExpression;
        }
        private void AvaliacaoID_TextChanged(object sender, EventArgs e)
        {
            AvaliacaoFilter();
        }

        private void AvaliacaoNome_TextChanged(object sender, EventArgs e)
        {
            AvaliacaoFilter();
        }

        private void AvaliacaoMarca_TextChanged(object sender, EventArgs e)
        {
            AvaliacaoFilter();
        }

        private void AvaliacaoCategoria_SelectedIndexChanged(object sender, EventArgs e)
        {
            AvaliacaoFilter();
        }
        private void CustomerLoad(SqlConnection CN)
        {
            // Query to get the customers join the Person table
            string query = "SELECT AP_Person.Name, AP_Customer.CC, AP_Customer.Id FROM AP_Person JOIN AP_Customer ON AP_Person.CC = AP_Customer.CC";

            using (SqlCommand cmd = new SqlCommand(query, CN))
            {
                try
                {
                    if (CN.State == System.Data.ConnectionState.Open)
                    {
                        CN.Close();
                    }

                    CN.Open();

                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        string customerName = reader["Name"].ToString();
                        string customerCC = reader["CC"].ToString();
                        string customerId = reader["Id"].ToString();

                        AvalicaoClient.Items.Add(customerId + " - " + customerName + " - " + customerCC);
                    }
                    CN.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred: " + ex.Message);
                }
            }
        }
        private void BtnAvaliacao_Click(object sender, EventArgs e)
        {
            AddAvaliacao(CN);
        }
        private void AddAvaliacao(SqlConnection CN)
        {
            // Get the values from the textboxes
            string partId = AvaliacaoPecas.SelectedRows[0].Cells["Part_id"].Value.ToString();
            string customerId = AvalicaoClient.SelectedItem?.ToString().Split('-')[0].Trim();
            int.TryParse(labelAvalicao.Text, out int avaliacao);

            // Query to insert the evaluation
            string query = "INSERT INTO AP_Rates (Part_id, Cid, Rating) VALUES (@Part_id, @Customer_id, @Rating)";

            using (SqlCommand cmd = new SqlCommand(query, CN))
            {
                cmd.Parameters.AddWithValue("@Part_id", partId);
                cmd.Parameters.AddWithValue("@Customer_id", customerId);
                cmd.Parameters.AddWithValue("@Rating", avaliacao);

                try
                {
                    if (CN.State == System.Data.ConnectionState.Open)
                    {
                        CN.Close();
                    }

                    CN.Open();

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Rating added successfully.");
                    }
                    else
                    {
                        MessageBox.Show("An error occurred while adding the rating.");
                    }
                    CN.Close();
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
                    if (CN.State == System.Data.ConnectionState.Open)
                    {
                        CN.Close();
                    }

                    CN.Open();

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Engine added successfully.");
                    }
                    else
                    {
                        MessageBox.Show("An error occurred while adding the engine.");
                    }
                    CN.Close();
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
        private void EngineList(SqlConnection CN, DataGridView dgv)
        {
            string query = "SELECT * FROM AP_Engine";
            if (CN.State == System.Data.ConnectionState.Open)
            {
                CN.Close();
            }

            CN.Open();
            using (SqlDataAdapter adapter = new SqlDataAdapter(query, CN))
            {
                DataTable engineDt = new DataTable();
                adapter.Fill(engineDt);
                dgv.DataSource = engineDt;
            }
            CN.Close();
        }
        private void MotorLista_Enter(object sender, EventArgs e)
        {
            EngineList(CN, Mlista);
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

            if (MlistaCil.Value != 0)
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
            if (Mlista.SelectedRows.Count > 0)
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
                            if (CN.State == System.Data.ConnectionState.Open)
                            {
                                CN.Close();
                            }

                            CN.Open();

                            int rowsAffected = cmd.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("Engine removed successfully.");
                                EngineList(CN, Mlista);
                            }
                            else
                            {
                                MessageBox.Show("An error occurred while removing the engine.");
                            }
                            CN.Close();
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

        // LIST ENGINE IN VEHICLE ADD PAGE
        private void VehicleAdd_Enter(object sender, EventArgs e)
        {
            EngineList(CN, VmotorLista);
        }

        // VEHICLE ENGINE FILTERS
        private void VmotorID_TextChanged(object sender, EventArgs e)
        {
            VmotorFilters();
        }

        private void VmotorMarca_TextChanged(object sender, EventArgs e)
        {
            VmotorFilters();
        }

        private void VmotorCil_ValueChanged(object sender, EventArgs e)
        {
            VmotorFilters();
        }

        private void VmotorVal_ValueChanged(object sender, EventArgs e)
        {
            VmotorFilters();
        }

        private void VmotorHP_TextChanged(object sender, EventArgs e)
        {
            VmotorFilters();
        }

        private void VmotorCC_TextChanged(object sender, EventArgs e)
        {
            VmotorFilters();
        }

        private void VmotorTipo_SelectedIndexChanged(object sender, EventArgs e)
        {
            VmotorFilters();
        }

        private void VmotorComb_SelectedIndexChanged(object sender, EventArgs e)
        {
            VmotorFilters();
        }

        private void VmotorLista_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (VmotorLista.IsCurrentCellDirty)
            {
                VmotorLista.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        private void VmotorFilters()
        {
            string filterID = VmotorID.Text;
            string filterMake = VmotorMarca.Text;
            string filterCil = VmotorCil.Value.ToString();
            string filterVal = VmotorVal.Value.ToString();
            string filterPower = VmotorHP.Text;
            string filterCC = VmotorCC.Text;
            string filterType = VmotorTipo.SelectedItem?.ToString();
            string filterFuel = VmotorComb.SelectedItem?.ToString();

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

            if (VmotorCil.Value != 0)
            {
                if (!string.IsNullOrEmpty(filterExpression))
                {
                    filterExpression += " AND ";
                }
                filterExpression += string.Format("Cylinder = {0}", filterCil);
            }

            if (VmotorVal.Value != 0)
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

            (VmotorLista.DataSource as DataTable).DefaultView.RowFilter = filterExpression;
        }

        // CHANGE THE VALUES OF THE LABELS GIVEN THE SELECTED ROW
        private void VmotorLista_SelectionChanged(object sender, EventArgs e)
        {
            if (VmotorLista.SelectedRows.Count > 0)
            {
                engineID = VmotorLista.SelectedRows[0].Cells["Engine_id"].Value.ToString();
                string enginePower = VmotorLista.SelectedRows[0].Cells["Horsepower"].Value.ToString();
                string engineTorque = VmotorLista.SelectedRows[0].Cells["Torque"].Value.ToString();
                string engineCc = VmotorLista.SelectedRows[0].Cells["Cubic_cpt"].Value.ToString();
                string engineFuel = VmotorLista.SelectedRows[0].Cells["Fuel_type"].Value.ToString();

                VmotorInput.Text = engineID;
                VhpInput.Text = enginePower + " Hp";
                VbinInput.Text = engineTorque + " Nm";
                VccInput.Text = engineCc + "cc";
                VcombInput.Text = engineFuel;
            }
        }

        // ADD VEHICLE
        private void addVehicle(SqlConnection CN)
        {
            // Get the values from the textboxes
            string vehicleMake = VmarcaInput.Text;
            string vehicleModel = VmodeloInput.Text;
            string vehicleVersion = VversaoInput.Text;
            string vehicleStartYear = VinicioInput.Value.Year.ToString();
            string vehicleEndYear = VfimInput.Value.Year.ToString();
            string vehicleType = VtipoInput.Text;
            string query;
            if (VfimInput.Value.Year < 2024)
            {
                // Query to insert the vehicle
                query = "INSERT INTO AP_Vehicle ( Make, Model, Sub_model, Type, Manuf_start, Manuf_end, Vengine_id) VALUES (@Make, @Model, @Sub_model, @Type, @StartYear, @EndYear, @Engine_id)";
            }
            else
            {
                // Query to insert the vehicle
                query = "INSERT INTO AP_Vehicle ( Make, Model, Sub_model, Type, Manuf_start, Vengine_id) VALUES (@Make, @Model, @Sub_model, @Type, @StartYear, @Engine_id)";
            }


            using (SqlCommand cmd = new SqlCommand(query, CN))
            {
                cmd.Parameters.AddWithValue("@Make", vehicleMake);
                cmd.Parameters.AddWithValue("@Model", vehicleModel);
                cmd.Parameters.AddWithValue("@Sub_model", vehicleVersion);
                cmd.Parameters.AddWithValue("@Type", vehicleType);
                cmd.Parameters.AddWithValue("@StartYear", vehicleStartYear);
                cmd.Parameters.AddWithValue("@EndYear", vehicleEndYear);
                cmd.Parameters.AddWithValue("@Engine_id", engineID);

                try
                {
                    if (CN.State == System.Data.ConnectionState.Open)
                    {
                        CN.Close();
                    }

                    CN.Open();

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Vehicle added successfully.");
                    }
                    else
                    {
                        MessageBox.Show("An error occurred while adding the vehicle.");
                    }
                    CN.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred: " + ex.Message);
                }
            }

        }

        private void Vadicionar_Click(object sender, EventArgs e)
        {
            addVehicle(CN);
        }

        // LIST VEHICLES
        private void VehicleListData(SqlConnection CN, DataGridView dgv)
        {
            string query = "SELECT * FROM AP_VehicleWithEngine";
            if (CN.State == System.Data.ConnectionState.Open)
            {
                CN.Close();
            }

            CN.Open();
            using (SqlDataAdapter adapter = new SqlDataAdapter(query, CN))
            {
                DataTable vehicleDt = new DataTable();
                adapter.Fill(vehicleDt);
                dgv.DataSource = vehicleDt;
            }
            CN.Close();
        }

        private void VehicleList_Enter(object sender, EventArgs e)
        {
            VehicleListData(CN, VlistaData);
        }

        // VEHICLE FILTERS
        private void VlistaID_TextChanged(object sender, EventArgs e)
        {
            VlistaFilters();
        }

        private void VlistaMarca_TextChanged(object sender, EventArgs e)
        {
            VlistaFilters();
        }

        private void VlistaModelo_TextChanged(object sender, EventArgs e)
        {
            VlistaFilters();
        }

        private void VlistaVersao_TextChanged(object sender, EventArgs e)
        {
            VlistaFilters();
        }

        private void VlistaTipo_SelectedIndexChanged(object sender, EventArgs e)
        {
            VlistaFilters();
        }

        private void VlistaComb_SelectedIndexChanged(object sender, EventArgs e)
        {
            VlistaFilters();
        }

        private void VlistaEngineID_TextChanged(object sender, EventArgs e)
        {
            VlistaFilters();
        }

        private void VlistaData_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (VlistaData.IsCurrentCellDirty)
            {
                VlistaData.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        private void VlistaFilters()
        {
            string filterID = VlistaID.Text;
            string filterMake = VlistaMarca.Text;
            string filterModel = VlistaModelo.Text;
            string filterVersion = VlistaVersao.Text;
            string filterType = VlistaTipo.SelectedItem?.ToString();
            string filterFuel = VlistaComb.SelectedItem?.ToString();
            string filterEngine = VlistaEngineID.Text;

            string filterExpression = string.Empty;

            if (!string.IsNullOrEmpty(filterID))
            {
                filterExpression += string.Format("Vehicle_id LIKE '%{0}%'", filterID);
            }

            if (!string.IsNullOrEmpty(filterMake))
            {
                if (!string.IsNullOrEmpty(filterExpression))
                {
                    filterExpression += " AND ";
                }
                filterExpression += string.Format("Make LIKE '{0}%'", filterMake);
            }

            if (!string.IsNullOrEmpty(filterModel))
            {
                if (!string.IsNullOrEmpty(filterExpression))
                {
                    filterExpression += " AND ";
                }
                filterExpression += string.Format("Model LIKE '{0}%'", filterModel);
            }

            if (!string.IsNullOrEmpty(filterVersion))
            {
                if (!string.IsNullOrEmpty(filterExpression))
                {
                    filterExpression += " AND ";
                }
                filterExpression += string.Format("Sub_model LIKE '{0}%'", filterVersion);
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

            if (!string.IsNullOrEmpty(filterEngine))
            {
                if (!string.IsNullOrEmpty(filterExpression))
                {
                    filterExpression += " AND ";
                }
                filterExpression += string.Format("Engine_id LIKE '%{0}%'", filterEngine);
            }

            (VlistaData.DataSource as DataTable).DefaultView.RowFilter = filterExpression;
        }

        // REMOVE VEHICLE

        private void VlistaRemove_Click(object sender, EventArgs e)
        {
            if (VlistaData.SelectedRows.Count > 0)
            {
                var confirmResult = MessageBox.Show("Tem a certeza que deseja remover o ve�culo selecionado?", "Confirmar", MessageBoxButtons.YesNo);
                if (confirmResult == DialogResult.Yes)
                {
                    string vehicleID = VlistaData.SelectedRows[0].Cells["Vehicle_id"].Value.ToString();
                    string query = "DELETE FROM AP_Vehicle WHERE Vehicle_id = @ID";

                    using (SqlCommand cmd = new SqlCommand(query, CN))
                    {
                        cmd.Parameters.AddWithValue("@ID", vehicleID);

                        try
                        {
                            if (CN.State == System.Data.ConnectionState.Open)
                            {
                                CN.Close();
                            }

                            CN.Open();

                            int rowsAffected = cmd.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("Vehicle removed successfully.");
                                VehicleListData(CN, VlistaData);
                            }
                            else
                            {
                                MessageBox.Show("An error occurred while removing the vehicle.");
                            }
                            CN.Close();
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
                MessageBox.Show("Por favor selecione um ve�culo para remover.");
            }

        }
        //
        //
        // FORNECEDOR CODE
        //
        //
        private void AddFornecedor(SqlConnection CN)
        {
            // Get the values from the textboxes
            string fornecedorName = FornecedorNome.Text;
            string address = FornecedorAddr.Text;
            string contact = FornecedorCont.Text;

            // Query to insert the fornecedor
            string query = "INSERT INTO AP_Supplier (Name, Phone, Address) VALUES (@Name, @Phone, @Address)";

            using (SqlCommand cmd = new SqlCommand(query, CN))
            {
                cmd.Parameters.AddWithValue("@Name", fornecedorName);
                cmd.Parameters.AddWithValue("@Phone", contact);
                cmd.Parameters.AddWithValue("@Address", address);
                try
                {
                    if (CN.State == System.Data.ConnectionState.Open)
                    {
                        CN.Close();
                    }

                    CN.Open();

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Fornecedor added successfully.");
                    }
                    else
                    {
                        MessageBox.Show("An error occurred while adding the fornecedor.");
                    }
                    CN.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred: " + ex.Message);
                }
            }

        }


        // MAKE ORDER
        private void OrderCustomerLoad(SqlConnection CN)
        {
            //query to get the customer
            string query = "SELECT AP_Customer.Id, AP_Person.Name FROM AP_Person JOIN AP_Customer ON AP_Person.CC = AP_Customer.CC";

            using (SqlCommand cmd = new SqlCommand(query, CN))
            {
                try
                {
                    if (CN.State == System.Data.ConnectionState.Open)
                    {
                        CN.Close();
                    }
                    CN.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        string CustomerName = reader["Name"].ToString();
                        string CustomerId = reader["Id"].ToString();

                        OcustomerID.Items.Add(CustomerId + " - " + CustomerName);
                    }
                    CN.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred: " + ex.Message);
                }
            }
        }

        private void addOrder(SqlConnection CN)
        {
            // Get the values from the textboxes
            string customerId = OcustomerID.SelectedItem?.ToString().Split('-')[0].Trim();
            string shippingAddress = Omorada.Text;

            var orderItems = new DataTable();
            orderItems.Columns.Add("ID", typeof(string));
            orderItems.Columns.Add("Quantidade", typeof(int));

            foreach (DataRow row in OrderCart.Rows)
            {
                string itemId = row["ID"].ToString();
                int quantity = Convert.ToInt32(row["Quantidade"]);
                orderItems.Rows.Add(itemId, quantity);
            }

            if (CN.State == System.Data.ConnectionState.Open)
            {
                CN.Close();
            }

            CN.InfoMessage += (sender, args) => Console.WriteLine(args.Message);
            CN.Open();

            try
            {
                using (SqlCommand cmd = new SqlCommand("AP_CreateOrder", CN))
                {
                    cmd.CommandType = CommandType.StoredProcedure;


                    cmd.Parameters.AddWithValue("@CustomerId", customerId);
                    cmd.Parameters.AddWithValue("@ShippingAddress", shippingAddress);

                    SqlParameter itemsTable = cmd.Parameters.AddWithValue("@OrderItems", orderItems);
                    itemsTable.SqlDbType = SqlDbType.Structured;
                    itemsTable.TypeName = "dbo.OrderItems";

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Order and stock update successful!");
                    }
                    else
                    {
                        MessageBox.Show("No rows affected.");
                    }
                    CN.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message);
            }

        }
        private void OrderPartsLoad(SqlConnection CN)
        {
            string query = "SELECT * FROM AP_PartDetailsView;";
            if (CN.State == System.Data.ConnectionState.Open)
            {
                CN.Close();
            }

            CN.Open();

            using (SqlDataAdapter adapter = new SqlDataAdapter(query, CN))
            {
                DataTable pecasDt = new DataTable();
                adapter.Fill(pecasDt);
                OpecaList.DataSource = pecasDt;
            }
            CN.Close();
        }

        private void OrderPartsFilter(SqlConnection CN)
        {
            string filterID = OpecaID.Text;
            string filterName = OpecaNome.Text;
            string filterMarca = OpecaMarca.Text;
            string filterCategoria = OpecaCat.Text;
            string filterVehicle = OveiculoID.Text;

            string filterExpression = string.Empty;

            if (!string.IsNullOrEmpty(filterID))
            {
                filterExpression += string.Format("ID LIKE '%{0}%'", filterID);
            }
            if (!string.IsNullOrEmpty(filterName))
            {
                if (!string.IsNullOrEmpty(filterExpression))
                {
                    filterExpression += " AND ";
                }
                filterExpression += string.Format("Nome LIKE '%{0}%'", filterName);
            }
            if (!string.IsNullOrEmpty(filterMarca))
            {
                if (!string.IsNullOrEmpty(filterExpression))
                {
                    filterExpression += " AND ";
                }
                filterExpression += string.Format("Marca LIKE '{0}%'", filterMarca);
            }
            if (!string.IsNullOrEmpty(filterCategoria))
            {
                if (!string.IsNullOrEmpty(filterExpression))
                {
                    filterExpression += " AND ";
                }
                filterExpression += string.Format("Categoria LIKE '%{0}%'", filterCategoria);
            }
            if (!string.IsNullOrEmpty(filterVehicle))
            {
                if (!string.IsNullOrEmpty(filterExpression))
                {
                    filterExpression += " AND ";
                }
                filterExpression += string.Format("ID_Ve�culo LIKE '%{0}%'", filterVehicle);
            }
            if (OpecaStock.Checked)
            {
                if (!string.IsNullOrEmpty(filterExpression))
                {
                    filterExpression += " AND ";
                }
                filterExpression += string.Format("Stock > 0");
            }

            (OpecaList.DataSource as DataTable).DefaultView.RowFilter = filterExpression;

        }

        private void OpecaID_TextChanged(object sender, EventArgs e)
        {
            OrderPartsFilter(CN);
        }

        private void OpecaNome_TextChanged(object sender, EventArgs e)
        {
            OrderPartsFilter(CN);
        }

        private void OpecaMarca_TextChanged(object sender, EventArgs e)
        {
            OrderPartsFilter(CN);
        }

        private void OpecaCat_SelectedIndexChanged(object sender, EventArgs e)
        {
            OrderPartsFilter(CN);
        }

        private void OveiculoID_TextChanged(object sender, EventArgs e)
        {
            OrderPartsFilter(CN);
        }

        private void OpecaStock_CheckedChanged(object sender, EventArgs e)
        {
            OrderPartsFilter(CN);
        }

        private void EncomendaAdd_Enter(object sender, EventArgs e)
        {
            if (OrderCart.Columns.Count == 0)
            {
                OrderCart.Columns.Add("ID", typeof(string));
                OrderCart.Columns.Add("Nome", typeof(string));
                OrderCart.Columns.Add("Pre�o", typeof(string));
                OrderCart.Columns.Add("ID_Ve�culo", typeof(string));
                OrderCart.Columns.Add("Quantidade", typeof(string));
            }

            OencomendaList.DataSource = OrderCart;
            OrderPartsLoad(CN);
            OrderCustomerLoad(CN);
        }

        // ADD PART TO ORDER

        private void OaddPart_Click(object sender, EventArgs e)
        {
            if (OpecaList.SelectedRows.Count > 0)
            {
                string partID = OpecaList.SelectedRows[0].Cells["ID"].Value.ToString();
                string partName = OpecaList.SelectedRows[0].Cells["Nome"].Value.ToString();
                string partPrice = OpecaList.SelectedRows[0].Cells["Pre�o"].Value.ToString();
                string partVehicleId = OpecaList.SelectedRows[0].Cells["ID_Ve�culo"].Value.ToString();
                int partQuantity = (int)OqtyAdd.Value;
                if (partQuantity == 0)
                {
                    MessageBox.Show("A quantidade tem de ser maior que 0");
                    return;
                }

                // ADD TO THE OTHER DATAGRIDVIEW "CART"
                OrderCart.Rows.Add(partID, partName, partPrice, partVehicleId, partQuantity);
            }
            else
            {
                MessageBox.Show("Por favor selecione uma pe�a para adicionar � encomenda.");
            }
        }

        private void OremPart_Click(object sender, EventArgs e)
        {
            if (OencomendaList.SelectedRows.Count > 0)
            {

                int partQuantity = (int)OqtyRmv.Value;
                if (OrderCart.Rows[OencomendaList.SelectedRows[0].Index]["Quantidade"].ToString() == "0" || (int)OrderCart.Rows[OencomendaList.SelectedRows[0].Index]["Quantidade"] < partQuantity)
                {
                    OrderCart.Rows.RemoveAt(OencomendaList.SelectedRows[0].Index);
                }
                else
                {
                    OrderCart.Rows[OencomendaList.SelectedRows[0].Index]["Quantidade"] = (int)OrderCart.Rows[OencomendaList.SelectedRows[0].Index]["Quantidade"] - partQuantity;
                }

            }
            else
            {
                MessageBox.Show("Por favor selecione uma pe�a para remover da encomenda.");
            }

        }


        private void OaddEncomenda_Click(object sender, EventArgs e)
        {
            addOrder(CN);
        }

        private void FornecedortBtn_Click(object sender, EventArgs e)
        {
            AddFornecedor(CN);
        }
        private void LoadFornecedor(SqlConnection CN)
        {
            string query = "SELECT * FROM AP_Supplier";
            if (CN.State == System.Data.ConnectionState.Open)
            {
                CN.Close();
            }

            CN.Open();
            using (SqlDataAdapter adapter = new SqlDataAdapter(query, CN))
            {
                DataTable fornecedorDt = new DataTable();
                adapter.Fill(fornecedorDt);
                FornecedorData.DataSource = fornecedorDt;
            }
            CN.Close();
        }
        private void FornecedorList_Enter(object sender, EventArgs e)
        {
            LoadFornecedor(CN);
        }
        private void FornecedorFilter()
        {
            int filterid = 0;
            int.TryParse(FornecedorIdFilter.Text, out filterid);
            string filterName = FornecedorNomeFilter.Text;

            string filterExpression = string.Empty;

            if (!string.IsNullOrEmpty(filterName))
            {
                filterExpression += string.Format("Name LIKE '%{0}%'", filterName);
            }
            if (filterid != 0)
            {
                if (!string.IsNullOrEmpty(filterExpression))
                {
                    filterExpression += " AND ";
                }
                filterExpression += string.Format("Supplier_id = {0}", filterid);
            }

            (FornecedorData.DataSource as DataTable).DefaultView.RowFilter = filterExpression;
        }

        private void FornecedorIdFilter_TextChanged(object sender, EventArgs e)
        {
            FornecedorFilter();
        }

        private void FornecedorNomeFilter_TextChanged(object sender, EventArgs e)
        {
            FornecedorFilter();
        }

        //
        //
        // ORDER CODE
        //
        //
        private void Load_Order(object sender, EventArgs e)
        {
            LoadOrder(CN);
            OrderClient_Load(CN);
        }
        private void LoadOrder(SqlConnection CN)
        {
            string query = "SELECT * FROM AP_OrderDetailsView";
            if (CN.State == System.Data.ConnectionState.Open)
            {
                CN.Close();
            }

            CN.Open();
            using (SqlDataAdapter adapter = new SqlDataAdapter(query, CN))
            {
                DataTable orderDt = new DataTable();
                adapter.Fill(orderDt);
                PesquisaOrder.DataSource = orderDt;
            }
            CN.Close();
        }
        private void OrderClient_Load(SqlConnection CN)
        {
            // Query to get the customers join the Person table
            string query = "SELECT AP_Person.Name, AP_Customer.CC, AP_Customer.Id FROM AP_Person JOIN AP_Customer ON AP_Person.CC = AP_Customer.CC";

            using (SqlCommand cmd = new SqlCommand(query, CN))
            {
                try
                {
                    if (CN.State == System.Data.ConnectionState.Open)
                    {
                        CN.Close();
                    }

                    CN.Open();

                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        string customerName = reader["Name"].ToString();
                        string customerCC = reader["CC"].ToString();
                        string customerId = reader["Id"].ToString();

                        OrderClienteFilter.Items.Add(customerId + " - " + customerName + " - " + customerCC);
                    }
                    CN.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred: " + ex.Message);
                }
            }
        }
        private void OrderFilter()
        {
            string filterPart = OrderPartIdFilter.Text;
            int filterCliente = 0;

            if (OrderClienteFilter.SelectedItem != null)
            {
                string selectedValue = OrderClienteFilter.SelectedItem.ToString().Split('-')[0].Trim();
                int.TryParse(selectedValue, out filterCliente);
            }

            string filterExpression = string.Empty;

            if (!string.IsNullOrEmpty(filterPart))
            {
                filterExpression += string.Format("Pe�a_ID LIKE '%{0}%'", filterPart);
            }
            if (filterCliente != 0)
            {
                if (!string.IsNullOrEmpty(filterExpression))
                {
                    filterExpression += " AND ";
                }
                filterExpression += string.Format("Cliente = {0}", filterCliente);
            }

            (PesquisaOrder.DataSource as DataTable).DefaultView.RowFilter = filterExpression;
        }

        private void OrderClienteFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            OrderFilter();
        }

        private void OrderPartIdFilter_TextChanged(object sender, EventArgs e)
        {
            OrderFilter();
        }
        private void OrderFilter_Clean(object sender, EventArgs e)
        {
            OrderPartIdFilter.Text = "";
            OrderClienteFilter.Items.Clear();
        }

    }
}
