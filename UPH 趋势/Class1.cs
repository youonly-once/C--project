using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
 using System.Data.SqlClient;
namespace UPH_趋势
{

    public class SqlHepler
    {
        //1.接着需要创建一个名为SqlHepler类，然后创建一个方法来获取app.config文件中配置的连接字符串。
        public static string GetSqlConnctingString()
        {
            return "server=10.139.114.219;database=mesapp;uid=hrled;pwd=hrled";
         
        }

        //2.下面来封装第一个SqlHepler方法，封装一个执行的sql 返回受影响的行数。
        public static int ExecuteNonQuery(string sqlText, params SqlParameter[] parameters)
        {
            using (SqlConnection conn = new SqlConnection(GetSqlConnctingString()))
            {
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    conn.Open();
                    cmd.CommandText = sqlText;
                    cmd.Parameters.AddRange(parameters);
                    return cmd.ExecuteNonQuery();
                }
            }
        }
        //3.继续封装一个查询操作，返回查询结果中的第一行第一列的值
        public static object ExecuteScalar(string sqlText, params SqlParameter[] parameters)
        {
            using (SqlConnection conn = new SqlConnection(GetSqlConnctingString()))
            {
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    conn.Open();
                    cmd.CommandText = sqlText;
                    cmd.Parameters.AddRange(parameters);
                    return cmd.ExecuteScalar();
                }
            }
        }
        //4.在封装个常用的查询方法，返回一个DataTable
        public static DataTable ExecuteDataTable(string sqlText, params SqlParameter[] parameters)
        {
            using (SqlDataAdapter adapter = new SqlDataAdapter(sqlText, GetSqlConnctingString()))
            {
                DataTable dt = new DataTable();
                adapter.SelectCommand.Parameters.AddRange(parameters);
                adapter.Fill(dt);
                return dt;
            }
        }
        //5.最后在写封装一个查询方法,该方法返回的是一个SqlDataReader类型
        public static SqlDataReader ExecuteReader(string sqlText, params SqlDataReader[] parameters)
        {
            //SqlDataReader要求，它读取数据的时候有，它独占它的SqlConnection对象，而且SqlConnection必须是Open状态
            SqlConnection conn = new SqlConnection(GetSqlConnctingString());//不要释放连接，因为后面还需要连接打开状态
            SqlCommand cmd = conn.CreateCommand();
            conn.Open();
            cmd.CommandText = sqlText;
            cmd.Parameters.AddRange(parameters);
            //CommandBehavior.CloseConnection当SqlDataReader释放的时候，顺便把SqlConnection对象也释放掉
            return cmd.ExecuteReader(CommandBehavior.CloseConnection);
        }


        //6.调用存储过程
        public static DataTable StoredProcedure(string name, string value)
        {
            SqlConnection conStr = new SqlConnection(GetSqlConnctingString());
            SqlCommand comStr = new SqlCommand(name, conStr);
            comStr.CommandType = CommandType.StoredProcedure;
            if (value != null)
            {
                comStr.Parameters.Add("@code", SqlDbType.Int).Value = value;
            }
            
            conStr.Open();//打开数据库连接
            SqlDataAdapter sqlData = new SqlDataAdapter(comStr);
            DataTable dt = new DataTable();
            sqlData.Fill(dt);
            return dt;
            //conStr.Close();//关闭连接 
        }

    }
}
