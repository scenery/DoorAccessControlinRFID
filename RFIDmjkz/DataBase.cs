using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace RFIDmjkz
{
    class DataBase
    {
        public SqlConnection lianjie()
        {
            
                String strCon = System.Configuration.ConfigurationManager.ConnectionStrings["sqlCon"].ToString();
                SqlConnection conn = new SqlConnection();
                conn.Open();
                return conn;
        }
        //连接是否能够打开
        public bool ConIsOpen()
        {
            String strCon = System.Configuration.ConfigurationManager.ConnectionStrings["sqlCon"].ToString();
            SqlConnection conn = new SqlConnection();
            try
            {
                conn.ConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["sqlCon"].ToString();
                conn.Open();

                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                //打开了才关闭
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
        }
    }
}
//        public string[] select(string look)  //数据库查询函数，形参为查询的关键词，返回值为查询结果的数组
//        {
//            int i = 0;
//            string[] d = new string[20];
//            SqlConnection conn = new SqlConnection();                       //连接数据库

//            try
//            {
//                conn.Open();                                            //打开数据库
//                SqlCommand comm = new SqlCommand("select * from RFID_sql", conn);
//                if (conn.State == ConnectionState.Closed)
//                {
//                    conn.Open();
//                }
//                SqlDataReader sql = comm.ExecuteReader();               //定义datareader
//                try
//                {
//                    while (sql.HasRows && sql.Read())                   //判断数据库是否有数据，且逐行读取
//                    {
//                        if (look != "")                                            //通过形参判断是全部查询，还是条件查询
//                        {
//                            if (sql["UID"].ToString() == look)        //当前读取行符合条件
//                            {
//                                d[0] = sql["UID"].ToString();
//                            }
//                        }
        
//                        i++;
//                    }
//                    sql.Close();                                           //关闭数据库连接
//                }
//                catch (Exception)
//                {

//                }
//            }
//            catch (Exception)
//            {

//            }    
//            conn.Close();
//            return d;                                           //返回数据库查询结果
//        }

//    }
//}
  