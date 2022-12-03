using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Configuration;
using System.Data.SqlClient;
using MySql.Data.MySqlClient;
using ZXing;
using barcodeB;
using barcodeD;

namespace barcode
{
    public partial class Form1 : Form
    {
        string codeBar = BarCode.CodeBar;
        int counter = 0; 
        int curPage; 

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            comboBox1.DropDownStyle = ComboBoxStyle.DropDown;

            timer1.Interval = 60000;
            timer1.Start();

            DBShop.SqlConnection = new MySqlConnection(ConfigurationManager.ConnectionStrings["DataBase"].ConnectionString);
            DBShop.SqlConnection.Open();

            if (DBShop.SqlConnection.State == ConnectionState.Open){
                MessageBox.Show("Подключение установлено");
            }
            else{
                MessageBox.Show("Подключение не установлено");
            }
        }

        private BarcodeFormat GetFormat()
        {
            switch (comboBox1.Text)
            {
                case "CODEBAR": return BarcodeFormat.CODABAR;
                case "CODE_39": return BarcodeFormat.CODE_39;
                case "CODE_93": return BarcodeFormat.CODE_93;
                case "CODE_128": return BarcodeFormat.CODE_128;
                case "QR_CODE": return BarcodeFormat.QR_CODE;
                case "MSI": return BarcodeFormat.MSI;
                case "DATA_MATRIX": return BarcodeFormat.DATA_MATRIX;
                default: return BarcodeFormat.CODABAR;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                codeBar = "";
                DataSet dataSet = new DataSet();
                DataTable dataTable = new DataTable();
                MySqlDataAdapter dataAdapter = new MySqlDataAdapter();
                MySqlCommand command = new MySqlCommand($@"SELECT `product`.`name` AS `Название`, 
                                                            `category`.`name` AS `Категория`, 
                                                            `product`.`priceEnd` AS `Цена`, 
                                                            `manufactor`.`name` AS `Производитель` 
                                                        FROM `product` INNER JOIN `category` ON(`product`.`categoryId` = `category`.`id`) 
                                                            INNER JOIN `manufactor` ON(`product`.`manufactorId` = `manufactor`.`id`) 
                                                        WHERE `product`.`print` = 1",
                                                        DBShop.SqlConnection);
                dataAdapter.SelectCommand = command;
                dataAdapter.Fill(dataTable);

                foreach (DataRow row in dataTable.Rows)
                {
                    var cells = row.ItemArray;
                    foreach (object cell in cells)
                        codeBar += cell + "   ";
                    codeBar += '\n';

                }
                pictureBox1.Image = BarCode.CreateCode(codeBar, pictureBox1.Width, pictureBox1.Height, GetFormat());
                MessageBox.Show(codeBar);
            }
            catch (Exception)
            {

            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                if (DateTime.Now.Hour == 15 && (DateTime.Now.Minute == 0))
                {
                    DataSet dataSet = new DataSet();
                    DataTable dataTable = new DataTable();
                    MySqlDataAdapter dataAdapter = new MySqlDataAdapter();
                    MySqlCommand command = new MySqlCommand($@"UPDATE `product`
                                                            SET `print` = false 
                                                            WHERE `print` = true",
                                                            DBShop.SqlConnection);

                    PrintDocument printDocument = new PrintDocument();
                    printDocument.BeginPrint += BeginPrint;
                    printDocument.PrintPage += PrintPageHandler;
                    PrintDialog printDialog = new PrintDialog();
                    printDialog.Document = printDocument;
                    if (printDialog.ShowDialog() == DialogResult.OK)
                        printDialog.Document.Print(); // печатаем
                    dataAdapter.SelectCommand = command;
                    dataAdapter.Fill(dataTable);
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void BeginPrint(object sender, PrintEventArgs e)
        {
            counter = 0;
            curPage = 1;
        }

        void PrintPageHandler(object sender, PrintPageEventArgs e)
        {
            int y = 0;
            DataSet dataSet = new DataSet();
            DataTable dataTable = new DataTable();
            MySqlDataAdapter dataAdapter = new MySqlDataAdapter();
            MySqlCommand command = new MySqlCommand($@"SELECT `product`.`name` AS `Название`, 
                                                            `category`.`name` AS `Категория`, 
                                                            `product`.`priceEnd` AS `Цена`, 
                                                            `manufactor`.`name` AS `Производитель` 
                                                        FROM `product` INNER JOIN `category` ON(`product`.`categoryId` = `category`.`id`) 
                                                            INNER JOIN `manufactor` ON(`product`.`manufactorId` = `manufactor`.`id`) 
                                                        WHERE `product`.`print` = 1",
                                                    DBShop.SqlConnection);
            dataAdapter.SelectCommand = command;
            dataAdapter.Fill(dataTable);

            int nLines = (int)e.MarginBounds.Height / (166);
            int nPages = (int)dataTable.Rows.Count / nLines + 1;

            int i = 0;
            DataRow row;

            for (int iterator = 0; iterator < dataTable.Rows.Count; iterator++)
            {
                row = dataTable.Rows[counter];
                var cells = row.ItemArray;
                foreach (object cell in cells)
                    codeBar += cell + "   ";
                codeBar = codeBar.Insert(codeBar.IndexOf(',') + 3, "₽");
                e.Graphics.DrawImage(BarCode.CreateCode(codeBar, 150, 150, GetFormat()), e.PageBounds.X, y, 150, 150);
                y += 150;
                e.Graphics.DrawString(codeBar, new Font("Arial", 7), Brushes.Black, 0, 2 + y);
                y += 16;
                codeBar = "";
                i += 1;
                counter += 1;
                if (i >= nLines || counter >= dataTable.Rows.Count)
                {
                    break;
                }
            }

            e.HasMorePages = false;
            if (curPage < nPages) {
                curPage += 1;
                e.HasMorePages = true;
            }
        }
    }
}
