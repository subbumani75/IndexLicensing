using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Standard.Licensing;
using Standard.Licensing.Validation;
using System.Xml;
using System.Data;
using System.IO;
using System.Linq.Expressions;
using Org.BouncyCastle.Asn1.Mozilla;

namespace IndexCadLinkSystem
{
    public class LicenseHandler
    {
        public static string SerialKey = "";
        public static System.Data.DataTable dtLicenseFile = new DataTable();
        public static string UserORMachineID = Environment.MachineName;
        public static bool IsUserSessionActive = false;
        public static bool IsProductRegistered = false;
        public static bool IsLicenseValid = false;

        public static string SerialKeyFilePath = ""; /*Path.Combine(EpicorConstants.IdxLicenseFolderPath, EpicorConstants.IdxSerialKeyFileName);*/
        //public static string RegisterUserFilePath = ""; Path.Combine(EpicorConstants.IdxLicenseFolderPath, EpicorConstants.IdxUserRegistrationFileName);
        public static string RegisterUserFilePath = Path.Combine("C:\\Key\\License33", "IdxLicenseKey.xml");
        public static string LicenseFilePath = "";/*  Path.Combine(EpicorConstants.IdxLicenseFolderPath, EpicorConstants.IdxLicenseKeyFileName);*/

        public static string ValidateLicense()
        {
            string strErrorMessage = ""; string strValue = "";
            //if (!IsProductRegistered) { IsProductRegistered = HasProductRegistered(SerialKeyFilePath); }
            //if (!IsProductRegistered) { strErrorMessage = "Product not registered. Please Contact Administrator"; }
            //if (string.IsNullOrEmpty(strErrorMessage))
            //{
            //    if (!IsLicenseValid) { strValue = IsValidLicense(SerialKey, LicenseFilePath); }
            //    if (strValue != "Valid") { strErrorMessage = strValue; } else { IsLicenseValid = true; }
            //}

            //if (string.IsNullOrEmpty(strErrorMessage))
            //{
            if (!IsUserSessionActive) { IsUserSessionActive = HasSessionActive(RegisterUserFilePath, UserORMachineID); }
                if (!IsUserSessionActive) { strErrorMessage = "User ID " + UserORMachineID + " not registered in the system. Please register using IRegisterMyUser option"; }
            //}

            return strErrorMessage;
        }

