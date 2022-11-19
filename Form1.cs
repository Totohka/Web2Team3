using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Configuration;
using System.Data.SqlClient;

using ZXing;
using ZXing.Rendering;
using ZXing.Common;
using ZXing.QrCode;

namespace barcode
{
    public partial class Form1 : Form
    {
        private SqlConnection sqlConnection = null;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            comboBox1.DropDownStyle = ComboBoxStyle.DropDown;

            sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["DataBase"].ConnectionString);
            sqlConnection.Open();

            if (sqlConnection.State == ConnectionState.Open)
            {
                MessageBox.Show("Подключение установлено");
            }
        }

        public static Image CreateCode(string text, int w, int h, BarcodeFormat format)
        {
            try
            {
                BarcodeWriter writer = new BarcodeWriter
                {
                    Format = format,
                    Options = new QrCodeEncodingOptions
                    {
                        Width = w,
                        Height = h,
                        CharacterSet = "UTF-8"
                    },
                    Renderer = new BitmapRenderer()
                };
                return writer.Write(text);
            }
            catch (Exception)
            {
                
            }
            return null;
        }

        public static string[] CodeScan(Bitmap bmp)
        {
            try
            {
                BarcodeReader reader = new BarcodeReader
                {
                    AutoRotate = true,
                    TryInverted = true,
                    Options = new DecodingOptions
                    {
                        TryHarder = true
                    }
                };
                Result[] results = reader.DecodeMultiple(bmp);
                if (results != null)
                {
                    return results.Where(x => x != null && !string.IsNullOrEmpty(x.Text)).Select(x => x.Text).ToArray();
                }
            }
            catch(Exception)
            {

            }
            return null;
        }
        
        public static string DecodeImage(Image img)
        {
            string outString = "";
            string[] results = CodeScan((Bitmap)img);

            if (results != null)
            {
                outString = string.Join(Environment.NewLine + Environment.NewLine, results);
            }

            return outString;
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

        private void button3_Click(object sender, EventArgs e)
        {
            //загрузка(не нужна)
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                MessageBox.Show(DecodeImage(pictureBox1.Image));

                SqlDataAdapter dataAdapter = new SqlDataAdapter(
                    $@"SELECT Product.Title AS Название, Category.Title AS Категория, Manufactor.Title AS Производитель, PriceEnd AS Стоимость from 
                        Product INNER JOIN Category ON(Product.IdCategory = Category.Id)
                        INNER JOIN Manufactor ON(Product.IdManufactor = Manufactor.Id)
                    WHERE Product.Id = {DecodeImage(pictureBox1.Image)}", 
                    sqlConnection
                    );

                DataSet dataSet = new DataSet();
                dataAdapter.Fill(dataSet);

                string codeBar = "";

                foreach (DataTable dt in dataSet.Tables)
                {                                                    
                    foreach (DataColumn column in dt.Columns)
                        codeBar += column.ColumnName + "   "; // все названия колонок
                    codeBar += "\n";
                    // перебор всех строк таблицы
                    foreach (DataRow row in dt.Rows)
                    {
                        // получаем все ячейки строки
                        var cells = row.ItemArray;
                        foreach (object cell in cells)
                            codeBar += cell + "   ";
                        codeBar += "\n";
                    }
                }
                MessageBox.Show(codeBar);
                //MessageBox.Show(DecodeImage(pictureBox1.Image));
            }
            catch (Exception)
            {

            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //сохранение не нужно
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            pictureBox1.Image = CreateCode(textBox1.Text, pictureBox1.Width, pictureBox1.Height, GetFormat());
        }
    }
}
