using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using CoffeeShop.Models;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using Microsoft.IdentityModel.Tokens;


namespace CoffeeShop.Controllers
{
    public class CityController : Controller
    {
        private IConfiguration configuration;

        public CityController(IConfiguration _configuration)
        {
            configuration = _configuration;
        }

        #region GETALL
        public IActionResult City()
        {
            string connectionString = this.configuration.GetConnectionString("ConnectionString");
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            SqlCommand command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "[dbo].[PR_City_GetAll]";
            SqlDataReader reader = command.ExecuteReader();
            DataTable table = new DataTable();
            table.Load(reader);
           
            return View(table);
        }
        #endregion

        #region DELETE

        public IActionResult CityDelete(string CityID)
        {
            try
            {
                int decryptedCityID = Convert.ToInt32(UrlEncryptor.Decrypt(CityID.ToString()));
                string connectionString = this.configuration.GetConnectionString("ConnectionString");
                SqlConnection connection = new SqlConnection(connectionString);
                connection.Open();
                SqlCommand command = connection.CreateCommand();
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "[dbo].[PR_City_DeleteByPK]";
                command.Parameters.Add("@CityID", SqlDbType.Int).Value = decryptedCityID;
                command.ExecuteNonQuery();
                connection.Close();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                Console.WriteLine(ex.ToString());
            }
            return RedirectToAction("City");
        }

        #endregion

        #region Add or Edit
        public IActionResult AddEditCity(string? CityID) 
        { 
            CityModel cityModel = new CityModel();

            string connectionString = this.configuration.GetConnectionString("ConnectionString");
            int? decryptedCityID = null;

            // Decrypt only if CityID is not null or empty
            if (!string.IsNullOrEmpty(CityID))
            {
                string decryptedCityIDString = UrlEncryptor.Decrypt(CityID); // Decrypt the encrypted CityID
                decryptedCityID = int.Parse(decryptedCityIDString); // Convert decrypted string to integer
            }
            LoadCountryList();

            #region AddEdit
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            SqlCommand command = connection.CreateCommand();
            command.CommandType= CommandType.StoredProcedure;
            command.CommandText = "[dbo].[PR_City_GetByID]";
            command.Parameters.Add("@CityID", SqlDbType.Int).Value = decryptedCityID ;
            SqlDataReader reader = command.ExecuteReader();
            if (reader.HasRows && CityID != null)
            {
                reader.Read();
                cityModel.CityID = Convert.ToInt32(reader["CityID"]);
                cityModel.StateID = Convert.ToInt32(reader["StateID"]);
                cityModel.CountryID = Convert.ToInt32(reader["CountryID"]);
                cityModel.CityName = reader["CityName"].ToString();
                cityModel.PinCode = reader["PinCode"].ToString();
                ViewBag.StateList = GetStateByCountryID(cityModel.CountryID);

            }
            connection.Close();
            GetStatesByCountry(cityModel.CountryID);
            return View(cityModel);
            #endregion
        }
        #endregion

        

        #region GetStatesByCountry
        [HttpPost]
        public JsonResult GetStatesByCountry(int CountryID)
        {
            List<StateDropDownModel> stateList = GetStateByCountryID(CountryID); 
            return Json(stateList); 
        }
        #endregion

        #region GetStateByCountryID
       
        public List<StateDropDownModel> GetStateByCountryID(int CountryID)
        {
            string connectionString = configuration.GetConnectionString("ConnectionString");
            SqlConnection connection1 = new SqlConnection(connectionString);
            connection1.Open();
            SqlCommand command1 = connection1.CreateCommand();
            command1.CommandType = CommandType.StoredProcedure;
            command1.CommandText = "[dbo].[PR_State_DropDown]";
            command1.Parameters.AddWithValue("@CountryID", CountryID);
            SqlDataReader reader1 = command1.ExecuteReader();
            DataTable dataTable = new DataTable();
            dataTable.Load(reader1);
            List<StateDropDownModel> stateList = new List<StateDropDownModel>();
            foreach (DataRow dataRow in dataTable.Rows)
            {
                StateDropDownModel stateDropDownModel = new StateDropDownModel
                {
                    StateID = Convert.ToInt32(dataRow["StateID"]),
                    StateName = dataRow["StateName"].ToString()
                };
                stateList.Add(stateDropDownModel);
            }
            ViewBag.StateList = stateList;
            connection1.Close();
            return stateList;
        }
        #endregion

        #region LoadCountryList
        private void LoadCountryList()
        {
            string connectionString = configuration.GetConnectionString("ConnectionString");
            SqlConnection connection2 = new SqlConnection(connectionString);
            connection2.Open();
            SqlCommand command2 = connection2.CreateCommand();
            command2.CommandType = CommandType.StoredProcedure;
            command2.CommandText = "[dbo].[PR_Country_DropDown]";
            SqlDataReader reader2 = command2.ExecuteReader();
            DataTable dataTable1 = new DataTable();
            dataTable1.Load(reader2);
            List<CountryDropDownModel> countryList = new List<CountryDropDownModel>();
            foreach (DataRow dataRow in dataTable1.Rows)
            {
                CountryDropDownModel countryDropDownModel = new CountryDropDownModel
                {
                    CountryID = Convert.ToInt32(dataRow["CountryID"]),
                    CountryName = dataRow["CountryName"].ToString()
                };
                countryList.Add(countryDropDownModel);
            }
            ViewBag.CountryList = countryList;
            connection2.Close();

        }
        #endregion

        #region Save
        public IActionResult Save(CityModel cityModel)
        {
            string connectionString = this.configuration.GetConnectionString("ConnectionString");
            SqlConnection connection = new SqlConnection( connectionString);
            connection.Open();
            SqlCommand command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;

            if(cityModel.CityID == null)
            {
                command.CommandText = "[dbo].[PR_City_Insert]";
            }
            else
            {
                command.CommandText = "[dbo].[PR_City_UpdateByPK]";
                command.Parameters.Add("@CityID", SqlDbType.Int).Value = cityModel.CityID;
            }
            command.Parameters.Add("@StateID",SqlDbType.Int).Value = cityModel.StateID;
            command.Parameters.Add("@CountryID", SqlDbType.Int).Value = cityModel.CountryID;
            command.Parameters.Add("@CityName", SqlDbType.VarChar).Value = cityModel.CityName;
            command.Parameters.Add("@PinCode", SqlDbType.VarChar).Value = cityModel.PinCode;
            command.ExecuteNonQuery();

            connection.Close();
            LoadCountryList();
            return RedirectToAction("City",cityModel);



        }
        #endregion

    }
}