        public static string IsValidLicense(string publicKey, string filePath)
        {

            string strValid = "Valid"; string strErrorMessage = ""; string strExceptionMessage = "";
            string currentDeviceIdentifier = ""; // Environment.MachineName;
            string strPath = filePath;
            if (string.IsNullOrEmpty(publicKey))
            {
                MessageBox.Show("Key cannot be empty or null", "Key Generation", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                try
                {
                    License license;

                    using (var xmlReader = new XmlTextReader(strPath))
                    {
                        license = License.Load(xmlReader);
                    }

                    var validationFailures =
                    license.Validate()
                           .ExpirationDate()
                           .And()
                           .Signature(publicKey)
                           .And()
                           .AssertThat(lic => // Check Device Identifier matches.
                               lic.AdditionalAttributes.Get("DeviceIdentifier") == currentDeviceIdentifier,
                               new GeneralValidationFailure()
                               {
                                   Message = "Invalid Device.",
                                   HowToResolve = "Contact the supplier to obtain a new license key."
                               })
                           .AssertValidLicense();

                    System.Threading.Thread.Sleep(2000);
                    try
                    {
                        bool bln = validationFailures.Any();
                    }
                    catch
                    { }
                    foreach (var failure in validationFailures)
                    {
                        strErrorMessage += failure.GetType().Name + ": " + failure.Message + " - " + failure.HowToResolve;
                        //strErrorMessage = validationFailures.First().Message;
                    }

                }
                catch (Exception ex)
                {
                    if (string.IsNullOrEmpty(strErrorMessage))
                    { strExceptionMessage += ex.ToString(); }
                }
                if (!string.IsNullOrEmpty(strExceptionMessage))
                {  //strErrorMessage
                    strValid = strExceptionMessage;
                    //MessageBox.Show("Invalid key passed" + Environment.NewLine + Environment.NewLine + strExceptionMessage + Environment.NewLine + Environment.NewLine + "Please contact vendor for valid key", "License Validation", MessageBoxButtons.OK, MessageBoxIcon.Error);
                } 
                else if (!string.IsNullOrEmpty(strErrorMessage))
                {  //strErrorMessage
                    strValid = strErrorMessage;
                    //MessageBox.Show(strErrorMessage + Environment.NewLine + Environment.NewLine + "", "License Validation", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                
            }

            return strValid;
        }

        private static DataTable GetSerialKeyTable()
        {
            // Create sample Customers table, in order
            // to demonstrate the behavior of the DataTableReader.
            DataTable table = new DataTable();
            table.TableName = "Registration";
            // Create three columns; OrderID, CustomerID, and OrderDate.
            table.Columns.Add(new DataColumn("SerialKey", typeof(System.String)));
            // Set the OrderID column as the primary key column.
            table.PrimaryKey = new DataColumn[] { table.Columns[0] };
            table.AcceptChanges();
            return table;
        }

        private static DataTable GetRegistrationTable()
        {
            // Create sample Customers table, in order
            // to demonstrate the behavior of the DataTableReader.
            DataTable table = new DataTable();
            table.TableName = "Registration";
            // Create three columns; OrderID, CustomerID, and OrderDate.
            table.Columns.Add(new DataColumn("MachineID", typeof(System.String)));
            table.Columns.Add(new DataColumn("RegisteredBy", typeof(System.String)));
            table.Columns.Add(new DataColumn("RegisteredOn", typeof(System.DateTime)));
            table.Columns.Add(new DataColumn("Active", typeof(System.Boolean)));
            table.Columns.Add(new DataColumn("IsSesstionActive", typeof(System.Boolean)));
            table.Columns.Add(new DataColumn("SessionActiveFrom", typeof(System.DateTime)));

            // Set the OrderID column as the primary key column.
            table.PrimaryKey = new DataColumn[] { table.Columns[0] };
            table.AcceptChanges();
            return table;
        }

        public static string RegisterUser(string filePath, string machineID, string registeredBy, bool active, bool isSesstionActive)
        {

            DataTable licenseRegTable= new DataTable() ;
            string strReturn = "Valid"; string strFilter = String.Format("MachineID = '{0}'", machineID);
            //string strFilePath = Path.Combine(EpicorConstants.IdxLicenseFolderPath, EpicorConstants.IdxUserRegistrationFileName);
            bool bAddNewRow = true;
            licenseRegTable = LoadRegisterUser(filePath);           

            if (licenseRegTable != null )
            {
                if (licenseRegTable.Rows.Count > 0)
                {
                    DataRow[] rows = licenseRegTable.Select(strFilter);
                    if (rows != null && rows.Length > 0)
                    {
                        rows[0]["IsSesstionActive"] = isSesstionActive;
                        rows[0]["SessionActiveFrom"] = (isSesstionActive) ? DateTime.Now : new DateTime();
                        licenseRegTable.AcceptChanges();
                        bAddNewRow = false;
                    }
                }
                if (bAddNewRow)
                { 
                    DataRow workRow = licenseRegTable.NewRow();

                    workRow["MachineID"] = Convert.ToString(machineID);
                    workRow["RegisteredBy"] = Convert.ToString(registeredBy);
                    workRow["RegisteredOn"] = DateTime.Now;
                    workRow["Active"] = active;
                    workRow["IsSesstionActive"] = isSesstionActive;
                    workRow["SessionActiveFrom"] = (isSesstionActive) ? DateTime.Now : new DateTime();

                    licenseRegTable.Rows.Add(workRow);
                }
  

                using (StreamWriter fs = new StreamWriter(filePath)) // XML File Path
                {
                    licenseRegTable.WriteXml(fs);
                }

            }

            return strReturn;

        }

        public static string StoreSerialKey(string filePath, string serialKey)
        {

            DataTable licenseRegTable = new DataTable();
            string strReturn = "Valid"; string strFilter = String.Format("SerialKey = '{0}'", serialKey);
            //string strFilePath = SerialKeyFilePath;
            bool bAddNewRow = true;
            licenseRegTable = GetSerialKeyTable();

            if (licenseRegTable != null  )
            {

                if (licenseRegTable.Rows.Count > 0) 
                {
                    DataRow[] rows = licenseRegTable.Select(strFilter);
                    if (rows != null && rows.Length > 0)
                    {
                        rows[0]["serialKey"] = serialKey;
                        licenseRegTable.AcceptChanges();
                        bAddNewRow = false;
                    }
                }                
                if (bAddNewRow)
                {
                    DataRow workRow = licenseRegTable.NewRow();
                    workRow["SerialKey"] = Convert.ToString(serialKey);                 

                    licenseRegTable.Rows.Add(workRow);
                }


                using (StreamWriter fs = new StreamWriter(filePath)) // XML File Path
                {
                    licenseRegTable.WriteXml(fs);
                }

            }

            return strReturn;

        }

        public static DataTable LoadRegisterUser(string filePath)
        {
            //create the DataTable that will hold the data
            DataTable licenseRegTable = GetRegistrationTable();
            try
            {
                //open the file using a Stream
                using (Stream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    //use ReadXml to read the XML stream
                    licenseRegTable.ReadXml(stream);

                    //return the results
                    return licenseRegTable;
                }
            }
            catch (Exception ex)
            {
                return licenseRegTable;
            }


        }

        public static bool HasProductRegistered(string filePath)
        {
           bool bReturn = true;
            if (!File.Exists(filePath))
            { bReturn = false; }
            else
            {
                DataTable dt = GetSerialKeyTable();
                dt.ReadXml(filePath);
                if ( dt != null  && dt.Rows.Count > 0)
                {
                    SerialKey = Convert.ToString(dt.Rows[0][0]);
                }
                if (string.IsNullOrEmpty(SerialKey)) { bReturn = false; }
            }             

            return bReturn;
        }

        public static bool HasSessionActive(string filePath, string machineID)
        {
            bool bReturn = true;
            if (!File.Exists(filePath))
            { bReturn = false; }
            else
            {
                DataTable dt = GetRegistrationTable();
                dt.ReadXml(filePath);
                if (dt != null && dt.Rows.Count > 0)
                {
                    DataRow[] dRow = dt.Select(string.Format ("MachineID = '{0}'" , machineID) );
                    if (dRow !=null && dRow.Length > 0 ) 
                    {
                        IsUserSessionActive = Convert.ToBoolean(dRow[0]["Active"]);
                        if (!IsUserSessionActive) 
                        {
                            RegisterUser(filePath, machineID,"",true,true);
                            IsUserSessionActive = true;
                        }
                    }
                    
                }
                if (!IsUserSessionActive) { bReturn = false; }
            }
             

            return bReturn;
        }

    } //Class
}
