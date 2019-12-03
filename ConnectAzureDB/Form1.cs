using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ConnectAzureDB
{
    delegate void SetTextCallback();
    
    public partial class Form1 : Form
    {
        Button button;

        Label label;

        // SQL Connection to AZURE
        SqlConnection connection;

        // Timer to execute stored procedure
        System.Windows.Forms.Timer SqlTimer = new System.Windows.Forms.Timer();

        // Temporary buffer dictionary to save orderid, print and display
        Dictionary<String, OrderItems> orders = new Dictionary<string, OrderItems>();

        // BackgroundWorker
        BackgroundWorker worker;

        SortedItems m_orderIds = new SortedItems();

        bool m_ButtonEnabled = false;

        bool m_labelEnabled = true;

        public Form1()
        {
            InitializeComponent();
            Setup();

            // Clear buffer dictionary
            orders.Clear();

            // Configure background worker thread
            worker = new BackgroundWorker();
            worker.DoWork += new DoWorkEventHandler(BackgroundWorker30Seconds);
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(BackgroundWorkerCompleted);

            // Configure timer
            SqlTimer.Interval = 30000;
            SqlTimer.Tick += new EventHandler(SQLTimer_Tick);
            SqlTimer.Enabled = true;
        }

        void Setup()
        {
            Panel left = new Panel();
            this.Padding = new Padding(2);
            left.Dock = DockStyle.Left;
            left.AutoSize = true;
            Panel right = new Panel();
            right.Dock = DockStyle.Fill;
            right.Padding = new Padding(4);

            button = new Button();
            button.Text = "Go Back";
            button.Dock = DockStyle.Left;
            button.Enabled = true;
            m_ButtonEnabled = false;
            button.Click += new EventHandler(button_Click);
            left.Controls.Add(button);

            label = new Label();
            label.Dock = DockStyle.Fill;
            label.Font = new Font("Arial",24,FontStyle.Regular);
            label.Enabled = true;
            m_labelEnabled = false;
            label.Text = "";
            label.Click += new EventHandler(label_Click);
            right.Controls.Add(label);
            this.Controls.Add(right);
            this.Controls.Add(left);
            
        }
        private void button_Click(object sender, System.EventArgs e)
        {
            if (!m_ButtonEnabled)
                return;

            // Go to previous page
            m_orderIds.SetBackElement();
            String displayText = m_orderIds.GetDisplayText();

            // Get Go Back Status
            if (true == m_orderIds.GetGoBackStatus())
            {
                button.Enabled = true;
            }
            else
            {
                button.Enabled = false;
            }

            if (true == m_orderIds.GetNextPageStatus())
            {
                m_labelEnabled = true;
                label.ForeColor = Color.Green;
            }
            else
            {
                m_labelEnabled = false;
                label.ForeColor = Color.Red;
            }

            label.Text = displayText;
        }

        private void label_Click(object sender, System.EventArgs e)
        {
            if (!m_labelEnabled)
                return;

            // Go to next page
            m_orderIds.SetNextElement();
            String displayText = m_orderIds.GetDisplayText();

            // Get Go Back Status
            if (true == m_orderIds.GetGoBackStatus())
            {
                button.Enabled = true;
            }
            else
            {
                button.Enabled = false;
            }

            if (true == m_orderIds.GetNextPageStatus())
            {
                m_labelEnabled = true;
                label.ForeColor = Color.Green;
            }
            else
            {
                m_labelEnabled = false;
                label.ForeColor = Color.Red;
            }

            label.Text = displayText;
        }

        void BackgroundWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // Insert extracted values into sorted items
            if(orders.Keys.Count > 0)
            {
                foreach(KeyValuePair<String, OrderItems> order in orders)
                {
                    bool newOrder = m_orderIds.OrderedInsert(order.Key, order.Value);

                    if(true == newOrder)
                    {
                        // Print with local printer
                        // Add code to print with local printer
                        String printText = order.Value.printOutput;
                        PrintDocument p = new PrintDocument();
                        p.PrintPage += delegate (object sender1, PrintPageEventArgs e1)
                        {
                            e1.Graphics.DrawString(printText, new Font("Arial",24), new SolidBrush(Color.Black), 100, 100);
                        };

                        try
                        {
                            p.Print();
                        }
                        catch(Exception ex)
                        {

                        }
                    }
                }
            }

            // Initialize the orderIds if not already initialized
            m_orderIds.Initialize();
            String displayText = m_orderIds.GetDisplayText();

            // Get Go Back Status
            if(true == m_orderIds.GetGoBackStatus())
            {
                button.Enabled = true;
            }
            else
            {
                button.Enabled = false;
            }
            
            // Get Next page status
            if(true == m_orderIds.GetNextPageStatus())
            {
                m_labelEnabled = true;
                label.ForeColor = Color.Green;
            }
            else
            {
                m_labelEnabled = false;
                label.ForeColor = Color.Red;
            }
            label.Text = displayText;
        }

        void BackgroundWorker30Seconds(object sender, DoWorkEventArgs e)
        {
            // Establish AZURE connection
            try
            {
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
                builder.DataSource = "yifangrepl1.database.windows.net";
                builder.UserID = "troubby";
                builder.Password = "Mk0$ss$71@hOppl";
                builder.InitialCatalog = "yifang";

                using (connection = new SqlConnection(builder.ConnectionString))
                {
                    connection.Open();

                    StringBuilder sb = new StringBuilder();
                    sb.Append("EXEC LP_INSERT_NEW_ORDERS_TO_DISPLAY_ON_SCREEN");
                    String sql = sb.ToString();

                    List<String> orderIds = new List<String>();

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                // Get order key
                                String strOrderKey = reader.GetString(0);

                                orderIds.Add(strOrderKey);
                            }
                        }
                    }

                    foreach(String orderId in orderIds)
                    {
                        String strPrint = String.Empty, strDisplay = String.Empty;
                        StringBuilder sbDisplay = new StringBuilder();
                        sbDisplay.Append("EXEC LP_DISPLAY_ORDER_KEY_ON_SCREEN '");
                        sbDisplay.Append(orderId);
                        sbDisplay.Append("'");
                        String sqlDisplay = sbDisplay.ToString();
                        using (SqlCommand command = new SqlCommand(sqlDisplay, connection))
                        {
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    strDisplay = reader.GetString(0);
                                }
                            }
                        }
                                    
                        StringBuilder sbPrint = new StringBuilder();
                        sbPrint.Append("EXEC LP_DISPLAY_ORDER_KEY_ON_PRINTER '");
                        sbPrint.Append(orderId);
                        sbPrint.Append("'");
                        String sqlPrint = sbPrint.ToString();
                        using (SqlCommand command = new SqlCommand(sqlPrint, connection))
                        {
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    strPrint = reader.GetString(0);
                                }
                            }
                        }

                        OrderItems item = new OrderItems();
                        item.orderId = orderId;
                        item.displayOutput = strDisplay;
                        item.printOutput = strPrint;

                        orders.Add(orderId, item);
                    }
                }
            }
            catch (SqlException e1)
            { }
        }

        void SQLTimer_Tick(object sender, EventArgs e)
        {
            // Clear buffer dictionary
            orders.Clear();

            // Worker background thread for processing
            worker.RunWorkerAsync();
        }
    }
}
