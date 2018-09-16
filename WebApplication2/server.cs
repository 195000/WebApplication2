using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.SignalR;
using System.Diagnostics;

namespace WebApplication2
{
    public class ChatHub : Hub
    {

        private static List<object[]> getQuery(string mess)
        {


            List<object[]> rows = new List<object[]>();

            using (SqlConnection conn = new SqlConnection())
            {
                conn.ConnectionString = "SERVER=ps2dbserwer.database.windows.net;DATABASE=sqldb;USER ID=michal7018;PASSWORD=Michal7011;";

                conn.Open();

                SqlCommand command = new SqlCommand(mess, conn);

                if (conn.State == ConnectionState.Closed)
                    conn.Open();


                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        object[] temp = new object[reader.FieldCount];

                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            temp[i] = reader[i];
                        }
                        rows.Add(temp);
                    }
                }
            }
            return rows;
        }


        public override Task OnConnected()
        {
            var name = Context.ConnectionId;
            Debug.WriteLine(name.ToString() + "  connected");

            return base.OnConnected();
        }

        public void getTablesList(string mess, string conID)
        {
            List<object[]> response = getQuery(mess);
            Clients.Client(conID).getTables(response);
        }

        public void ReadSingleTable(string mess, string conID)
        {
            List<object[]> response = getQuery(mess);
            Clients.Client(conID).singleTableResponse(response);
        }


        public void sendQuery(string mess, string conID)
        {
            List<object[]> response = getQuery(mess);
            Clients.Client(conID).getTables(response);
        }

        public static void db_OnChange()
        {
            IHubContext context = GlobalHost.ConnectionManager.GetHubContext<ChatHub>();
            context.Clients.All.refresh();
        }

        public void executeUserCommand(string userID, string query)
        {

            Debug.WriteLine(query);

            using (SqlConnection conn = new SqlConnection())
            {
                conn.ConnectionString = "SERVER=ps2dbserwer.database.windows.net;DATABASE=sqldb;USER ID=michal7018;PASSWORD=Michal7011;";

                conn.Open();

                if (conn.State == ConnectionState.Closed)
                    conn.Open();

                if (query.Contains("select"))
                {
                    List<object[]> response = getQuery(query);

                    Debug.WriteLine(response);
                    Clients.Client(userID).userCommandResponse(response);
                }
                else
                {
                    SqlCommand command = new SqlCommand(query);
                    int result = -1;

                    result = command.ExecuteNonQuery();

                    Clients.Client(userID).clientmess("Error!");
                }
            }
        }


        public void insertRow(string userID, string tableName, string par1, string par2 = null)
        {

            bool refreshFlag = false;

            using (SqlConnection conn = new SqlConnection())
            {
                conn.ConnectionString = "SERVER=ps2dbserwer.database.windows.net;DATABASE=sqldb;USER ID=michal7018;PASSWORD=Michal7011;";

                conn.Open();

                SqlCommand command = null;

                if (conn.State == ConnectionState.Closed)
                    conn.Open();

                switch (tableName)
                {
                    case "Urzytkownicy":
                        command = new SqlCommand("INSERT INTO Urzytkownicy (nazwa_urzytkownika) VALUES (@0)", conn);
                        command.Parameters.Add(new SqlParameter("0", par1));
                        break;

                    case "Produkty":
                        command = new SqlCommand("INSERT INTO Produkty (nazwa_produktu, cena) VALUES (@0, @1)", conn);
                        command.Parameters.Add(new SqlParameter("0", par1));
                        command.Parameters.Add(new SqlParameter("1", par2));

                        break;

                    case "Grupa":
                        command = new SqlCommand("INSERT INTO Grupa (id_urzytkownika, id_produktu) VALUES (@0, @1)", conn);
                        command.Parameters.Add(new SqlParameter("0", par1));
                        command.Parameters.Add(new SqlParameter("1", par2));
                        break;
                }

                int result = -1;

                try
                {
                    if (par1 == "")
                        throw new Exception();
                    result = command.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    Clients.Client(userID).clientmess("Error inserting  data into Database!");
                    refreshFlag = true;
                }

                if (result < 0)
                {
                    Clients.Client(userID).clientmess("Error inserting data into Database!");
                    refreshFlag = true;
                }
            }

            if (!refreshFlag)
                db_OnChange();
        }

        public void deleteRow(string userID, string tableName, string par1 = null)
        {
            bool refreshFlag = false;

            using (SqlConnection conn = new SqlConnection())
            {
                conn.ConnectionString = "SERVER=ps2dbserwer.database.windows.net;DATABASE=sqldb;USER ID=michal7018;PASSWORD=Michal7011;";

                conn.Open();

                SqlCommand command = null;

                if (conn.State == ConnectionState.Closed)
                    conn.Open();

                switch (tableName)
                {
                    case "Urzytkownicy":
                        command = new SqlCommand("DELETE FROM Urzytkownicy WHERE id=(@0)", conn);
                        command.Parameters.Add(new SqlParameter("0", par1));
                        break;

                    case "Produkty":
                        command = new SqlCommand("DELETE FROM Produkty WHERE id=(@0)", conn);
                        command.Parameters.Add(new SqlParameter("0", par1));

                        break;

                    case "Grupa":
                        command = new SqlCommand("DELETE FROM Grupa WHERE id=(@0)", conn);
                        command.Parameters.Add(new SqlParameter("0", par1));
                        break;
                }

                int result = -1;

                try
                {
                    if (par1 == "")
                        throw new Exception();
                    result = command.ExecuteNonQuery();

                }
                catch (Exception e)
                {
                    Clients.Client(userID).clientmess("Error during deleting data into Database!");
                    refreshFlag = true;
                }

                if (result < 0)
                {
                    Clients.Client(userID).clientmess("Error during deleting data into Database!");
                    refreshFlag = true;
                }
            }

            if (!refreshFlag)
                db_OnChange();
        }

        public void editRow(string userID, string tableName, string par1 = null, string par2 = null, string par3 = null)
        {
            bool refreshFlag = false;

            using (SqlConnection conn = new SqlConnection())
            {
                conn.ConnectionString = "SERVER=ps2dbserwer.database.windows.net;DATABASE=sqldb;USER ID=michal7018;PASSWORD=Michal7011;";

                conn.Open();

                SqlCommand command = null;

                if (conn.State == ConnectionState.Closed)
                    conn.Open();

                switch (tableName)
                {
                    case "Urzytkownicy":
                        command = new SqlCommand("UPDATE Urzytkownicy SET nazwa_urzytkownika=(@1) WHERE id=(@0)", conn);
                        command.Parameters.Add(new SqlParameter("0", par1));
                        command.Parameters.Add(new SqlParameter("1", par2));
                        break;

                    case "Produkty":
                        command = new SqlCommand("UPDATE Produkty SET nazwa_produktu=(@1), cena=(@2) WHERE id=(@0)", conn);
                        command.Parameters.Add(new SqlParameter("0", par1));
                        command.Parameters.Add(new SqlParameter("1", par2));
                        command.Parameters.Add(new SqlParameter("2", par3));

                        break;

                    case "Grupa":
                        command = new SqlCommand("UPDATE Grupa SET id_urzytkownika=(@1), id_produktu(@2) WHERE id=(@0)", conn);
                        command.Parameters.Add(new SqlParameter("0", par1));
                        command.Parameters.Add(new SqlParameter("1", par2));
                        command.Parameters.Add(new SqlParameter("2", par3));
                        break;
                }

                int result = -1;

                try
                {
                    if (par1 == "" || par2 == "")
                        throw new Exception();
                    result = command.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    Clients.Client(userID).clientmess("Error");
                    refreshFlag = true;

                }

                if (result < 0)
                {
                    Clients.Client(userID).clientmess("Error");
                    refreshFlag = true;
                }

            }

            if (!refreshFlag)
                db_OnChange();
        }
    }
}